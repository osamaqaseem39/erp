# PowerShell script to deploy all stored procedures to the ERP database
# Usage: .\Deploy-StoredProcedures.ps1

$ErrorActionPreference = "Stop"

# Database connection settings
$Server = "HELLOWORLD\SQLEXPRESS"
$Database = "erp"
$TrustedConnection = $true

# Path to stored procedure files
$StoredProceduresPath = "ERP.Infrastructure\Database\StoredProcedures"

# List of stored procedure files to deploy
$SpFiles = @(
    "CoreModule_StoredProcedures.sql",
    "HRMModule_StoredProcedures.sql",
    "FinanceModule_StoredProcedures.sql",
    "InventoryModule_StoredProcedures.sql",
    "CRMModule_StoredProcedures.sql",
    "ProcurementModule_StoredProcedures.sql",
    "ManufacturingModule_StoredProcedures.sql",
    "ProjectsModule_StoredProcedures.sql",
    "SalesPOSModule_StoredProcedures.sql",
    "ReportsModule_StoredProcedures.sql",
    "AdminModule_StoredProcedures.sql"
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Deploying Stored Procedures to Database" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Server: $Server" -ForegroundColor Yellow
Write-Host "Database: $Database" -ForegroundColor Yellow
Write-Host ""

$successCount = 0
$failCount = 0

foreach ($file in $SpFiles) {
    $filePath = Join-Path $StoredProceduresPath $file
    
    if (Test-Path $filePath) {
        Write-Host "Deploying: $file..." -ForegroundColor Green -NoNewline
        
        try {
            # Use sqlcmd to execute the SQL file
            $result = sqlcmd -S $Server -d $Database -i $filePath -b 2>&1
            
            if ($LASTEXITCODE -eq 0) {
                Write-Host " SUCCESS" -ForegroundColor Green
                $successCount++
            } else {
                Write-Host " FAILED" -ForegroundColor Red
                Write-Host "Error output: $result" -ForegroundColor Red
                $failCount++
            }
        }
        catch {
            Write-Host " FAILED" -ForegroundColor Red
            Write-Host "Exception: $_" -ForegroundColor Red
            $failCount++
        }
    }
    else {
        Write-Host "File not found: $filePath" -ForegroundColor Yellow
        $failCount++
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Deployment Summary" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Successful: $successCount" -ForegroundColor Green
Write-Host "Failed: $failCount" -ForegroundColor $(if ($failCount -gt 0) { "Red" } else { "Green" })
Write-Host ""

if ($failCount -eq 0) {
    Write-Host "All stored procedures deployed successfully!" -ForegroundColor Green
    exit 0
} else {
    Write-Host "Some stored procedures failed to deploy. Please check the errors above." -ForegroundColor Red
    exit 1
}

