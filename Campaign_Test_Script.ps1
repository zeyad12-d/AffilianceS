# Campaign API Test Script
# Run this script to test all Campaign endpoints

$baseUrl = "http://localhost:5179"
$campaignBaseUrl = "$baseUrl/api/campaign"

# Colors for output
function Write-Success { Write-Host $args -ForegroundColor Green }
function Write-Error { Write-Host $args -ForegroundColor Red }
function Write-Info { Write-Host $args -ForegroundColor Cyan }
function Write-Warning { Write-Host $args -ForegroundColor Yellow }

# Skip SSL certificate validation (for localhost)
[System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}

Write-Info "========================================="
Write-Info "Campaign API Testing Script"
Write-Info "========================================="
Write-Host ""

# Test counter
$script:testCount = 0
$script:passedTests = 0
$script:failedTests = 0

function Test-Endpoint {
    param(
        [string]$Name,
        [string]$Method,
        [string]$Url,
        [hashtable]$Headers = @{},
        [object]$Body = $null,
        [int]$ExpectedStatus = 200
    )
    
    $script:testCount++
    Write-Info "[Test $script:testCount] $Name"
    Write-Host "  Method: $Method"
    Write-Host "  URL: $Url"
    
    try {
        $params = @{
            Uri = $Url
            Method = $Method
            Headers = $Headers
            ContentType = "application/json; charset=utf-8"
            ErrorAction = "Stop"
        }
        
        if ($Body) {
            $params.Body = ($Body | ConvertTo-Json -Depth 10)
            Write-Host "  Body: $($params.Body)"
        }
        
        $response = Invoke-RestMethod @params
        $statusCode = 200
        
        if ($response.success) {
            Write-Success "  PASSED - Status: $statusCode"
            Write-Host "  Response: $($response.message)"
            if ($response.data) {
                Write-Host "  Data: $($response.data | ConvertTo-Json -Depth 2 -Compress)"
            }
            $script:passedTests++
            return $response
        } else {
            Write-Warning "  Response indicates failure: $($response.message)"
            $script:failedTests++
            return $null
        }
    } catch {
        $statusCode = $_.Exception.Response.StatusCode.value__
        if ($statusCode -eq $ExpectedStatus) {
            Write-Success "  PASSED - Expected status: $statusCode"
            $script:passedTests++
        } else {
            Write-Error "  FAILED - Status: $statusCode"
            Write-Error "  Error: $($_.Exception.Message)"
            
            # Try to read the error response body
            try {
                $stream = $_.Exception.Response.GetResponseStream()
                if ($stream) {
                    $reader = New-Object System.IO.StreamReader($stream)
                    $errorBody = $reader.ReadToEnd()
                    if (![string]::IsNullOrEmpty($errorBody)) {
                        Write-Warning "  Server Response: $errorBody"
                    }
                }
            } catch {
                # Ignore errors reading the error stream
            }

            $script:failedTests++
        }
        return $null
    }
    Write-Host ""
}

# ========================================
# Step 1: Login as Admin
# ========================================
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
    Write-Warning "Make sure the application is running and admin credentials are correct."
    exit 1
}

# ========================================
# Step 2: Setup Company Account
# ========================================
Write-Info "Step 2: Setting up Company account..."

# Try to login first
$companyEmail = "testcompany@example.com"
$companyPassword = "Company@123"
$companyLoginBody = @{
    email = $companyEmail
    password = $companyPassword
} | ConvertTo-Json

$companyToken = $null

