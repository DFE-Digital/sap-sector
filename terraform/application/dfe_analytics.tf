provider "google" {
  project = "school-standards"
}

module "dfe_analytics" {
  count  = local.dfe_analytics_enabled ? 1 : 0
  source = "./vendor/modules/aks//aks/dfe_analytics"

  azure_resource_prefix = var.azure_resource_prefix
  cluster               = var.cluster
  namespace             = var.namespace
  service_short         = var.service_short
  environment           = local.environment

  gcp_keyring       = "school-standards-key-ring"
  gcp_key           = "school-standards-key"
  gcp_taxonomy_id   = "9218536955874377223"
  gcp_policy_tag_id = "1991951892101805780"
}
