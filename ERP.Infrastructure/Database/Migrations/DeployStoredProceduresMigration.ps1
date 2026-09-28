# PowerShell script to deploy stored procedures migration
# This script executes the migration and all updated stored procedure files

param(
    [Parameter(Mandatory=$true)]
    [string]$ServerInstance,
    
    [Parameter(Mandatory=$true)]
    [string]$Database,
    
    [Parameter(Mandatory=$false)]
    [string]$Username,
    
    [Parameter(Mandatory=$false)]
    [string]$Password,
    
    [Parameter(Mandatory=$false)]
    [switch]$UseIntegratedSecurity = $true
)

$ErrorActionPreference = "Stop"

# Get the script directory
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RootDir = Split-Path -Parent $ScriptDir
$StoredProceduresDir = Join-Path $RootDir "StoredProcedures"
$MigrationFile = Join-Path $ScriptDir "UpdateStoredProceduresNamingConvention.sql"

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Stored Procedures Migration Deployment" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Server: $ServerInstance" -ForegroundColor Yellow
Write-Host "Database: $Database" -ForegroundColor Yellow
Write-Host ""

# Build connection string
if ($UseIntegratedSecurity) {
    $ConnectionString = "Server=$ServerInstance;Database=$Database;Integrated Security=True;TrustServerCertificate=True;"
} else {
    $ConnectionString = "Server=$ServerInstance;Database=$Database;User Id=$Username;Password=$Password;TrustServerCertificate=True;"
}

try {
    # Load SQL Server module
    Import-Module SqlServer -ErrorAction SilentlyContinue
    
    if (-not (Get-Module SqlServer)) {
        Write-Host "Installing SqlServer module..." -ForegroundColor Yellow
        Install-Module -Name SqlServer -Scope CurrentUser -Force -AllowClobber
        Import-Module SqlServer
    }
    
    Write-Host "Step 1: Executing migration script..." -ForegroundColor Green
    $MigrationSQL = Get-Content $MigrationFile -Raw
    Invoke-Sqlcmd -ConnectionString $ConnectionString -Query $MigrationSQL -ErrorAction Stop
    Write-Host "Migration script executed successfully." -ForegroundColor Green
    Write-Host ""
    
    # List of stored procedure files to execute
    $ProcedureFiles = @(
        "CoreModule_StoredProcedures.sql",
        "CRMModule_StoredProcedures.sql",
        "HRMModule_StoredProcedures.sql",
        "FinanceModule_StoredProcedures.sql",
        "InventoryModule_StoredProcedures.sql",
        "AdminModule_StoredProcedures.sql"
    )
    
    Write-Host "Step 2: Executing stored procedure files..." -ForegroundColor Green
    foreach ($File in $ProcedureFiles) {
        $FilePath = Join-Path $StoredProceduresDir $File
        if (Test-Path $FilePath) {
            Write-Host "  Executing: $File" -ForegroundColor Yellow
            $SQL = Get-Content $FilePath -Raw
            Invoke-Sqlcmd -ConnectionString $ConnectionString -Query $SQL -ErrorAction Stop
            Write-Host "  ✓ $File executed successfully" -ForegroundColor Green
        } else {
            Write-Host "  ⚠ Warning: $File not found, skipping..." -ForegroundColor Yellow
        }
    }
    
    Write-Host ""
    Write-Host "=========================================" -ForegroundColor Cyan
    Write-Host "Migration completed successfully!" -ForegroundColor Green
    Write-Host "=========================================" -ForegroundColor Cyan
    
} catch {
    Write-Host ""
    Write-Host "=========================================" -ForegroundColor Red
    Write-Host "ERROR: Migration failed!" -ForegroundColor Red
    Write-Host "=========================================" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}