try {
    $companyLoginResponse = Invoke-RestMethod -Uri "$baseUrl/api/account/login" `
        -Method POST `
        -ContentType "application/json" `
        -Body $companyLoginBody `
        -ErrorAction Stop
    
    $companyToken = $companyLoginResponse.data.token
    Write-Success "Company login successful!"
    Write-Host ""
} catch {
    Write-Warning "Company not found, trying seeded companies..."
    
    # Try seeded companies
    $seededCompanies = @("techcorp@example.com", "fashionhub@example.com", "healthplus@example.com")
    foreach ($email in $seededCompanies) {
        try {
            $testBody = @{
                email = $email
                password = "Company@123"
            } | ConvertTo-Json
            
            $testResponse = Invoke-RestMethod -Uri "$baseUrl/api/account/login" `
                -Method POST `
                -ContentType "application/json" `
                -Body $testBody `
                -ErrorAction Stop
            
            $companyToken = $testResponse.data.token
            Write-Success "Company login successful with $email!"
            Write-Host ""
            break
        } catch {
            # Continue to next
        }
    }
    
    if (-not $companyToken) {
        Write-Warning "All company logins failed."
        Write-Warning "Company endpoints will be skipped."
        Write-Warning "Please restart the API to trigger database seeding, or add companies through Swagger."
        Write-Host ""
    }
}

# ========================================
# Step 3: Login as Marketer
# ========================================
Write-Info "Step 3: Logging in as Marketer..."
$marketerLoginBody = @{
    email = "Zuser2@example.com"
    password = "Stringsdvdfv!#!1"
} | ConvertTo-Json

try {
    $marketerLoginResponse = Invoke-RestMethod -Uri "$baseUrl/api/account/login" `
        -Method POST `
        -ContentType "application/json" `
        -Body $marketerLoginBody `
        -ErrorAction Stop
    
    $marketerToken = $marketerLoginResponse.data.token
    Write-Success "Marketer login successful!"
    Write-Host ""
} catch {
    Write-Warning "Marketer login failed: $($_.Exception.Message)"
    Write-Warning "Marketer endpoints will be skipped."
    $marketerToken = $null
    Write-Host ""
}

# Headers for authenticated requests
$adminHeaders = @{
    "Authorization" = "Bearer $adminToken"
    "Content-Type" = "application/json"
}

$companyHeaders = @{
    "Authorization" = "Bearer $companyToken"
    "Content-Type" = "application/json"
}

$marketerHeaders = @{
    "Authorization" = "Bearer $marketerToken"
    "Content-Type" = "application/json"
}

# ========================================
# PUBLIC ENDPOINTS - Campaign Discovery
# ========================================
Write-Info "========================================="
Write-Info "Testing GET Endpoints (Public)"
Write-Info "========================================="
Write-Host ""

# Test 1: Get All Campaigns
$allCampaigns = Test-Endpoint `
    -Name "Get All Campaigns" `
    -Method "GET" `
    -Url "${campaignBaseUrl}?page=1&pageSize=10"

# Test 2: Get Active Campaigns
$activeCampaigns = Test-Endpoint `
    -Name "Get Active Campaigns" `
    -Method "GET" `
    -Url "$campaignBaseUrl/active?page=1&pageSize=10"

