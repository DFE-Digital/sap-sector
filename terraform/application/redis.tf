module "redis-cache" {
  source = "git::https://github.com/DFE-Digital/terraform-modules.git//aks/redis?ref=spike-alerting-module"

  namespace                 = var.namespace
  environment               = var.environment
  azure_resource_prefix     = var.azure_resource_prefix
  service_short             = var.service_short
  config_short              = var.config_short
  service_name              = var.service_name
  cluster_configuration_map = module.cluster_data.configuration_map
  use_azure                 = var.deploy_azure_backing_services
  azure_enable_monitoring   = var.enable_monitoring
  azure_patch_schedule      = [{ "day_of_week" : "Sunday", "start_hour_utc" : 01 }]
  server_version            = "6"
}