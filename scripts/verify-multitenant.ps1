param([int]$ApiPort = 18080, [int]$WebPort = 18081, [switch]$IncludeSampleData, [switch]$UseSqlScript)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$config = Get-Content (Join-Path $root 'BilliardManagement.API\appsettings.json') -Raw | ConvertFrom-Json
Add-Type -AssemblyName System.Data
$builder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder($config.ConnectionStrings.DBDefault)
$database = 'billiard_verify_' + [Guid]::NewGuid().ToString('N')
$builder['Initial Catalog'] = 'master'
$master = New-Object System.Data.SqlClient.SqlConnection($builder.ConnectionString)
$api = $null; $web = $null
$keys = @('ConnectionStrings__DBDefault', 'ApiSettings__BaseAddress', 'Seed__SuperAdminPassword', 'Seed__SampleData', 'VERIFY_API', 'VERIFY_WEB')
$previous = @{}
foreach ($key in $keys) { $previous[$key] = [Environment]::GetEnvironmentVariable($key) }
try {
    & dotnet build (Join-Path $root 'BilliardManagementSystem.sln') --nologo -v q
    if ($LASTEXITCODE -ne 0) { throw 'Build failed' }
    $master.Open()
    $cmd = $master.CreateCommand(); $cmd.CommandText = "CREATE DATABASE [$database]"; $cmd.ExecuteNonQuery() | Out-Null
    $builder['Initial Catalog'] = $database
    if ($UseSqlScript) {
        $bootstrap = New-Object System.Data.SqlClient.SqlConnection($builder.ConnectionString)
        try {
            $bootstrap.Open()
            $script = Get-Content (Join-Path $root 'db\database.sql') -Raw
            foreach ($batch in [regex]::Split($script, '(?im)^\s*GO\s*\r?$')) {
                if (![string]::IsNullOrWhiteSpace($batch)) {
                    $sql = $bootstrap.CreateCommand(); $sql.CommandText = $batch; $sql.ExecuteNonQuery() | Out-Null
                }
            }
        } finally { $bootstrap.Dispose() }
    }
    $env:ConnectionStrings__DBDefault = $builder.ConnectionString
    $env:Seed__SuperAdminPassword = 'TenantSmoke123!'
    $env:Seed__SampleData = $IncludeSampleData.IsPresent.ToString().ToLowerInvariant()
    $env:VERIFY_API = "http://127.0.0.1:$ApiPort"
    $env:VERIFY_WEB = "http://127.0.0.1:$WebPort"
    $env:ApiSettings__BaseAddress = "$($env:VERIFY_API)/api/"
    $logDir = [IO.Path]::GetTempPath()
    $api = Start-Process dotnet -ArgumentList "run --project BilliardManagement.API --no-build --no-launch-profile --urls $($env:VERIFY_API) --Logging:LogLevel:Microsoft.EntityFrameworkCore=Warning" -WorkingDirectory $root -PassThru -RedirectStandardOutput (Join-Path $logDir "$database-api.log") -RedirectStandardError (Join-Path $logDir "$database-api-error.log")
    foreach ($url in @("$($env:VERIFY_API)/swagger/index.html", "$($env:VERIFY_WEB)/Auth/Login")) {
        if ($url.StartsWith($env:VERIFY_WEB)) {
            $web = Start-Process dotnet -ArgumentList "run --project BilliardManagement.Web --no-build --no-launch-profile --urls $($env:VERIFY_WEB)" -WorkingDirectory $root -PassThru -RedirectStandardOutput (Join-Path $logDir "$database-web.log") -RedirectStandardError (Join-Path $logDir "$database-web-error.log")
        }
        $ready = $false
        for ($i=0; $i -lt 180; $i++) {
            $code = & curl.exe -s -o NUL -w '%{http_code}' --max-time 2 $url
            if ($code -eq '200') { $ready=$true; break }
            Start-Sleep -Seconds 1
        }
        if (!$ready) { throw "Startup timeout: $url; logs: $logDir$database-*.log" }
    }
    $audit = New-Object System.Data.SqlClient.SqlConnection($builder.ConnectionString)
    try {
        $audit.Open()
        $check = $audit.CreateCommand()
        $check.CommandText = Get-Content (Join-Path $PSScriptRoot 'audit-tenant-schema.sql') -Raw
        $check.ExecuteScalar()
        & node (Join-Path $PSScriptRoot 'verify-multitenant.mjs')
        $testExitCode = $LASTEXITCODE
        $check.ExecuteScalar()
        if ($testExitCode -eq 0) {
            $snapshot = $audit.CreateCommand()
            $snapshot.CommandText = 'SELECT Id, Status INTO #OrdersBefore FROM dbo.Orders; SELECT Id, PasswordHash, StoreId, Role INTO #UsersBefore FROM dbo.Users; SELECT Id, DurationHours, DurationMinutes, EndTime, TotalPrice, ComboPrice, Status INTO #SessionsBefore FROM dbo.TableSessions;'
            $snapshot.ExecuteNonQuery() | Out-Null
            & taskkill /PID $api.Id /T /F | Out-Null
            $api.WaitForExit()
            $api = Start-Process dotnet -ArgumentList "run --project BilliardManagement.API --no-build --no-launch-profile --urls $($env:VERIFY_API) --Logging:LogLevel:Microsoft.EntityFrameworkCore=Warning" -WorkingDirectory $root -PassThru -RedirectStandardOutput (Join-Path $logDir "$database-api-restart.log") -RedirectStandardError (Join-Path $logDir "$database-api-restart-error.log")
            $ready = $false
            for ($i=0; $i -lt 60; $i++) {
                $code = & curl.exe -s -o NUL -w '%{http_code}' --max-time 2 "$($env:VERIFY_API)/swagger/index.html"
                if ($code -eq '200') { $ready=$true; break }
                Start-Sleep -Seconds 1
            }
            if (!$ready) { throw "Restart timeout; logs: $logDir$database-api-restart-*.log" }
            $snapshot.CommandText = @'
IF EXISTS (SELECT Id, Status FROM #OrdersBefore EXCEPT SELECT Id, Status FROM dbo.Orders)
    THROW 51000, 'Startup seed modified existing orders', 1;
IF EXISTS (SELECT Id, PasswordHash, StoreId, Role FROM #UsersBefore EXCEPT SELECT Id, PasswordHash, StoreId, Role FROM dbo.Users)
    THROW 51000, 'Startup seed deleted or reassigned an existing account', 1;
IF EXISTS (SELECT Id, DurationHours, DurationMinutes, EndTime, TotalPrice, ComboPrice, Status FROM #SessionsBefore
    EXCEPT SELECT Id, DurationHours, DurationMinutes, EndTime, TotalPrice, ComboPrice, Status FROM dbo.TableSessions)
    THROW 51000, 'Startup seed modified existing sessions', 1;
SELECT 'PASS AUDIT: API restart preserves existing accounts, order statuses and active/pay-later sessions';
'@
            $snapshot.ExecuteScalar()
            $check.ExecuteScalar()
        }
    } finally { $audit.Dispose() }
    if ($testExitCode -ne 0) { throw "Integration checks failed; logs: $logDir$database-*.log" }
} finally {
    foreach ($process in @($api, $web)) { if ($process -and !$process.HasExited) { & taskkill /PID $process.Id /T /F | Out-Null } }
    if ($master.State -eq 'Open') {
        $cmd = $master.CreateCommand()
        $cmd.CommandText = "IF DB_ID('$database') IS NOT NULL BEGIN ALTER DATABASE [$database] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$database]; END"
        $cmd.ExecuteNonQuery() | Out-Null; $master.Dispose()
    }
    foreach ($key in $keys) { [Environment]::SetEnvironmentVariable($key, $previous[$key]) }
}
