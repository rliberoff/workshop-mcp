# Azure Container Apps Module for MCP Workshop
# Purpose: Host MCP servers in managed Kubernetes environment
# Based on: research.md section 2 - Azure Container Apps as primary hosting platform

# Container Apps Environment (shared by all MCP servers)
resource "azurerm_container_app_environment" "mcp_workshop" {
  name                       = var.environment_name
  location                   = var.location
  resource_group_name        = var.resource_group_name
  log_analytics_workspace_id = var.log_analytics_workspace_id

  tags = merge(
    var.tags,
    {
      Purpose = "MCP Workshop - Managed Kubernetes Environment"
      Module  = "container-apps"
    }
  )
}

# Container App for Exercise 4: SQL MCP Server
resource "azurerm_container_app" "sql_mcp_server" {
  count                        = var.enable_sql_server ? 1 : 0
  name                         = "${var.name_prefix}-sql-mcp"
  container_app_environment_id = azurerm_container_app_environment.mcp_workshop.id
  resource_group_name          = var.resource_group_name
  revision_mode                = "Single"

  template {
    min_replicas = var.min_replicas
    max_replicas = var.max_replicas

    container {
      name   = "sql-mcp-server"
      image  = var.sql_server_image
      cpu    = 0.5
      memory = "1Gi"

      env {
        name  = "ASPNETCORE_ENVIRONMENT"
        value = var.environment
      }

      env {
        name  = "SQL_CONNECTION_STRING"
        value = var.sql_connection_string
      }

      env {
        name  = "APPLICATIONINSIGHTS_CONNECTION_STRING"
        value = var.app_insights_connection_string
      }

      env {
        name  = "JWT_SECRET"
        value = var.jwt_secret
      }

      env {
        name  = "JWT_ISSUER"
        value = var.jwt_issuer
      }

      env {
        name  = "JWT_AUDIENCE"
        value = var.jwt_audience
      }
    }

    http_scale_rule {
      name                = "http-requests"
      concurrent_requests = 10
    }
  }

  ingress {
    external_enabled = true
    target_port      = 5010
    transport        = "http"

    traffic_weight {
      latest_revision = true
      percentage      = 100
    }
  }

  tags = merge(
    var.tags,
    {
      Exercise   = "Exercise4"
      ServerType = "SQL"
    }
  )
}

# Container App for Exercise 4: Cosmos MCP Server
resource "azurerm_container_app" "cosmos_mcp_server" {
  count                        = var.enable_cosmos_server ? 1 : 0
  name                         = "${var.name_prefix}-cosmos-mcp"
  container_app_environment_id = azurerm_container_app_environment.mcp_workshop.id
  resource_group_name          = var.resource_group_name
  revision_mode                = "Single"

  template {
    min_replicas = var.min_replicas
    max_replicas = var.max_replicas

    container {
      name   = "cosmos-mcp-server"
      image  = var.cosmos_server_image
      cpu    = 0.5
      memory = "1Gi"

      env {
        name  = "ASPNETCORE_ENVIRONMENT"
        value = var.environment
      }

      env {
        name  = "COSMOS_ENDPOINT"
        value = var.cosmos_endpoint
      }

      env {
        name  = "COSMOS_KEY"
        value = var.cosmos_key
      }

      env {
        name  = "COSMOS_DATABASE_ID"
        value = var.cosmos_database_id
      }

      env {
        name  = "APPLICATIONINSIGHTS_CONNECTION_STRING"
        value = var.app_insights_connection_string
      }

      env {
        name  = "JWT_SECRET"
        value = var.jwt_secret
      }

      env {
        name  = "JWT_ISSUER"
        value = var.jwt_issuer
      }

      env {
        name  = "JWT_AUDIENCE"
        value = var.jwt_audience
      }
    }

    http_scale_rule {
      name                = "http-requests"
      concurrent_requests = 10
    }
  }

  ingress {
    external_enabled = true
    target_port      = 5011
    transport        = "http"

    traffic_weight {
      latest_revision = true
      percentage      = 100
    }
  }

  tags = merge(
    var.tags,
    {
      Exercise   = "Exercise4"
      ServerType = "Cosmos"
    }
  )
}

