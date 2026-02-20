# Affiliance Company API Test Script
# Tests all Company endpoint operations

$baseUrl = "http://localhost:5179/api/company"
$adminToken = ""
$companyToken = ""
$companyId = 0
$newCompanyId = 0

# Admin credentials (from DataSeeder)
$adminEmail = "admin@affiliance.com"
$adminPassword = "Admin@123"

# Company credentials for testing
$companyEmail = "testcompany@example.com"
$companyPassword = "Company@123"

Write-Host "==========================================" -ForegroundColor Yellow
Write-Host "AFFILIANCE COMPANY API TEST SCRIPT" -ForegroundColor Yellow
Write-Host "==========================================" -ForegroundColor Yellow

# Helper function to get auth tokens
function Get-AuthToken {
    param (
        [string]$email,
        [string]$password,
        [string]$role
    )

    Write-Host "`nAuthenticating $role ($email)..." -ForegroundColor Cyan
    try {
        $loginResponse = Invoke-WebRequest -Uri "http://localhost:5179/api/account/login" `
            -Method Post `
            -ContentType "application/json" `
            -Body (ConvertTo-Json @{
                email = $email
                password = $password
            }) `
            -ErrorAction Stop

        $loginData = $loginResponse.Content | ConvertFrom-Json
        if ($loginData.success) {
            Write-Host "✓ Authentication successful" -ForegroundColor Green
            return $loginData.data.token
        }
        else {
            Write-Host "✗ Authentication failed: $($loginData.message)" -ForegroundColor Red
            return $null
        }
    }
    catch {
        Write-Host "✗ Error during authentication: $($_.Exception.Message)" -ForegroundColor Red
        return $null
    }
}

# Helper function to make API calls
function Invoke-CompanyApi {
    param (
        [string]$Uri,
        [string]$Method = "Get",
        [object]$Body = $null,
        [string]$Token = $null,
        [string]$TestName
    )

    Write-Host "`nTest: $TestName" -ForegroundColor Cyan
    Write-Host "Method: $Method | Endpoint: $Uri" -ForegroundColor Gray

    try {
        $params = @{
            Uri             = $Uri
            Method          = $Method
            ContentType     = "application/json"
            ErrorAction     = "Stop"
        }

        if ($Body) {
            $params.Body = $Body
        }

        if ($Token) {
            $params.Headers = @{
                "Authorization" = "Bearer $Token"
            }
        }

        $response = Invoke-WebRequest @params
        $data = $response.Content | ConvertFrom-Json

        if ($data.success) {
            Write-Host "✓ PASSED" -ForegroundColor Green
            return $data
        }
        else {
            Write-Host "✗ FAILED: $($data.message)" -ForegroundColor Red
            return $data
        }
    }
    catch {
        try {
            $errorResponse = $_.Exception.Response.Content | ConvertFrom-Json
            Write-Host "✗ ERROR: $($errorResponse.message)" -ForegroundColor Red
        }
        catch {
            Write-Host "✗ ERROR: $($_.Exception.Message)" -ForegroundColor Red
        }
        return $null
    }
}

# ============================================
# SECTION 1: GET OPERATIONS (Public)
# ============================================

Write-Host "`n`n========================================" -ForegroundColor Yellow
Write-Host "   SECTION 1: PUBLIC GET OPERATIONS       " -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Yellow

# Test 1.1: Search Companies (no filter)
$searchUri = $baseUrl + '/search?keyword=&page=1&pageSize=10'
$searchResult = Invoke-CompanyApi `
    -Uri $searchUri `
    -Method "Get" `
    -TestName "Search all companies"

if ($searchResult -and $searchResult.data.items.Count -gt 0) {
    $companyId = $searchResult.data.items[0].id
    Write-Host "Found company ID: $companyId" -ForegroundColor Green
}

# Test 1.2: Get Company by ID
if ($companyId -gt 0) {
    Invoke-CompanyApi `
        -Uri "$baseUrl/$companyId" `
        -Method "Get" `
        -TestName "Get company by ID"
}

# ============================================
# SECTION 2: AUTHENTICATION
# ============================================

Write-Host "`n`n========================================" -ForegroundColor Yellow
Write-Host "         SECTION 2: AUTHENTICATION         " -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Yellow

# Get tokens
$adminToken = Get-AuthToken -email $adminEmail -password $adminPassword -role "Admin"
$companyToken = Get-AuthToken -email $companyEmail -password $companyPassword -role "Company"

