Add-Type -AssemblyName System.Net.Http

$baseUrl = "http://localhost:5179"
$testCount = 0
$passedTests = 0
$failedTests = 0

# Colors
function Write-Success { Write-Host $args -ForegroundColor Green }
function Write-Error { Write-Host $args -ForegroundColor Red }
function Write-Info { Write-Host $args -ForegroundColor Cyan }
function Write-Warning { Write-Host $args -ForegroundColor Yellow }

# Skip SSL for localhost
[System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}

Write-Info "========================================"
Write-Info "Company API Complete Test Suite"
Write-Info "========================================"
Write-Host ""

# Test Helper Function
function Test-Endpoint {
    param(
        [string]$Name,
        [string]$Method,
        [string]$Url,
        [hashtable]$Headers,
        [object]$Body,
        [int]$ExpectedStatus = 200
    )
    
    $script:testCount++
    Write-Info "[Test $script:testCount] $Name"
    Write-Host "  Method: $Method | URL: $Url"
    
    try {
        $params = @{
            Uri = $Url
            Method = $Method
            Headers = $Headers
            ContentType = "application/json"
            ErrorAction = "Stop"
        }
        
        if ($Body) {
            if ($Body -is [string]) {
                $params.Body = $Body
            } else {
                $params.Body = ($Body | ConvertTo-Json -Depth 10)
            }
        }
        
        $response = Invoke-RestMethod @params
        Write-Success "  ? PASSED"
        $script:passedTests++
        return $response
    } catch {
        $statusCode = $_.Exception.Response.StatusCode.value__
        if ($statusCode -eq $ExpectedStatus) {
            Write-Success "  ? PASSED (Expected Status: $statusCode)"
            $script:passedTests++
            return $null
        } else {
            Write-Error "  ? FAILED - Status: $statusCode"
            Write-Error "    Error: $($_.Exception.Message)"
            try {
                $stream = $_.Exception.Response.GetResponseStream()
                if ($stream) {
                    $reader = New-Object System.IO.StreamReader($stream)
                    $errorBody = $reader.ReadToEnd()
                    if (![string]::IsNullOrEmpty($errorBody)) {
                        Write-Warning "    Response: $errorBody"
                    }
                }
            } catch {}
            $script:failedTests++
            return $null
        }
    }
    Write-Host ""
}

# ========== LOGIN ==========
Write-Info ""
Write-Info "========== Step 1: Authentication =========="
Write-Info ""

# Company Login
$companyLoginBody = @{ email = "techcorp@example.com"; password = "Company@123" } | ConvertTo-Json
$companyLogin = Test-Endpoint "Company Login" 'POST' "$baseUrl/api/account/login" @{ "Content-Type" = "application/json" } $companyLoginBody

if ($companyLogin -and $companyLogin.data -and $companyLogin.data.token) {
    $companyToken = $companyLogin.data.token
    $companyHeaders = @{ Authorization = "Bearer $companyToken"; "Content-Type" = "application/json" }
} else {
    Write-Error "Company login failed!"
    exit 1
}

# Admin Login
$adminLoginBody = @{ email = "admin@affiliance.com"; password = "Admin@123" } | ConvertTo-Json
$adminLogin = Test-Endpoint "Admin Login" 'POST' "$baseUrl/api/account/login" @{ "Content-Type" = "application/json" } $adminLoginBody

if ($adminLogin -and $adminLogin.data -and $adminLogin.data.token) {
    $adminToken = $adminLogin.data.token
    $adminHeaders = @{ Authorization = "Bearer $adminToken"; "Content-Type" = "application/json" }
} else {
    Write-Warning "Admin login failed - admin tests will be skipped"
    $adminToken = $null
}

# ========== PUBLIC ENDPOINTS ==========
Write-Info ""
Write-Info "========== Step 2: Public Endpoints =========="
Write-Info ""

# Get company by ID (public)
Test-Endpoint "Get Company By ID (Public)" 'GET' "$baseUrl/api/Company/1" @{ "Content-Type" = "application/json" } $null

# Search companies (public)
Test-Endpoint "Search Companies (Public)" 'GET' "$baseUrl/api/Company/search?keyword=Tech&page=1&pageSize=10" @{ "Content-Type" = "application/json" } $null

# ========== COMPANY AUTHENTICATED ENDPOINTS ==========
Write-Info ""
Write-Info "========== Step 3: Company Authenticated Endpoints =========="
Write-Info ""

# 3.1) Get My Profile
$myProfile = Test-Endpoint "Get My Company Profile" 'GET' "$baseUrl/api/Company/my-profile" $companyHeaders $null
Write-Host "  Profile Data:"
$myProfile.data | ConvertTo-Json -Depth 3 | Write-Host

# 3.2) Get My Statistics
Test-Endpoint "Get My Statistics (No Date Filter)" 'GET' "$baseUrl/api/Company/my-statistics" $companyHeaders $null

# 3.3) Get My Statistics with Date Filter
Test-Endpoint "Get My Statistics (With Date Filter)" 'GET' "$baseUrl/api/Company/my-statistics?from=2024-01-01&to=2024-12-31" $companyHeaders $null

# 3.4) Update My Profile
$updateProfileBody = @{
    campanyName = "TechCorp Solutions - Updated"
    address = "456 Updated Street, Dubai, UAE"
    phoneNumber = "+971501234567"
    website = "https://techcorp-updated.com"
    description = "Updated company description via API"
} | ConvertTo-Json

