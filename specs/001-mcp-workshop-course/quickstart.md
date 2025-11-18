# Quick Start: MCP Workshop Setup

**Workshop**: MCP Workshop Course  
**Date**: 2025-11-17  
**Time Required**: 30-45 minutes  
**Target Audience**: Workshop attendees and instructors

## Overview

This guide will help you set up your development environment for the MCP workshop. By the end, you'll have:

- ✅ .NET SDK installed and configured
- ✅ Required development tools (IDE, Git, PowerShell)
- ✅ Workshop repository cloned
- ✅ ModelContextProtocol library installed
- ✅ Sample data and local MCP server running
- ✅ Azure CLI configured (optional, for cloud exercises)

---

## Prerequisites

### Required

- **Operating System**: Windows 11
- **RAM**: Minimum 8 GB (16 GB recommended)
- **Disk Space**: 5 GB free space
- **Internet**: Required for initial setup and package downloads

### Accounts (Optional for Azure Exercises)

- **Azure Account**: Free tier sufficient ([azure.microsoft.com/free](https://azure.microsoft.com/free))
- **GitHub Account**: For repository access (if using private fork)

---

## Step 1: Install .NET SDK

### Windows

1. Download .NET 10.0 SDK from [dotnet.microsoft.com/download](https://dotnet.microsoft.com/download)
2. Run installer and follow wizard
3. Verify installation:

```powershell
dotnet --version
# Expected output: 10.0.x or higher
```

### macOS

```bash
# Using Homebrew
brew install dotnet-sdk

# Verify
dotnet --version
```

### Linux (Ubuntu/Debian)

```bash
# Add Microsoft package repository
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

# Install SDK
sudo apt-get update
sudo apt-get install -y dotnet-sdk-10.0

# Verify
dotnet --version
```

---

## Step 2: Install Development Tools

### IDE Options

#### Option A: Visual Studio Code (Recommended)**

1. Download from [code.visualstudio.com](https://code.visualstudio.com)
2. Install required extensions:
    - C# Dev Kit (Microsoft)
    - REST Client (Huachao Mao)
    - Markdown Preview Mermaid Support (Matt Bierner)

#### Option B: Visual Studio 2022

1. Download Community edition from [visualstudio.microsoft.com](https://visualstudio.microsoft.com)
2. Select workloads:
    - ASP.NET and web development
    - Azure development
    - .NET desktop development

### Git

**Windows**: Download from [git-scm.com](https://git-scm.com)  
**macOS**: Included with Xcode Command Line Tools or via Homebrew  
**Linux**: `sudo apt-get install git`

Verify:

```bash
git --version
```

### PowerShell (Windows users already have this)

**macOS/Linux**: Install PowerShell 7+

```bash
# macOS
brew install --cask powershell

# Linux
wget https://github.com/PowerShell/PowerShell/releases/download/v7.4.0/powershell_7.4.0-1.deb_amd64.deb
sudo dpkg -i powershell_7.4.0-1.deb_amd64.deb
```

---

## Step 3: Clone Workshop Repository

```bash
# Clone repository
git clone https://github.com/YOUR-ORG/mcp-workshop.git
cd mcp-workshop

# Checkout workshop branch
git checkout 001-mcp-workshop-course

# Verify structure
ls -la
# Expected: docs/, src/, tests/, infrastructure/, scripts/
```

---

## Step 4: Install ModelContextProtocol Library

The workshop uses the **ModelContextProtocol** prerelease library from NuGet.

### Create Sample Project

```bash
# Navigate to sample directory
cd src/McpWorkshop.Servers/StaticResourceServer

# Restore packages
dotnet restore

# Verify ModelContextProtocol package
dotnet list package | grep ModelContextProtocol
# Expected: ModelContextProtocol   [version]   (--prerelease)
```

### Manual Installation (if needed)

```bash
# Add package to any project
dotnet add package ModelContextProtocol --prerelease

# Verify installation
dotnet build
```

---

## Step 5: Setup Sample Data

Run the data generation script to create local JSON files for exercises:

```powershell
# From repository root
.\scripts\create-sample-data.ps1

# Expected output:
# ✓ Created ./data/customers.json (100 records)
# ✓ Created ./data/products.json (50 records)
# ✓ Created ./data/orders.json (200 records)
# ✓ Created ./data/analytics.json (500 events)
```

**Verify data files:**

```bash
ls -l data/
# Expected: customers.json, products.json, orders.json, analytics.json
```

---

## Step 6: Run First MCP Server

### Start the Static Resource Server

```bash
# Navigate to Exercise 1 server
cd src/McpWorkshop.Servers/StaticResourceServer

# Run server
dotnet run

# Expected output:
# info: Microsoft.Hosting.Lifetime[14]
#       Now listening on: http://localhost:5000
# info: StaticResourceServer[0]
#       MCP Server initialized: static-resource-server v1.0.0
```

### Test the Server

#### Option A: Using PowerShell**

```powershell
# In a new terminal
$response = Invoke-RestMethod -Uri 'http://localhost:5000/mcp' -Method POST -Body '{
  "jsonrpc": "2.0",
  "method": "resources/list",
  "id": 1
}' -ContentType 'application/json'

$response | ConvertTo-Json -Depth 10

# Expected output: List of resources (customers, products)
```

#### Option B: Using REST Client (VS Code)**

Create a file `test-mcp-server.http`:

```http
### Initialize MCP Server
POST http://localhost:5000/mcp
Content-Type: application/json

{
  "jsonrpc": "2.0",
  "method": "initialize",
  "params": {
    "protocolVersion": "2024-11-05",
    "clientInfo": {
      "name": "test-client",
      "version": "1.0.0"
    }
  },
  "id": 1
}

### List Resources
POST http://localhost:5000/mcp
Content-Type: application/json

{
  "jsonrpc": "2.0",
  "method": "resources/list",
  "id": 2
}

### Read Customer Resource
POST http://localhost:5000/mcp
Content-Type: application/json

{
  "jsonrpc": "2.0",
  "method": "resources/read",
  "params": {
    "uri": "workshop://customers"
  },
  "id": 3
}
```

Click "Send Request" above each section and verify responses.

---

## Step 7: Run Verification Script

Ensure all prerequisites are met:

```powershell
# From repository root
.\scripts\verify-setup.ps1

# Expected output:
# ✓ .NET SDK 10.0.x installed
# ✓ Git 2.x.x installed
# ✓ PowerShell 7.x.x installed
# ✓ Repository cloned successfully
# ✓ ModelContextProtocol package found
# ✓ Sample data files present (4/4)
# ✓ Static resource server responds
# ✓ All tests passed! Ready for workshop.
```

---

## Step 8: Azure Setup (Optional)

For exercises involving Azure deployment (Exercise 4, orchestration demos):

### Install Azure CLI

**Windows**:

```powershell
winget install Microsoft.AzureCLI
```

**macOS**:

```bash
brew install azure-cli
```

**Linux**:

```bash
curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash
```

### Login to Azure

```bash
# Login interactively
az login

# Verify subscription
az account show
```

### Install Terraform (for infrastructure exercises)

**Windows**:

```powershell
winget install Hashicorp.Terraform
```

**macOS**:

```bash
brew tap hashicorp/tap
brew install hashicorp/tap/terraform
```

**Linux**:

```bash
wget https://releases.hashicorp.com/terraform/1.6.0/terraform_1.6.0_linux_amd64.zip
unzip terraform_1.6.0_linux_amd64.zip
sudo mv terraform /usr/local/bin/
```

Verify:

```bash
terraform --version
az --version
```

---

## Troubleshooting

### Issue: "ModelContextProtocol package not found"

**Solution**:

```bash
# Ensure NuGet source includes prerelease packages
dotnet nuget add source https://api.nuget.org/v3/index.json -n nuget.org

# Clear cache and restore
dotnet nuget locals all --clear
dotnet restore --force

# Try installing with explicit prerelease flag
dotnet add package ModelContextProtocol --prerelease --version [latest]
```

### Issue: "Port 5000 already in use"

**Solution**:

```bash
# Find process using port 5000
# Windows
netstat -ano | findstr :5000

# macOS/Linux
lsof -i :5000

# Kill process or use different port
# In appsettings.json, change:
"Urls": "http://localhost:5001"
```

### Issue: "Sample data not loading"

**Solution**:

```powershell
# Verify file paths in server configuration
cat src/McpWorkshop.Servers/StaticResourceServer/appsettings.json

# Ensure DataDirectory points to correct path:
"DataAdapter": {
  "Local": {
    "DataDirectory": "./data"  # or absolute path
  }
}
```

### Issue: "Azure CLI login fails"

**Solution**:

```bash
# Use device code flow for browser issues
az login --use-device-code

# Or use service principal (for automation)
az login --service-principal -u <app-id> -p <password> --tenant <tenant-id>
```

---

## Next Steps

🎉 **Congratulations!** Your environment is ready.

### What's Next

1. **Explore Documentation**: Read `docs/modules/01-fundamentals/` for MCP concepts
2. **Review Contracts**: Check `specs/001-mcp-workshop-course/contracts/` for exercise specifications
3. **Start Exercise 1**: Navigate to `docs/modules/03-exercise-static-resource/` and follow instructions
4. **Join Workshop**: Wait for instructor-led session or proceed with self-paced learning

### Workshop Day Checklist

- [ ] Laptop charged
- [ ] All tools installed (run `verify-setup.ps1`)
- [ ] Sample data loaded
- [ ] Test server working
- [ ] Internet connection stable
- [ ] Azure account ready (if doing cloud exercises)
- [ ] Questions prepared for instructor

---

## Resources

- **MCP Specification**: [spec.modelcontextprotocol.io](https://spec.modelcontextprotocol.io)
- **Workshop Documentation**: `docs/` directory
- **Contract Specifications**: `specs/001-mcp-workshop-course/contracts/`
- **Sample Code**: `src/McpWorkshop.Servers/`
- **Support**: Open an issue in the repository or ask during workshop

---

## Offline Mode

If you'll be working without internet during the workshop:

```powershell
# Pre-download all NuGet packages
dotnet restore --force --no-cache

# Download Azure data (for local Cosmos DB emulator)
# Windows: Install Cosmos DB Emulator from Microsoft
# macOS/Linux: Use Docker container
docker pull mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator

# Download sample datasets
.\scripts\create-sample-data.ps1 --offline-mode
```

---

**Estimated Setup Time**: 30-45 minutes  
**Questions?** Contact workshop organizers or check the troubleshooting section above.

Ready to build with MCP! 🚀
