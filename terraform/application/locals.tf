locals {
  environment = var.environment != "" ? var.environment : var.cluster

  key_vault_name = "${var.azure_resource_prefix}-${var.service_short}-${var.config_short}-app-kv"

  resource_group_name = "${var.azure_resource_prefix}-${var.service_short}-${var.config_short}-rg"

  dfe_analytics_enabled = var.enable_dfe_analytics_federated_auth

  federated_auth_configmap = local.dfe_analytics_enabled ? {
    DfeAnalytics__Environment = local.environment
    DfeAnalytics__ProjectId   = module.dfe_analytics[0].bigquery_project_id
    DfeAnalytics__DatasetId   = module.dfe_analytics[0].bigquery_dataset
    DfeAnalytics__TableId     = module.dfe_analytics[0].bigquery_table_name
  } : {}

  federated_auth_secrets = local.dfe_analytics_enabled ? {
    DfeAnalytics__CredentialsJson = module.dfe_analytics[0].google_cloud_credentials
  } : {}
}