Test-Endpoint "Update My Profile" 'PUT' "$baseUrl/api/Company/my-profile" $companyHeaders $updateProfileBody

# 3.5) Upload Logo
$dummyImage = Join-Path $env:TEMP "company_logo.png"
$pngBase64 = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII="
[IO.File]::WriteAllBytes($dummyImage, [Convert]::FromBase64String($pngBase64))

Write-Info "[Test $([ref]$script:testCount).Value] Upload Company Logo (Multipart)"
$script:testCount++
Write-Host "  Method: PUT | URL: $baseUrl/api/Company/my-logo"

try {
    $client = New-Object System.Net.Http.HttpClient
    $client.DefaultRequestHeaders.Authorization = New-Object System.Net.Http.Headers.AuthenticationHeaderValue('Bearer', $companyToken)
    $content = New-Object System.Net.Http.MultipartFormDataContent
    $fileStream = [System.IO.File]::OpenRead($dummyImage)
    $fileContent = New-Object System.Net.Http.StreamContent($fileStream)
    $fileContent.Headers.ContentType = [System.Net.Http.Headers.MediaTypeHeaderValue]::Parse('image/png')
    $content.Add($fileContent, 'logoFile', [System.IO.Path]::GetFileName($dummyImage))
    
    $resp = $client.PutAsync("$baseUrl/api/Company/my-logo", $content).Result
    $fileStream.Close()
    
    if ($resp.IsSuccessStatusCode) {
        Write-Success "  ? PASSED"
        $script:passedTests++
        $respContent = $resp.Content.ReadAsStringAsync().Result
        Write-Host "  Response: $respContent"
    } else {
        Write-Error "  ? FAILED - HTTP $($resp.StatusCode)"
        $script:failedTests++
        $respContent = $resp.Content.ReadAsStringAsync().Result
        Write-Warning "  Response: $respContent"
    }
    $client.Dispose()
} catch {
    Write-Error "  ? FAILED - $($_.Exception.Message)"
    $script:failedTests++
}
Write-Host ""

# ========== ADMIN ENDPOINTS ==========
if ($adminToken) {
    Write-Info ""
    Write-Info "========== Step 4: Admin Endpoints =========="
    Write-Info ""
    
    # 4.1) Get All Companies with Filters
    Test-Endpoint "Admin: Get All Companies (No Filter)" 'GET' "$baseUrl/api/Company/admin/all?page=1&pageSize=10" $adminHeaders $null
    
    # 4.2) Get All Companies with Search Filter
    Test-Endpoint "Admin: Get All Companies (Search Filter)" 'GET' "$baseUrl/api/Company/admin/all?searchKeyword=Tech&page=1&pageSize=10" $adminHeaders $null
    
    # 4.3) Get All Companies Verified Filter
    Test-Endpoint "Admin: Get Verified Companies" 'GET' "$baseUrl/api/Company/admin/verified?page=1&pageSize=10" $adminHeaders $null
    
    # 4.4) Get Pending Companies
    $pendingCompanies = Test-Endpoint "Admin: Get Pending Companies" 'GET' "$baseUrl/api/Company/admin/pending?page=1&pageSize=10" $adminHeaders $null
    
    # If there are pending companies, perform admin actions
    if ($pendingCompanies -and $pendingCompanies.data -and $pendingCompanies.data.data -and $pendingCompanies.data.data.Count -gt 0) {
        Write-Info "Found pending companies, performing admin actions..."
        Write-Host ""
        
        $pendingCompanyId = $pendingCompanies.data.data[0].id
        
        # 4.5) Approve Company
        $approveBody = @{ note = "Approved via API test" } | ConvertTo-Json
        Test-Endpoint "Admin: Approve Company" 'POST' "$baseUrl/api/Company/$pendingCompanyId/approve" $adminHeaders $approveBody
        
        # 4.6) Verify Company Documents
        Test-Endpoint "Admin: Verify Company Documents" 'PUT' "$baseUrl/api/Company/$pendingCompanyId/verify" $adminHeaders $null
        
        # 4.7) Suspend Company
        $suspendBody = @{ note = "Suspended via API test" } | ConvertTo-Json
        Test-Endpoint "Admin: Suspend Company" 'PUT' "$baseUrl/api/Company/$pendingCompanyId/suspend" $adminHeaders $suspendBody
        
        # 4.8) Reactivate Company
        Test-Endpoint "Admin: Reactivate Company" 'PUT' "$baseUrl/api/Company/$pendingCompanyId/reactivate" $adminHeaders $null
        
    } else {
        Write-Warning "No pending companies found for admin action tests"
    }
    
    # 4.9) Try to reject company (use a verified one if available)
    if ($pendingCompanies -and $pendingCompanies.data -and $pendingCompanies.data.data -and $pendingCompanies.data.data.Count -gt 0) {
        $companyToReject = $pendingCompanies.data.data[0].id
        $rejectBody = @{ responseNotes = "Rejected via API test - does not meet requirements" } | ConvertTo-Json
        Test-Endpoint "Admin: Reject Company" 'POST' "$baseUrl/api/Company/$companyToReject/reject" $adminHeaders $rejectBody
    }
    
} else {
    Write-Warning ""
    Write-Warning "Admin token not available - skipping admin endpoints"
}

# ========== SUMMARY ==========
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
