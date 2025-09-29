# Core
variable "environment" {
  type        = string
  description = "Environment name (e.g. dev, test, prod)"
}
variable "project_name" {
  type        = string
  description = "Project/application name"
}
variable "azure_location" {
  type        = string
  description = "Azure region for resources"
}
variable "tags" {
  type        = map(string)
  description = "Common tags to apply to resources"
  default     = {}
}

# Container Registry
variable "enable_container_registry" {
  type        = bool
  default     = true
}
variable "registry_admin_enabled" {
  type        = bool
  default     = false
}
variable "registry_use_managed_identity" {
  type        = bool
  default     = true
}
variable "registry_managed_identity_assign_role" {
  type        = bool
  default     = true
}
variable "registry_server" {
  type        = string
}
variable "image_name" {
  type        = string
}

# Container App
variable "container_command" {
  type        = list(string)
  default     = []
}
variable "container_secret_environment_variables" {
  type        = map(string)
  default     = {}
}
variable "container_health_probe_path" {
  type        = string
  default     = "/health"
}
variable "container_cpu" {
  type        = number
  default     = 1
}
variable "container_memory" {
  type        = number
  default     = 1
}
variable "container_min_replicas" {
  type        = number
  default     = 1
}
variable "container_max_replicas" {
  type        = number
  default     = 1
}
variable "container_port" {
  type        = number
  default     = 8080
}
variable "container_scale_http_concurrency" {
  type        = number
  default     = 50
}
variable "enable_container_health_probe" {
  type        = bool
  default     = true
}
variable "container_health_probe_protocol" {
  type        = string
  default     = "HTTP"
}
variable "container_apps_allow_ips_inbound" {
  type        = list(string)
  default     = []
}

# Redis (optional)
variable "enable_redis_cache" {
  type        = bool
  default     = false
}
variable "redis_cache_sku" {
  type        = string
  default     = "Basic"
}
variable "redis_cache_capacity" {
  type        = number
  default     = 0
}

# CDN & DNS
variable "enable_cdn_frontdoor" {
  type        = bool
  default     = false
}
variable "enable_dns_zone" {
  type        = bool
  default     = false
}
variable "dns_zone_domain_name" {
  type        = string
  default     = ""
}
variable "dns_ns_records" {
  type        = list(string)
  default     = []
}
variable "dns_txt_records" {
  type        = list(string)
  default     = []
}
variable "dns_mx_records" {
  type        = list(string)
  default     = []
}
variable "dns_alias_records" {
  type        = list(string)
  default     = []
}

variable "cdn_frontdoor_custom_domains" {
  type        = list(string)
  default     = []
}
variable "cdn_frontdoor_host_redirects" {
  type        = map(string)
  default     = {}
}
variable "cdn_frontdoor_host_add_response_headers" {
  type        = map(string)
  default     = {}
}
variable "cdn_frontdoor_health_probe_path" {
  type        = string
  default     = "/"
}
variable "cdn_frontdoor_enable_rate_limiting" {
  type        = bool
  default     = false
}
variable "cdn_frontdoor_waf_custom_rules" {
  type        = list(any)
  default     = []
}
variable "cdn_frontdoor_rate_limiting_duration_in_minutes" {
  type        = number
  default     = 1
}
variable "cdn_frontdoor_rate_limiting_threshold" {
  type        = number
  default     = 100
}
variable "cdn_frontdoor_origin_fqdn_override" {
  type        = string
  default     = ""
}
variable "cdn_frontdoor_origin_host_header_override" {
  type        = string
  default     = ""
}
variable "cdn_frontdoor_forwarding_protocol" {
  type        = string
  default     = "HttpsOnly"
}
variable "enable_cdn_frontdoor_health_probe" {
  type        = bool
  default     = false
}
variable "enable_cdn_frontdoor_vdp_redirects" {
  type        = bool
  default     = false
}
variable "cdn_frontdoor_vdp_destination_hostname" {
  type        = string
  default     = ""
}

# Monitoring
variable "enable_monitoring" {
  type        = bool
  default     = true
}
variable "monitor_email_receivers" {
  type        = list(string)
  default     = []
}
variable "monitor_endpoint_healthcheck" {
  type        = string
  default     = ""
}
variable "enable_monitoring_traces" {
  type        = bool
  default     = false
}
variable "monitor_http_availability_fqdn" {
  type        = string
  default     = ""
}

# Event Hub / Logging
variable "enable_event_hub" {
  type        = bool
  default     = false
}
variable "enable_logstash_consumer" {
  type        = bool
  default     = false
}
variable "eventhub_export_log_analytics_table_names" {
  type        = list(string)
  default     = []
}

# Key Vault & Networking
variable "existing_logic_app_workflow" {
  type        = string
  default     = ""
}
variable "existing_network_watcher_name" {
  type        = string
  default     = ""
}
variable "existing_network_watcher_resource_group_name" {
  type        = string
  default     = ""
}
variable "key_vault_access_ipv4" {
  type        = list(string)
  default     = []
}

# StatusCake
variable "statuscake_monitored_resource_addresses" {
  type        = list(string)
  default     = []
}
variable "statuscake_contact_group_name" {
  type        = string
  default     = ""
}
variable "statuscake_contact_group_integrations" {
  type        = list(string)
  default     = []
}
variable "statuscake_contact_group_email_addresses" {
  type        = list(string)
  default     = []
}

# Storage / File Shares
variable "enable_container_app_file_share" {
  type        = bool
  default     = false
}
variable "storage_account_ipv4_allow_list" {
  type        = list(string)
  default     = []
}
variable "storage_account_public_access_enabled" {
  type        = bool
  default     = false
}

# Health Insights API
variable "enable_health_insights_api" {
  type        = bool
  default     = false
}
variable "health_insights_api_cors_origins" {
  type        = list(string)
  default     = []
}
variable "health_insights_api_ipv4_allow_list" {
  type        = list(string)
  default     = []
}

# Init container
variable "enable_init_container" {
  type        = bool
  default     = false
}
variable "init_container_image" {
  type        = string
  default     = ""
}
variable "init_container_command" {
  type        = list(string)
  default     = []
}

# Misc
variable "tfvars_filename" {
  type        = string
  default     = "dev.tfvars"
}
