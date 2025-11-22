# Variables for Azure SQL Database Module

variable "server_name" {
  description = "Name of the Azure SQL Server"
  type        = string
}

variable "database_name" {
  description = "Name of the SQL Database"
  type        = string
  default     = "mcpworkshop"
}

variable "resource_group_name" {
  description = "Name of the resource group"
  type        = string
}

variable "location" {
  description = "Azure region for resources"
  type        = string
}

variable "admin_username" {
  description = "SQL Server administrator username"
  type        = string
  default     = "mcpadmin"
}

variable "admin_password" {
  description = "SQL Server administrator password"
  type        = string
  sensitive   = true
}

variable "azuread_admin_login" {
  description = "Azure AD administrator login name (optional)"
  type        = string
  default     = null
}

variable "azuread_admin_object_id" {
  description = "Azure AD administrator object ID (optional)"
  type        = string
  default     = null
}

variable "enable_public_access" {
  description = "Enable public network access to SQL Server"
  type        = bool
  default     = false
}

variable "max_size_gb" {
  description = "Maximum size of the database in gigabytes"
  type        = number
  default     = 2
}

variable "sku_name" {
  description = "SKU name for the database (e.g., Basic, S0, P1)"
  type        = string
  default     = "S0"
}

variable "zone_redundant" {
  description = "Enable zone redundancy for high availability"
  type        = bool
  default     = false
}

variable "workshop_ip_range" {
  description = "IP range for workshop attendee access"
  type = object({
    start = string
    end   = string
  })
  default = null
}

variable "enable_auditing" {
  description = "Enable SQL Server auditing"
  type        = bool
  default     = false
}

variable "auditing_storage_endpoint" {
  description = "Storage endpoint for audit logs"
  type        = string
  default     = ""
}

variable "auditing_storage_key" {
  description = "Storage account key for audit logs"
  type        = string
  sensitive   = true
  default     = ""
}

variable "audit_retention_days" {
  description = "Number of days to retain audit logs"
  type        = number
  default     = 30
}

variable "enable_threat_protection" {
  description = "Enable Advanced Threat Protection"
  type        = bool
  default     = false
}

variable "threat_protection_email_admins" {
  description = "Send threat alerts to subscription admins"
  type        = bool
  default     = false
}

variable "threat_protection_emails" {
  description = "List of emails to receive threat alerts"
  type        = list(string)
  default     = []
}

variable "seed_sample_data" {
  description = "Seed database with sample data for exercises"
  type        = bool
  default     = true
}

variable "tags" {
  description = "Tags to apply to all resources"
  type        = map(string)
  default     = {}
}
