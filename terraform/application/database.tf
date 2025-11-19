module "postgres" {
  source = "git::https://github.com/DFE-Digital/terraform-modules.git//aks/postgres?ref=spike-alerting-module"

  namespace                      = var.namespace
  environment                    = var.environment
  azure_resource_prefix          = var.azure_resource_prefix
  service_name                   = var.service_name
  service_short                  = var.service_short
  config_short                   = var.config_short
  cluster_configuration_map      = module.cluster_data.configuration_map
  use_azure                      = var.deploy_azure_backing_services
  azure_enable_monitoring        = var.enable_monitoring
  azure_sku_name                 = var.postgres_flexible_server_sku
  azure_enable_high_availability = var.enable_postgres_high_availability
  azure_enable_backup_storage    = var.enable_postgres_backup_storage
  azure_maintenance_window       = var.azure_maintenance_window
  server_version                 = "16"
}

