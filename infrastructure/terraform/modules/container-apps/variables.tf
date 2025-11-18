# Variables for Azure Container Apps Module

variable "environment_name" {
  description = "Name of the Container Apps Environment"
  type        = string
}

variable "location" {
  description = "Azure region for resources"
  type        = string
}

variable "resource_group_name" {
  description = "Name of the resource group"
  type        = string
}

variable "log_analytics_workspace_id" {
  description = "ID of the Log Analytics workspace for logging"
  type        = string
}

variable "name_prefix" {
  description = "Prefix for naming Container Apps"
  type        = string
  default     = "mcpworkshop"
}

variable "environment" {
  description = "Environment name (Development, Staging, Production)"
  type        = string
  default     = "Development"
}

variable "min_replicas" {
  description = "Minimum number of replicas for auto-scaling"
  type        = number
  default     = 1
}

variable "max_replicas" {
  description = "Maximum number of replicas for auto-scaling"
  type        = number
  default     = 3
}

variable "tags" {
  description = "Tags to apply to all resources"
  type        = map(string)
  default     = {}
}

# SQL MCP Server Configuration
variable "enable_sql_server" {
  description = "Enable SQL MCP Server deployment"
  type        = bool
  default     = true
}

variable "sql_server_image" {
  description = "Container image for SQL MCP Server"
  type        = string
  default     = "mcpworkshop.azurecr.io/sql-mcp-server:latest"
}

variable "sql_connection_string" {
  description = "Connection string for Azure SQL Database"
  type        = string
  sensitive   = true
}

# Cosmos MCP Server Configuration
variable "enable_cosmos_server" {
  description = "Enable Cosmos MCP Server deployment"
  type        = bool
  default     = true
}

variable "cosmos_server_image" {
  description = "Container image for Cosmos MCP Server"
  type        = string
  default     = "mcpworkshop.azurecr.io/cosmos-mcp-server:latest"
}

variable "cosmos_endpoint" {
  description = "Azure Cosmos DB endpoint URL"
  type        = string
}

variable "cosmos_key" {
  description = "Azure Cosmos DB primary key"
  type        = string
  sensitive   = true
}

variable "cosmos_database_id" {
  description = "Cosmos DB database ID"
  type        = string
  default     = "mcpworkshop"
}

# REST MCP Server Configuration
variable "enable_rest_server" {
  description = "Enable REST API MCP Server deployment"
  type        = bool
  default     = true
}

variable "rest_server_image" {
  description = "Container image for REST MCP Server"
  type        = string
  default     = "mcpworkshop.azurecr.io/rest-mcp-server:latest"
}

variable "external_api_base_url" {
  description = "Base URL for external API calls"
  type        = string
  default     = "https://api.example.com"
}

# Virtual Analyst Configuration
variable "enable_virtual_analyst" {
  description = "Enable Virtual Analyst Orchestrator deployment"
  type        = bool
  default     = true
}

variable "virtual_analyst_image" {
  description = "Container image for Virtual Analyst"
  type        = string
  default     = "mcpworkshop.azurecr.io/virtual-analyst:latest"
}

# Authentication Configuration
variable "jwt_secret" {
  description = "JWT secret key for authentication"
  type        = string
  sensitive   = true
}

variable "jwt_issuer" {
  description = "JWT issuer URL"
  type        = string
}

variable "jwt_audience" {
  description = "JWT audience identifier"
  type        = string
}

# Monitoring Configuration
variable "app_insights_connection_string" {
  description = "Application Insights connection string"
  type        = string
  sensitive   = true
}
