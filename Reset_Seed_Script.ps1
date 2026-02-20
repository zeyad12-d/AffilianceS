# Reset & Re-Seed Database Script
# This script logs in as Admin, then calls the reset-seed endpoint

$baseUrl = "http://localhost:5179"

function Write-Success { Write-Host $args -ForegroundColor Green }
function Write-Error { Write-Host $args -ForegroundColor Red }
function Write-Info { Write-Host $args -ForegroundColor Cyan }
function Write-Warning { Write-Host $args -ForegroundColor Yellow }

Write-Info "========================================="
Write-Info "Database Reset & Re-Seed Script"
Write-Info "========================================="
Write-Host ""

# Step 1: Login as Admin
Write-Info "Step 1: Logging in as Admin..."
$loginBody = @{
    email = "admin@affiliance.com"
    password = "Admin@123"
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri "$baseUrl/api/account/login" `
        -Method POST `
        -ContentType "application/json" `
        -Body $loginBody `
        -ErrorAction Stop
    
    $adminToken = $loginResponse.data.token
    Write-Success "Admin login successful!"
    Write-Host ""
} catch {
    Write-Error "Admin login failed: $($_.Exception.Message)"
    Write-Warning "Make sure the application is running and admin account exists."
    exit 1
}

# Step 2: Call reset-seed endpoint
Write-Info "Step 2: Resetting and re-seeding database..."
Write-Warning "This will DELETE all data and re-create seed data!"
Write-Host ""

$headers = @{
    "Authorization" = "Bearer $adminToken"
    "Content-Type" = "application/json"
}

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/admin/reset-seed" `
        -Method POST `
        -Headers $headers `
        -ErrorAction Stop
    
    Write-Success "========================================="
    Write-Success $response.message
    Write-Success "========================================="
    Write-Host ""
    Write-Info "Seeded accounts:"
    Write-Host "  Admin:    admin@affiliance.com / Admin@123"
    Write-Host "  Company:  techcorp@example.com / Company@123"
    Write-Host "  Marketer: marketer1@example.com / Marketer@123"
    Write-Host ""
} catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    Write-Error "Reset failed! Status: $statusCode"
    Write-Error "Error: $($_.Exception.Message)"
    
    try {
        $stream = $_.Exception.Response.GetResponseStream()
        if ($stream) {
            $reader = New-Object System.IO.StreamReader($stream)
            $errorBody = $reader.ReadToEnd()
            Write-Warning "Server Response: $errorBody"
        }
    } catch {}
}
