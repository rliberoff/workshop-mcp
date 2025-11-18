# Variables for Azure Cosmos DB Module

variable "account_name" {
  description = "Name of the Cosmos DB account"
  type        = string
}

variable "database_name" {
  description = "Name of the Cosmos DB database"
  type        = string
  default     = "mcpworkshop"
}

variable "resource_group_name" {
  description = "Name of the resource group"
  type        = string
}

variable "location" {
  description = "Azure region for primary location"
  type        = string
}

variable "consistency_level" {
  description = "Consistency level (Strong, BoundedStaleness, Session, Eventual, ConsistentPrefix)"
  type        = string
  default     = "Session"

  validation {
    condition     = contains(["Strong", "BoundedStaleness", "Session", "Eventual", "ConsistentPrefix"], var.consistency_level)
    error_message = "Consistency level must be one of: Strong, BoundedStaleness, Session, Eventual, ConsistentPrefix"
  }
}

variable "secondary_locations" {
  description = "List of secondary geo-locations for multi-region replication"
  type = list(object({
    location          = string
    failover_priority = number
  }))
  default = []
}

variable "cart_events_ttl_seconds" {
  description = "Time-to-live for cart events in seconds (default: 30 days)"
  type        = number
  default     = 2592000 # 30 days
}

variable "seed_sample_data" {
  description = "Seed Cosmos DB with sample data for exercises"
  type        = bool
  default     = true
}

variable "tags" {
  description = "Tags to apply to all resources"
  type        = map(string)
  default     = {}
}