# Test 3: Get Campaign by ID (if we have campaigns)
$campaignId = $null
if ($activeCampaigns -and $activeCampaigns.data) {
    # Check if data has items property (paged result)
    if ($activeCampaigns.data.items -and $activeCampaigns.data.items.Count -gt 0) {
        $campaignId = $activeCampaigns.data.items[0].id
    }
    # Or check if data is an array directly
    elseif ($activeCampaigns.data -is [Array] -and $activeCampaigns.data.Count -gt 0) {
        $campaignId = $activeCampaigns.data[0].id
    }
    
    if ($campaignId) {
        $campaignDetails = Test-Endpoint `
            -Name "Get Campaign by ID" `
            -Method "GET" `
            -Url "$campaignBaseUrl/$campaignId"
    } else {
        Write-Warning "Skipping campaign ID test - no campaigns found"
    }
} else {
    Write-Warning "Skipping campaign ID test - no campaigns found"
}

# Test 4: Search Campaigns
Test-Endpoint `
    -Name "Search Campaigns (keyword)" `
    -Method "GET" `
    -Url "$campaignBaseUrl/search?keyword=test&page=1&pageSize=10"

# Test 5: Get Campaigns by Category
Test-Endpoint `
    -Name "Get Campaigns by Category" `
    -Method "GET" `
    -Url "$campaignBaseUrl/category/1?page=1&pageSize=10"

# Test 6: Get Campaigns by Company
Test-Endpoint `
    -Name "Get Campaigns by Company" `
    -Method "GET" `
    -Url "$campaignBaseUrl/company/1?page=1&pageSize=10"

# Test 7: Get Campaigns by Status
Test-Endpoint `
    -Name "Get Campaigns by Status (Active)" `
    -Method "GET" `
    -Url "$campaignBaseUrl/status/1?page=1&pageSize=10"

# ========================================
# MARKETER ENDPOINTS
# ========================================
if ($marketerToken) {
    Write-Info "========================================="
    Write-Info "Testing Marketer Endpoints"
    Write-Info "========================================="
    Write-Host ""

    # Test 8: Get Recommended Campaigns
    Test-Endpoint `
        -Name "Get Recommended Campaigns (Marketer)" `
        -Method "GET" `
        -Url "$campaignBaseUrl/recommended?limit=10" `
        -Headers $marketerHeaders

    # Test 9: Apply to Campaign
    if ($campaignId) {
        $applicationResponse = Test-Endpoint `
            -Name "Apply to Campaign (Marketer)" `
            -Method "POST" `
            -Url "$campaignBaseUrl/$campaignId/apply" `
            -Headers $marketerHeaders
        
        # Store application ID for later tests
        $applicationId = $null
        if ($applicationResponse -and $applicationResponse.data) {
            $applicationId = $applicationResponse.data.id
            Write-Success "Application created with ID: $applicationId"
        }

        # Test 10: Withdraw Application
        if ($applicationId) {
            Start-Sleep -Seconds 2
            Test-Endpoint `
                -Name "Withdraw Application (Marketer)" `
                -Method "POST" `
                -Url "$campaignBaseUrl/applications/$applicationId/withdraw" `
                -Headers $marketerHeaders
        }
    } else {
        Write-Warning "Skipping marketer application tests - no campaign found"
    }
}

# ========================================
# COMPANY ENDPOINTS - Campaign CRUD (No Auth Required - Testing Mode)
# ========================================
Write-Info "========================================="
Write-Info "Testing Company Endpoints - CRUD"
Write-Info "========================================="
Write-Host ""

# Test 11: Get My Campaigns
$myCampaigns = Test-Endpoint `
    -Name "Get My Campaigns (Company)" `
    -Method "GET" `
    -Url "$campaignBaseUrl/my-campaigns?page=1&pageSize=10"

    # Test 12: Create Campaign
    $timestamp = Get-Date -Format 'yyyyMMddHHmmss'
    $startDate = (Get-Date).AddDays(1).ToString("yyyy-MM-ddTHH:mm:ss")
    $endDate = (Get-Date).AddDays(30).ToString("yyyy-MM-ddTHH:mm:ss")
    
    $newCampaign = @{
        title = "Test Campaign $timestamp"
        description = "This is a test campaign created by automated testing script"
        categoryId = 1
        commissionType = 0  # Percentage
        commissionValue = 10.5
        budget = 5000.00
        startDate = $startDate
        endDate = $endDate
        promotionalMaterials = "Test promotional materials"
        trackingBaseUrl = "https://example.com/track"
    }

    $createdCampaign = Test-Endpoint `
        -Name "Create Campaign (Company)" `
        -Method "POST" `
        -Url "$campaignBaseUrl" `
         `
        -Body $newCampaign

    $createdCampaignId = $null
    if ($createdCampaign -and $createdCampaign.data) {
        $createdCampaignId = $createdCampaign.data.id
        Write-Success "Created campaign with ID: $createdCampaignId"
        Write-Host ""
    }

    # Test 13: Get My Campaign by ID
    if ($createdCampaignId) {
        Test-Endpoint `
            -Name "Get My Campaign by ID (Company)" `
            -Method "GET" `
            -Url "$campaignBaseUrl/my-campaigns/$createdCampaignId" `
            
    }

    # Test 14: Update Campaign
    if ($createdCampaignId) {
        Start-Sleep -Seconds 1
        $updateData = @{
            description = "Updated campaign description - test $timestamp"
            promotionalMaterials = "Updated promotional materials"
        }
        
        Test-Endpoint `
            -Name "Update Campaign (Company)" `
            -Method "PUT" `
            -Url "$campaignBaseUrl/$createdCampaignId" `
             `
            -Body $updateData
    }

    # ========================================
    # ADMIN ENDPOINTS - Campaign Approval
    # ========================================
    Write-Info "========================================="
    Write-Info "Testing Admin Endpoints - Approval"
    Write-Info "========================================="
    Write-Host ""

    # Test 15: Approve Campaign (Admin)
    if ($createdCampaignId) {
        Start-Sleep -Seconds 1
        Test-Endpoint `
            -Name "Approve Campaign (Admin)" `
            -Method "PUT" `
            -Url "$campaignBaseUrl/$createdCampaignId/admin/approve" `
             `
            -Body "Approved for testing purposes"
    }

    # ========================================
    # COMPANY ENDPOINTS - Campaign Lifecycle
    # ========================================
    Write-Info "========================================="
    Write-Info "Testing Company Endpoints - Lifecycle"
    Write-Info "========================================="
    Write-Host ""

    # Test 16: Pause Campaign
    if ($createdCampaignId) {
        Start-Sleep -Seconds 1
        Test-Endpoint `
            -Name "Pause Campaign (Company)" `
            -Method "PUT" `
            -Url "$campaignBaseUrl/$createdCampaignId/pause" `
            
    }

    # Test 17: Resume Campaign
    if ($createdCampaignId) {
        Start-Sleep -Seconds 1
        Test-Endpoint `
            -Name "Resume Campaign (Company)" `
            -Method "PUT" `
            -Url "$campaignBaseUrl/$createdCampaignId/resume" `
            
    }

    # Test 18: Update Campaign Status
    if ($createdCampaignId) {
        Start-Sleep -Seconds 1
        Test-Endpoint `
            -Name "Update Campaign Status (Company)" `
            -Method "PUT" `
            -Url "$campaignBaseUrl/$createdCampaignId/status" `
             `
            -Body 1  # Active
    }

    # ========================================
    # COMPANY ENDPOINTS - Applications Management
    # ========================================
    Write-Info "========================================="
    Write-Info "Testing Company Endpoints - Applications"
    Write-Info "========================================="
    Write-Host ""

    # Test 19: Get Campaign Applications
    if ($createdCampaignId) {
        Test-Endpoint `
            -Name "Get Campaign Applications (Company)" `
            -Method "GET" `
            -Url "$campaignBaseUrl/$createdCampaignId/applications?page=1&pageSize=10" `
            
    }

    # Create a test application for approval/rejection tests
    if ($createdCampaignId -and $marketerToken) {
        Write-Info "Creating test application for approval tests..."
        $testApplication = Test-Endpoint `
            -Name "Apply to Created Campaign (Marketer)" `
            -Method "POST" `
            -Url "$campaignBaseUrl/$createdCampaignId/apply" `
            -Headers $marketerHeaders
        
        $testApplicationId = $null
        if ($testApplication -and $testApplication.data) {
            $testApplicationId = $testApplication.data.id
            Write-Success "Test application created with ID: $testApplicationId"
            Write-Host ""

            # Test 20: Approve Application
            Start-Sleep -Seconds 1
            $approveData = @{
                applicationId = $testApplicationId
                note = "Approved for testing"
            }
            Test-Endpoint `
                -Name "Approve Application (Company)" `
                -Method "POST" `
                -Url "$campaignBaseUrl/applications/$testApplicationId/approve" `
                 `
                -Body $approveData
        }
    }

    # Create another test application for rejection test
    if ($createdCampaignId -and $marketerToken) {
        Write-Info "Creating another test application for rejection test..."
        
        # First withdraw the previous application to apply again
        Start-Sleep -Seconds 2
        
        $testApplication2 = Test-Endpoint `
            -Name "Apply Again to Campaign (Marketer)" `
            -Method "POST" `
            -Url "$campaignBaseUrl/$createdCampaignId/apply" `
            -Headers $marketerHeaders `
            -ExpectedStatus 400  # May fail if marketer already applied
        
        if ($testApplication2 -and $testApplication2.data) {
            $testApplicationId2 = $testApplication2.data.id
            
            # Test 21: Reject Application
            Start-Sleep -Seconds 1
            $rejectData = @{
                applicationId = $testApplicationId2
                note = "Rejected for testing purposes"
            }
            Test-Endpoint `
                -Name "Reject Application (Company)" `
                -Method "POST" `
                -Url "$campaignBaseUrl/applications/$testApplicationId2/reject" `
                 `
                -Body $rejectData
        }
    }

    # Test 22: Get Campaign Statistics
    if ($createdCampaignId) {
        Start-Sleep -Seconds 1
        Test-Endpoint `
            -Name "Get Campaign Statistics (Company)" `
            -Method "GET" `
            -Url "$campaignBaseUrl/$createdCampaignId/statistics" `
            
    }

    # ========================================
    # COMPANY ENDPOINTS - Delete Campaign
    # ========================================
    Write-Info "========================================="
    Write-Info "Testing Company Endpoints - Delete"
    Write-Info "========================================="
    Write-Host ""

    # Test 23: Delete Campaign
    if ($createdCampaignId) {
        # First pause it to allow deletion
        Test-Endpoint `
            -Name "Pause Campaign Before Delete (Company)" `
            -Method "PUT" `
            -Url "$campaignBaseUrl/$createdCampaignId/pause" `
            

        Start-Sleep -Seconds 1
        
        Test-Endpoint `
            -Name "Delete Campaign (Company)" `
            -Method "DELETE" `
            -Url "$campaignBaseUrl/$createdCampaignId" `
            
    }

# ========================================
# Create Campaign for Rejection Test by Admin
# ========================================
Write-Info "========================================="
Write-Info "Testing Admin Rejection"
Write-Info "========================================="
Write-Host ""

    $timestamp = Get-Date -Format 'yyyyMMddHHmmss'
    $startDate = (Get-Date).AddDays(2).ToString("yyyy-MM-ddTHH:mm:ss")
    $endDate = (Get-Date).AddDays(32).ToString("yyyy-MM-ddTHH:mm:ss")
    
    $campaignForRejection = @{
        title = "Campaign To Reject $timestamp"
        description = "This campaign will be rejected by admin"
        categoryId = 1
        commissionType = 1  # Fixed
        commissionValue = 50.00
        budget = 2000.00
        startDate = $startDate
        endDate = $endDate
    }

    $campaignToReject = Test-Endpoint `
        -Name "Create Campaign for Rejection Test (Company)" `
        -Method "POST" `
        -Url "$campaignBaseUrl" `
         `
        -Body $campaignForRejection

    $rejectCampaignId = $null
    if ($campaignToReject -and $campaignToReject.data) {
        $rejectCampaignId = $campaignToReject.data.id
        Write-Success "Created campaign for rejection with ID: $rejectCampaignId"
        Write-Host ""

        # Test 24: Reject Campaign (Admin)
        Start-Sleep -Seconds 1
        Test-Endpoint `
            -Name "Reject Campaign (Admin)" `
            -Method "PUT" `
            -Url "$campaignBaseUrl/$rejectCampaignId/admin/reject" `
             `
            -Body "Rejected for testing - does not meet guidelines"
    }

# ========================================
# Test Summary
# ========================================
Write-Host ""
Write-Info "========================================="
Write-Info "Test Summary"
Write-Info "========================================="
Write-Host ""
Write-Success "Total Tests: $script:testCount"
Write-Success "Passed: $script:passedTests"
if ($script:failedTests -gt 0) {
    Write-Error "Failed: $script:failedTests"
} else {
    Write-Success "Failed: $script:failedTests"
}

$successRate = [math]::Round(($script:passedTests / $script:testCount) * 100, 2)
Write-Host ""
Write-Info "Success Rate: $successRate%"
Write-Host ""
Write-Info "========================================="
Write-Info "Campaign API Testing Complete!"
Write-Info "========================================="
