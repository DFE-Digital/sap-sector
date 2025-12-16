module "storage" {
  source = "git::https://github.com/DFE-Digital/terraform-modules.git//aks/storage_account?ref=stable"

  name = "dp"

  environment                   = var.environment
  azure_resource_prefix         = var.azure_resource_prefix
  service_short                 = var.service_short
  config_short                  = var.config_short
  public_network_access_enabled = true

  # Create containers for the application (all containers are private)
  containers = [
    { name = "keys" }
  ]

  # Configure blob lifecycle management (default: delete after 7 days)
  container_delete_retention_days = var.storage_container_delete_retention_days
  blob_delete_after_days          = 90

  # Enable infrastructure encryption for additional security
  infrastructure_encryption_enabled = true

  # Create a Microsoft-managed encryption scope
  create_encryption_scope = false
}