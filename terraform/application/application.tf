data "azurerm_key_vault" "app_key_vault" {
  name                = local.key_vault_name
  resource_group_name = local.resource_group_name
}

# Fetch DSI secrets from Key Vault
data "azurerm_key_vault_secret" "dsi_client_id" {
  name         = "DsiClientId"
  key_vault_id = data.azurerm_key_vault.app_key_vault.id
}

data "azurerm_key_vault_secret" "dsi_client_secret" {
  name         = "DsiClientSecret"
  key_vault_id = data.azurerm_key_vault.app_key_vault.id
}

data "azurerm_key_vault_secret" "dsi_api_secret" {
  name         = "DsiApiSecret"
  key_vault_id = data.azurerm_key_vault.app_key_vault.id
}

data "azurerm_key_vault_secret" "dsi_service_id" {
  name         = "DsiServiceId"
  key_vault_id = data.azurerm_key_vault.app_key_vault.id
}

data "azurerm_key_vault_secret" "sign_in_url" {
  name         = "SignInUri"
  key_vault_id = data.azurerm_key_vault.app_key_vault.id
}

data "azurerm_key_vault_secret" "help_uri" {
  name         = "HelpUri"
  key_vault_id = data.azurerm_key_vault.app_key_vault.id
}

data "azurerm_key_vault_secret" "logit_http_url" {
  name         = "LogitHttpUrl"
  key_vault_id = data.azurerm_key_vault.app_key_vault.id
}

data "azurerm_key_vault_secret" "logit_api_key" {
  name         = "LogitApiKey"
  key_vault_id = data.azurerm_key_vault.app_key_vault.id
}

# Create storage account for shared data protection keys
resource "azurerm_storage_account" "dataprotection" {
  name                     = "${var.azure_resource_prefix}${var.service_short}keys${var.environment}"
  resource_group_name      = local.resource_group_name
  location                 = var.azure_location
  account_tier             = "Standard"
  account_replication_type = "LRS"
  min_tls_version          = "TLS1_2"
  
  # Security
  allow_nested_items_to_be_public = false
  
  tags = {
    Environment = var.environment
    Purpose     = "DataProtection"
  }
}

resource "azurerm_storage_share" "dataprotection" {
  name                 = "dataprotection-keys"
  storage_account_name = azurerm_storage_account.dataprotection.name
  quota                = 1 
}

# Create Kubernetes secret with storage credentials
resource "kubernetes_secret" "dataprotection_storage" {
  metadata {
    name      = "dataprotection-storage"
    namespace = var.namespace
  }

  data = {
    azurestorageaccountname = azurerm_storage_account.dataprotection.name
    azurestorageaccountkey  = azurerm_storage_account.dataprotection.primary_access_key
  }
}

module "application_configuration" {
  source = "./vendor/modules/aks//aks/application_configuration"

  namespace              = var.namespace
  environment            = var.environment
  azure_resource_prefix  = var.azure_resource_prefix
  service_short          = var.service_short
  config_short           = var.config_short
  secret_key_vault_short = "app"
  config_variables_path  = "${path.module}/config/${var.config}.yml"

  # Delete for non rails apps
  is_rails_application = true

  config_variables = {
    ENVIRONMENT_NAME = var.environment
    PGSSLMODE        = local.postgres_ssl_mode

    DsiConfiguration__ServiceName           = "SAP Sector Service"
    DsiConfiguration__ApiUri                = local.dsi_urls.api_uri
    DsiConfiguration__Authority             = local.dsi_urls.authority
    DsiConfiguration__Issuer                = local.dsi_urls.issuer
    DsiConfiguration__Audience              = "SAP"
    DsiConfiguration__MetadataAddress       = local.dsi_urls.metadata_address
    DsiConfiguration__CallbackPath          = "/signin-oidc"
    DsiConfiguration__SignedOutCallbackPath = "/signout-callback-oidc"
    DsiConfiguration__RequireHttpsMetadata  = local.dsi_urls.require_https
    DsiConfiguration__ValidateIssuer        = "true"
    DsiConfiguration__ValidateAudience      = "true"
    DsiConfiguration__ValidateLifetime      = "true"
    DsiConfiguration__TokenExpiryMinutes    = "60"
  }
  secret_variables = {
    DATABASE_URL = module.postgres.url
    DsiConfiguration__ClientId     = data.azurerm_key_vault_secret.dsi_client_id.value
    DsiConfiguration__ClientSecret = data.azurerm_key_vault_secret.dsi_client_secret.value
    DsiConfiguration__ApiSecret    = data.azurerm_key_vault_secret.dsi_api_secret.value
    DsiConfiguration__ServiceId    = data.azurerm_key_vault_secret.dsi_service_id.value
    DFESignInSettings__SignInUri   = data.azurerm_key_vault_secret.sign_in_url.value
    DFESignInSettings__HelpUri     = data.azurerm_key_vault_secret.help_uri.value
    LOGIT_HTTP_URL = data.azurerm_key_vault_secret.logit_http_url.value
    LOGIT_API_KEY  = data.azurerm_key_vault_secret.logit_api_key.value
  }
}

module "web_application" {
  source = "./vendor/modules/aks//aks/application"

  is_web = true

  namespace    = var.namespace
  environment  = var.environment
  service_name = var.service_name

  cluster_configuration_map  = module.cluster_data.configuration_map
  kubernetes_config_map_name = module.application_configuration.kubernetes_config_map_name
  kubernetes_secret_name     = module.application_configuration.kubernetes_secret_name

  docker_image = var.docker_image
  enable_logit = true
  replicas     = var.replicas

  send_traffic_to_maintenance_page = var.send_traffic_to_maintenance_page

  volumes = [
    {
      name = "dataprotection-keys"
      azure_file = {
        secret_name = kubernetes_secret.dataprotection_storage.metadata[0].name
        share_name  = azurerm_storage_share.dataprotection.name
        read_only   = false
      }
    }
  ]
  
  volume_mounts = [
    {
      name       = "dataprotection-keys"
      mount_path = "/mnt/dataprotection"
      read_only  = false
    }
  ]
}