# Container App for Exercise 4: REST API MCP Server
resource "azurerm_container_app" "rest_mcp_server" {
  count                        = var.enable_rest_server ? 1 : 0
  name                         = "${var.name_prefix}-rest-mcp"
  container_app_environment_id = azurerm_container_app_environment.mcp_workshop.id
  resource_group_name          = var.resource_group_name
  revision_mode                = "Single"

  template {
    min_replicas = var.min_replicas
    max_replicas = var.max_replicas

    container {
      name   = "rest-mcp-server"
      image  = var.rest_server_image
      cpu    = 0.5
      memory = "1Gi"

      env {
        name  = "ASPNETCORE_ENVIRONMENT"
        value = var.environment
      }

      env {
        name  = "EXTERNAL_API_BASE_URL"
        value = var.external_api_base_url
      }

      env {
        name  = "APPLICATIONINSIGHTS_CONNECTION_STRING"
        value = var.app_insights_connection_string
      }

      env {
        name  = "JWT_SECRET"
        value = var.jwt_secret
      }

      env {
        name  = "JWT_ISSUER"
        value = var.jwt_issuer
      }

      env {
        name  = "JWT_AUDIENCE"
        value = var.jwt_audience
      }
    }

    http_scale_rule {
      name                = "http-requests"
      concurrent_requests = 10
    }
  }

  ingress {
    external_enabled = true
    target_port      = 5012
    transport        = "http"

    traffic_weight {
      latest_revision = true
      percentage      = 100
    }
  }

  tags = merge(
    var.tags,
    {
      Exercise   = "Exercise4"
      ServerType = "REST"
    }
  )
}

# Container App for Exercise 4: Virtual Analyst Orchestrator
resource "azurerm_container_app" "virtual_analyst" {
  count                        = var.enable_virtual_analyst ? 1 : 0
  name                         = "${var.name_prefix}-virtual-analyst"
  container_app_environment_id = azurerm_container_app_environment.mcp_workshop.id
  resource_group_name          = var.resource_group_name
  revision_mode                = "Single"

  template {
    min_replicas = var.min_replicas
    max_replicas = var.max_replicas

    container {
      name   = "virtual-analyst"
      image  = var.virtual_analyst_image
      cpu    = 1.0
      memory = "2Gi"

      env {
        name  = "ASPNETCORE_ENVIRONMENT"
        value = var.environment
      }

      env {
        name  = "SQL_MCP_SERVER_URL"
        value = var.enable_sql_server ? azurerm_container_app.sql_mcp_server[0].latest_revision_fqdn : ""
      }

      env {
        name  = "COSMOS_MCP_SERVER_URL"
        value = var.enable_cosmos_server ? azurerm_container_app.cosmos_mcp_server[0].latest_revision_fqdn : ""
      }

      env {
        name  = "REST_MCP_SERVER_URL"
        value = var.enable_rest_server ? azurerm_container_app.rest_mcp_server[0].latest_revision_fqdn : ""
      }

      env {
        name  = "APPLICATIONINSIGHTS_CONNECTION_STRING"
        value = var.app_insights_connection_string
      }

      env {
        name  = "JWT_SECRET"
        value = var.jwt_secret
      }

      env {
        name  = "JWT_ISSUER"
        value = var.jwt_issuer
      }

      env {
        name  = "JWT_AUDIENCE"
        value = var.jwt_audience
      }

      env {
        name  = "CACHE_ENABLED"
        value = "true"
      }

      env {
        name  = "CACHE_TTL_MINUTES"
        value = "5"
      }
    }

    http_scale_rule {
      name                = "http-requests"
      concurrent_requests = 20
    }
  }

  ingress {
    external_enabled = true
    target_port      = 5004
    transport        = "http"

    traffic_weight {
      latest_revision = true
      percentage      = 100
    }
  }

  tags = merge(
    var.tags,
    {
      Exercise   = "Exercise4"
      ServerType = "Orchestrator"
    }
  )
}