# ============================================
# SECTION 3: COMPANY OPERATIONS (Authenticated)
# ============================================

Write-Host "`n`n========================================" -ForegroundColor Yellow
Write-Host "   SECTION 3: COMPANY OPERATIONS           " -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Yellow

# Test 3.1: Get My Profile
if ($companyToken) {
    $profileResult = Invoke-CompanyApi `
        -Uri "$baseUrl/my-profile" `
        -Method "Get" `
        -Token $companyToken `
        -TestName "Get my company profile"
    
    if ($profileResult -and $profileResult.data) {
        Write-Host "My Company: $($profileResult.data.name)" -ForegroundColor Gray
    }
}

# Test 3.2: Get My Statistics
if ($companyToken) {
    Invoke-CompanyApi `
        -Uri "$baseUrl/my-statistics" `
        -Method "Get" `
        -Token $companyToken `
        -TestName "Get my company statistics"
}

# Test 3.3: Update My Profile
if ($companyToken) {
    $updateBody = ConvertTo-Json @{
        name        = "Updated Company Name"
        description = "Updated Description"
        phone       = "+1-555-0100"
    }
    
    Invoke-CompanyApi `
        -Uri "$baseUrl/my-profile" `
        -Method "Put" `
        -Body $updateBody `
        -Token $companyToken `
        -TestName "Update my company profile"
}

# ============================================
# SECTION 4: ADMIN OPERATIONS
# ============================================

Write-Host "`n`n========================================" -ForegroundColor Yellow
Write-Host "      SECTION 4: ADMIN OPERATIONS          " -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Yellow

if ($adminToken) {
    # Test 4.1: Get Pending Companies
    $pendingUri = $baseUrl + '/admin/pending?page=1&pageSize=10'
    $pendingResult = Invoke-CompanyApi `
        -Uri $pendingUri `
        -Method "Get" `
        -Token $adminToken `
        -TestName "Get pending companies for approval"
    
    if ($pendingResult -and $pendingResult.data.items.Count -gt 0) {
        $pendingCompanyId = $pendingResult.data.items[0].companyId
        Write-Host "Pending company ID: $pendingCompanyId" -ForegroundColor Green
    }

    # Test 4.2: Get Verified Companies
    $verifiedUri = $baseUrl + '/admin/verified?page=1&pageSize=10'
    Invoke-CompanyApi `
        -Uri $verifiedUri `
        -Method "Get" `
        -Token $adminToken `
        -TestName "Get verified companies"

    # Test 4.3: Get All Companies with Filters
    $allUri = $baseUrl + '/admin/all?page=1&pageSize=10&isVerified=true&sortBy=createdAt&isDescending=true'
    Invoke-CompanyApi `
        -Uri $allUri `
        -Method "Get" `
        -Token $adminToken `
        -TestName "Get all companies with filters"

    # Test 4.4: Approve Company (if pending exists)
    if ($pendingCompanyId -gt 0) {
        $approveBody = ConvertTo-Json @{
            note = "Approved by test script"
        }
        
        Invoke-CompanyApi `
            -Uri "$baseUrl/$pendingCompanyId/approve" `
            -Method "Post" `
            -Body $approveBody `
            -Token $adminToken `
            -TestName "Approve pending company"
    }

    # Test 4.5: Verify Company
    if ($companyId -gt 0) {
        Invoke-CompanyApi `
            -Uri "$baseUrl/$companyId/verify" `
            -Method "Put" `
            -Token $adminToken `
            -TestName "Verify company documents"
    }

    # Test 4.6: Suspend Company
    if ($companyId -gt 0) {
        $suspendBody = ConvertTo-Json @{
            note = "Suspended by test script"
        }
        
        Invoke-CompanyApi `
            -Uri "$baseUrl/$companyId/suspend" `
            -Method "Put" `
            -Body $suspendBody `
            -Token $adminToken `
            -TestName "Suspend company"
    }

    # Test 4.7: Reactivate Company
    if ($companyId -gt 0) {
        Invoke-CompanyApi `
            -Uri "$baseUrl/$companyId/reactivate" `
            -Method "Put" `
            -Token $adminToken `
            -TestName "Reactivate suspended company"
    }
}

Write-Host "`n`n========================================" -ForegroundColor Yellow
Write-Host "      COMPANY API TEST COMPLETED           " -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Yellow
