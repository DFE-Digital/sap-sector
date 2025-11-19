module "alerts" {
  source = "git::https://github.com/DFE-Digital/terraform-modules.git//aks/alerts?ref=spike-alerting-module"

  kubernetes_cluster_id         = module.cluster_data.kubernetes_id
  service_name                  = var.service_name
  service_short                 = var.service_short
  azure_resource_prefix         = var.azure_resource_prefix
  config_short                  = var.config_short
  environment                   = var.environment
  azure_enable_app_monitoring   = false
  azure_enable_db_monitoring    = true
  azure_enable_redis_monitoring = true

  app_name                      = module.web_application.name
  db_name                       = module.postgres.azure_server_name
  redis_cache_name              = module.redis-cache.name

  depends_on = [module.web_application, module.postgres, module.redis-cache]
}
