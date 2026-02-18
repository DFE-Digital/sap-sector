locals {
  environment = var.environment != "" ? var.environment : var.cluster

  dsi_environment = contains(["production", "prod", "pd"], local.environment) ? "production" : "test"

  dsi_config = {
    test = {
      api_uri          = "https://test-api.signin.education.gov.uk"
      authority        = "https://test-oidc.signin.education.gov.uk"
      issuer           = "https://test-oidc.signin.education.gov.uk"
      metadata_address = "https://test-oidc.signin.education.gov.uk/.well-known/openid-configuration"
      require_https    = "true"
    }
    production = {
      api_uri          = "https://api.signin.education.gov.uk"
      authority        = "https://oidc.signin.education.gov.uk"
      issuer           = "https://oidc.signin.education.gov.uk"
      metadata_address = "https://oidc.signin.education.gov.uk/.well-known/openid-configuration"
      require_https    = "true"
    }
  }

  dsi_urls = local.dsi_config[local.dsi_environment]

  key_vault_name = "${var.azure_resource_prefix}-${var.service_short}-${var.config_short}-app-kv"
  
  resource_group_name = "${var.azure_resource_prefix}-${var.service_short}-${var.config_short}-rg"
}
