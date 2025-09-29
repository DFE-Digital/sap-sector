terraform {
  backend "azurerm" {
    resource_group_name  = ""   # change for your RG
    storage_account_name = ""       # must be globally unique
    container_name       = ""
    key                  = ""
  }
}