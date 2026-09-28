# PowerShell script to clean script.sql by removing dbo schema duplicates
# Keeps only module schema tables (Core, CRM, Finance, HRM, Inventory, Procurement)

$inputFile = "c:\Users\osama\OneDrive\Documents\script.sql"
$outputFile = "c:\Users\osama\OneDrive\Documents\script_cleaned.sql"
$backupFile = "c:\Users\osama\OneDrive\Documents\script_backup.sql"

Write-Host "Reading input file..."
$content = Get-Content $inputFile -Raw

# Create backup
Write-Host "Creating backup..."
Copy-Item $inputFile $backupFile -Force

Write-Host "Processing file (this may take a few minutes)..."

# Remove dbo table definitions (except __EFMigrationsHistory)
# Pattern: /****** Object:  Table [dbo].[TableName] ... CREATE TABLE [dbo].[TableName] ... GO
$pattern1 = '(?s)/\*\*\*\*\*.*?Object:.*?Table \[dbo\]\.(?!__EFMigrationsHistory).*?CREATE TABLE \[dbo\]\.[^\]]+\].*?GO\s+'
$content = $content -replace $pattern1, ''

# Remove ALTER TABLE statements for dbo tables (defaults, constraints)
# Pattern: ALTER TABLE [dbo].[TableName] ... GO
$pattern2 = '(?s)ALTER TABLE \[dbo\]\.(?!__EFMigrationsHistory)[^\]]+\].*?GO\s+'
$content = $content -replace $pattern2, ''

# Remove foreign keys that reference dbo tables
# Pattern: ALTER TABLE ... FOREIGN KEY ... REFERENCES [dbo].[TableName] ... GO
$pattern3 = '(?s)ALTER TABLE \[[^\]]+\]\.\[[^\]]+\].*?WITH CHECK ADD.*?FOREIGN KEY.*?REFERENCES \[dbo\]\.(?!__EFMigrationsHistory)[^\]]+\].*?GO\s+'
$content = $content -replace $pattern3, ''

# Also remove the CHECK CONSTRAINT lines that follow FK definitions
$pattern4 = '(?s)ALTER TABLE \[[^\]]+\]\.\[[^\]]+\].*?CHECK CONSTRAINT.*?GO\s+'
# But only if the previous FK was removed - this is tricky, so we'll be more specific
# Let's remove CHECK CONSTRAINT lines that reference dbo tables
$pattern5 = '(?s)ALTER TABLE \[dbo\]\.(?!__EFMigrationsHistory)[^\]]+\].*?CHECK CONSTRAINT.*?GO\s+'
$content = $content -replace $pattern5, ''

# Remove indexes on dbo tables (except __EFMigrationsHistory)
$pattern6 = '(?s)/\*\*\*\*\*.*?Index.*?ON \[dbo\]\.(?!__EFMigrationsHistory)[^\]]+\].*?GO\s+'
$content = $content -replace $pattern6, ''

# Clean up multiple consecutive GO statements
$content = $content -replace '(GO\s+){3,}', "GO`r`n`r`n"

# Clean up multiple blank lines
$content = $content -replace '(\r?\n\s*){4,}', "`r`n`r`n"

Write-Host "Writing cleaned file..."
$content | Set-Content $outputFile -Encoding UTF8 -NoNewline

Write-Host "Done! Cleaned file saved to: $outputFile"
Write-Host "Backup saved to: $backupFile"
Write-Host ""
Write-Host "Original file size: $((Get-Item $inputFile).Length) bytes"
Write-Host "Cleaned file size: $((Get-Item $outputFile).Length) bytes"
Write-Host ""
Write-Host "Next step: Review the cleaned file and replace the original if satisfied."
