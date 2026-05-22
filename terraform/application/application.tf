data "azurerm_key_vault" "app_key_vault" {
  name                = local.key_vault_name
  resource_group_name = local.resource_group_name
}

data "azurerm_key_vault_secret" "dsi_client_secret" {
  name         = "DsiClientSecret"
  key_vault_id = data.azurerm_key_vault.app_key_vault.id
}

data "azurerm_key_vault_secret" "dsi_api_secret" {
  name         = "DsiApiSecret"
  key_vault_id = data.azurerm_key_vault.app_key_vault.id
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

  config_variables = merge({
    ENVIRONMENT_NAME = var.environment
    PGSSLMODE        = local.postgres_ssl_mode
  }, local.federated_auth_configmap)
  secret_variables = merge({
    DATABASE_URL                                = module.postgres.url
    ConnectionStrings__PostgresConnectionString = module.postgres.dotnet_connection_string
    DsiConfiguration__ClientSecret              = data.azurerm_key_vault_secret.dsi_client_secret.value
    DsiConfiguration__ApiSecret                 = data.azurerm_key_vault_secret.dsi_api_secret.value
    StorageConnectionString                     = "DefaultEndpointsProtocol=https;AccountName=${module.storage.name};AccountKey=${module.storage.primary_access_key}"
  }, local.federated_auth_secrets)
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
  enable_gcp_wif                   = var.enable_dfe_analytics_federated_auth
}
