@echo off
REM Batch script to deploy stored procedures migration
REM Usage: DeployStoredProceduresMigration.bat [ServerInstance] [Database] [Username] [Password]

setlocal enabledelayedexpansion

REM Set default values
set SERVER_INSTANCE=%1
set DATABASE=%2
set USERNAME=%3
set PASSWORD=%4

if "%SERVER_INSTANCE%"=="" set SERVER_INSTANCE=localhost
if "%DATABASE%"=="" set DATABASE=ERP

echo =========================================
echo Stored Procedures Migration Deployment
echo =========================================
echo Server: %SERVER_INSTANCE%
echo Database: %DATABASE%
echo.

REM Get the script directory
set SCRIPT_DIR=%~dp0
set ROOT_DIR=%SCRIPT_DIR%..
set STORED_PROCEDURES_DIR=%ROOT_DIR%\StoredProcedures
set MIGRATION_FILE=%SCRIPT_DIR%UpdateStoredProceduresNamingConvention.sql

REM Check if sqlcmd is available
where sqlcmd >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: sqlcmd is not found in PATH.
    echo Please install SQL Server Command Line Utilities or add sqlcmd to your PATH.
    exit /b 1
)

echo Step 1: Executing migration script...
if "%USERNAME%"=="" (
    sqlcmd -S %SERVER_INSTANCE% -d %DATABASE% -i "%MIGRATION_FILE%" -E
) else (
    sqlcmd -S %SERVER_INSTANCE% -d %DATABASE% -U %USERNAME% -P %PASSWORD% -i "%MIGRATION_FILE%"
)

if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Migration script failed!
    exit /b 1
)

echo Migration script executed successfully.
echo.

echo Step 2: Executing stored procedure files...

set PROCEDURE_FILES=CoreModule_StoredProcedures.sql CRMModule_StoredProcedures.sql HRMModule_StoredProcedures.sql FinanceModule_StoredProcedures.sql InventoryModule_StoredProcedures.sql AdminModule_StoredProcedures.sql

for %%F in (%PROCEDURE_FILES%) do (
    set FILE_PATH=%STORED_PROCEDURES_DIR%\%%F
    if exist "!FILE_PATH!" (
        echo   Executing: %%F
        if "%USERNAME%"=="" (
            sqlcmd -S %SERVER_INSTANCE% -d %DATABASE% -i "!FILE_PATH!" -E
        ) else (
            sqlcmd -S %SERVER_INSTANCE% -d %DATABASE% -U %USERNAME% -P %PASSWORD% -i "!FILE_PATH!"
        )
        if !ERRORLEVEL! EQU 0 (
            echo   [OK] %%F executed successfully
        ) else (
            echo   [ERROR] %%F failed to execute
            exit /b 1
        )
    ) else (
        echo   [WARNING] %%F not found, skipping...
    )
)

echo.
echo =========================================
echo Migration completed successfully!
echo =========================================

endlocal

