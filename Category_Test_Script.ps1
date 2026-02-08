# Category API Test Script
# Run this script to test all Category endpoints

$baseUrl = "https://localhost:7253"
$categoryBaseUrl = "$baseUrl/api/category"

# Colors for output
function Write-Success { Write-Host $args -ForegroundColor Green }
function Write-Error { Write-Host $args -ForegroundColor Red }
function Write-Info { Write-Host $args -ForegroundColor Cyan }
function Write-Warning { Write-Host $args -ForegroundColor Yellow }

# Skip SSL certificate validation (for localhost)
[System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}

Write-Info "========================================="
Write-Info "Category API Testing Script"
Write-Info "========================================="
Write-Host ""

# Step 1: Login as Admin to get token
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
    Write-Success "Login successful! Token obtained."
    Write-Host ""
} catch {
    Write-Error "Login failed: $($_.Exception.Message)"
    Write-Warning "Make sure the application is running and admin credentials are correct."
    exit 1
}

# Headers for authenticated requests
$authHeaders = @{
    "Authorization" = "Bearer $adminToken"
    "Content-Type" = "application/json"
}

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
        }
        
        $response = Invoke-RestMethod @params
        $statusCode = 200
        
        if ($response.success) {
            Write-Success "  PASSED - Status: $statusCode"
            Write-Host "  Response: $($response.message)"
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

Write-Info "========================================="
Write-Info "Testing GET Endpoints (Public)"
Write-Info "========================================="
Write-Host ""

# Test 1: Get All Categories
$allCategories = Test-Endpoint `
    -Name "Get All Categories" `
    -Method "GET" `
    -Url "$categoryBaseUrl"

# Test 2: Get Root Categories
$rootCategories = Test-Endpoint `
    -Name "Get Root Categories" `
    -Method "GET" `
    -Url "$categoryBaseUrl/roots"

# Test 3: Get Category by ID (if we have categories)
if ($allCategories -and $allCategories.data -and $allCategories.data.Count -gt 0) {
    $firstCategoryId = $allCategories.data[0].id
    $categoryById = Test-Endpoint `
        -Name "Get Category by ID" `
        -Method "GET" `
        -Url "$categoryBaseUrl/$firstCategoryId"
    
    # Test 4: Get Category Children
    Test-Endpoint `
        -Name "Get Category Children" `
        -Method "GET" `
        -Url "$categoryBaseUrl/$firstCategoryId/children"
} else {
    Write-Warning "Skipping category ID tests - no categories found"
}

# Test 5: Get Category Hierarchy
Test-Endpoint `
    -Name "Get Category Hierarchy" `
    -Method "GET" `
    -Url "$categoryBaseUrl/hierarchy"

# Test 6: Get Category by Slug
if ($allCategories -and $allCategories.data -and $allCategories.data.Count -gt 0) {
    $firstCategorySlug = $allCategories.data[0].slug
    Test-Endpoint `
        -Name "Get Category by Slug" `
        -Method "GET" `
        -Url "$categoryBaseUrl/slug/$firstCategorySlug"
}

Write-Host ""
Write-Info "========================================="
Write-Info "Testing POST Endpoints (Admin)"
Write-Info "========================================="
Write-Host ""

# Test 7: Create Category
$timestamp = Get-Date -Format 'yyyyMMddHHmmss'
$newCategory = @{
    nameEn = "Test Category $timestamp"
    nameAr = "فئة اختبار"
    slug = "test-category-$timestamp"
    icon = "TEST"
    parentId = $null
}

$createdCategory = Test-Endpoint `
    -Name "Create Category" `
    -Method "POST" `
    -Url "$categoryBaseUrl" `
    -Headers $authHeaders `
    -Body $newCategory

$createdCategoryId = $null
if ($createdCategory -and $createdCategory.data) {
    $createdCategoryId = $createdCategory.data.id
    Write-Success "Created category with ID: $createdCategoryId"
}

# Test 8: Create Categories Bulk
$bulkCategories = @(
    @{
        nameEn = "Bulk Category 1"
        nameAr = "فئة مجمعة 1"
        slug = "bulk-category-1-$timestamp"
        icon = "BULK1"
        parentId = $null
    },
    @{
        nameEn = "Bulk Category 2"
        nameAr = "فئة مجمعة 2"
        slug = "bulk-category-2-$timestamp"
        icon = "BULK2"
        parentId = $null
    }
)

Test-Endpoint `
    -Name "Create Categories Bulk" `
    -Method "POST" `
    -Url "$categoryBaseUrl/bulk" `
    -Headers $authHeaders `
    -Body $bulkCategories

Write-Host ""
Write-Info "========================================="
Write-Info "Testing PUT Endpoints (Admin)"
Write-Info "========================================="
Write-Host ""

# Test 9: Update Category
if ($createdCategoryId) {
    $updateData = @{
        nameEn = "Updated Test Category"
        nameAr = "فئة اختبار محدثة"
        icon = "UPDATED"
    }
    
    Test-Endpoint `
        -Name "Update Category" `
        -Method "PUT" `
        -Url "$categoryBaseUrl/$createdCategoryId" `
        -Headers $authHeaders `
        -Body $updateData
} else {
    Write-Warning "Skipping update test - no category was created"
}

Write-Host ""
Write-Info "========================================="
Write-Info "Testing DELETE Endpoints (Admin)"
Write-Info "========================================="
Write-Host ""

# Test 10: Delete Category Safe
if ($createdCategoryId) {
    Test-Endpoint `
        -Name "Delete Category Safe" `
        -Method "DELETE" `
        -Url "$categoryBaseUrl/$createdCategoryId/safe" `
        -Headers $authHeaders
} else {
    Write-Warning "Skipping delete test - no category was created"
}

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
Write-Host ""
Write-Info "========================================="
