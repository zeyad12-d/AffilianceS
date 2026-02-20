# Marketer, Review, and TrackingLink API Test Script
# Run this script to test all endpoints for Marketers

Add-Type -AssemblyName System.Net.Http

$baseUrl = "http://localhost:5179"

# Create a dummy PDF for CV
$dummyPdfPath = Join-Path $env:TEMP "test_document.pdf"
"This is a dummy PDF content for testing." | Set-Content -Path $dummyPdfPath

# Create a dummy PNG for ID (1x1 pixel)
$dummyImagePath = Join-Path $env:TEMP "test_image.png"
$base64Image = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII="
[System.IO.File]::WriteAllBytes($dummyImagePath, [System.Convert]::FromBase64String($base64Image))

# Colors for output
function Write-Success { Write-Host $args -ForegroundColor Green }
function Write-Error { Write-Host $args -ForegroundColor Red }
function Write-Info { Write-Host $args -ForegroundColor Cyan }
function Write-Warning { Write-Host $args -ForegroundColor Yellow }

# Skip SSL certificate validation (for localhost)
[System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}

Write-Info "========================================="
Write-Info "Marketer API Testing Script"
Write-Info "========================================="
Write-Host ""

# Step 1: Login as Admin to get Admin token (for admin endpoints)
Write-Info "Step 1: Logging in as Admin..."
$adminLoginBody = @{
    email = "admin@affiliance.com"
    password = "Admin@123"
} | ConvertTo-Json

try {
    $adminLoginResponse = Invoke-RestMethod -Uri "$baseUrl/api/account/login" `
        -Method POST `
        -ContentType "application/json" `
        -Body $adminLoginBody `
        -ErrorAction Stop
    
    $adminToken = $adminLoginResponse.data.token
    Write-Success "Admin Login successful! Token obtained."
} catch {
    Write-Error "Admin Login failed: $($_.Exception.Message)"
    Write-Warning "Make sure the application is running and admin credentials are correct."
}

# Step 2: Login as Marketer to get Marketer token (for marketer endpoints)
Write-Info "Step 2: Logging in as Marketer..."
# Using default seeded marketer account
$marketerLoginBody = @{
    email = "Zuser2@example.com"
    password = "Stringsdvdfv!#!1"
} | ConvertTo-Json

try {
    # Try login first
    $marketerLoginResponse = Invoke-RestMethod -Uri "$baseUrl/api/account/login" `
        -Method POST `
        -ContentType "application/json" `
        -Body $marketerLoginBody `
        -ErrorAction Stop
    
    $marketerToken = $marketerLoginResponse.data.token
    Write-Success "Marketer Login successful! Token obtained."
} catch {
    Write-Error "Marketer Login failed: $($_.Exception.Message)"
    Write-Warning "Please make sure the default marketer account exists (marketer1@example.com with password Marketer@123)"
    Write-Warning "Or run database seed to create default accounts."
    exit 1
}

# Headers for authenticated requests
$adminHeaders = @{
    "Authorization" = "Bearer $adminToken"
    "Content-Type" = "application/json"
}

$marketerHeaders = @{
    "Authorization" = "Bearer $marketerToken"
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
            # Check if Body is already JSON string (simplistic check) or object
            if ($Body -is [string]) {
                $params.Body = $Body
            } else {
                $params.Body = ($Body | ConvertTo-Json -Depth 10)
            }
        }
        
        $response = Invoke-RestMethod @params
        $statusCode = 200
        
        if ($response.success) {
            Write-Success "  PASSED - Status: $statusCode"
            # Write-Host "  Response: $($response.message)"
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

function Test-MultipartUpload {
    param(
        [string]$Name,
        [string]$Url,
        [string]$FilePath,
        [string]$ParamName,
        [string]$Token
    )
    
    $script:testCount++
    Write-Info "[Test $script:testCount] $Name (Multipart Upload)"
    Write-Host "  Method: PUT"
    Write-Host "  URL: $Url"

    try {
        $client = New-Object System.Net.Http.HttpClient
        $client.DefaultRequestHeaders.Authorization = New-Object System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", $Token)
        
        $content = New-Object System.Net.Http.MultipartFormDataContent
        
        $fileStream = [System.IO.File]::OpenRead($FilePath)
        $fileName = [System.IO.Path]::GetFileName($FilePath)
        $extension = [System.IO.Path]::GetExtension($FilePath).ToLower()
        $contentType = "application/octet-stream"
        
        if ($extension -eq ".pdf") { $contentType = "application/pdf" }
        elseif ($extension -eq ".png") { $contentType = "image/png" }

        $fileContent = New-Object System.Net.Http.StreamContent($fileStream)
        # Using octet-stream or pdf as content type
        $fileContent.Headers.ContentType = [System.Net.Http.Headers.MediaTypeHeaderValue]::Parse($contentType)
        
        $content.Add($fileContent, $ParamName, $fileName)
        
        # Determine method (default/hardcoded to PUT for these endpoints)
        $response = $client.PutAsync($Url, $content).Result
        
        $fileStream.Close()
        $client.Dispose()
        
        $statusCode = [int]$response.StatusCode
        
        if ($response.IsSuccessStatusCode) {
            Write-Success "  PASSED - Status: $statusCode"
            $script:passedTests++
            return $true
        } else {
            Write-Error "  FAILED - Status: $statusCode"
            $script:failedTests++
            return $false
        }
    } catch {
        Write-Error "  FAILED - Error: $($_.Exception.Message)"
        $script:failedTests++
        return $false
    }
    Write-Host ""
}

Write-Host ""
Write-Info "========================================="
Write-Info "Testing Marketer Dashboard & Profile"
Write-Info "========================================="
Write-Host ""

# Test: Get Dashboard
Test-Endpoint `
    -Name "Get Marketer Dashboard" `
    -Method "GET" `
    -Url "$baseUrl/api/Marketer/my/dashboard" `
    -Headers $marketerHeaders

# Test: Get Statistics
Test-Endpoint `
    -Name "Get Marketer Statistics" `
    -Method "GET" `
    -Url "$baseUrl/api/Marketer/my/statistics" `
    -Headers $marketerHeaders

# Test: Get Earnings Report
Test-Endpoint `
    -Name "Get Earnings Report" `
    -Method "GET" `
    -Url "$baseUrl/api/Marketer/my/earnings-report" `
    -Headers $marketerHeaders

# Test: Get Performance History
Test-Endpoint `
    -Name "Get Performance History" `
    -Method "GET" `
    -Url "$baseUrl/api/Marketer/my/performance-history" `
    -Headers $marketerHeaders

# Test: Update Profile
$profileUpdate = @{
    bio = "Expert Marketer in Tech and Health"
    niche = "Health & Tech"
    socialLinks = "twitter.com/testmarketer"
    skillsExtracted = "Marketing, SEO"
}

Test-Endpoint `
    -Name "Update Marketer Profile" `
    -Method "PUT" `
    -Url "$baseUrl/api/Marketer/my/profile" `
    -Headers $marketerHeaders `
    -Body $profileUpdate

# Test: Update Bio
Test-Endpoint `
    -Name "Update Marketer Bio" `
    -Method "PUT" `
    -Url "$baseUrl/api/Marketer/my/bio" `
    -Headers $marketerHeaders `
    -Body '"Updated Bio via specific endpoint"'

# Test: Update Niche
Test-Endpoint `
    -Name "Update Marketer Niche" `
    -Method "PUT" `
    -Url "$baseUrl/api/Marketer/my/niche" `
    -Headers $marketerHeaders `
    -Body '"Finance"'

# Test: Update Social Links
Test-Endpoint `
    -Name "Update Social Links" `
    -Method "PUT" `
    -Url "$baseUrl/api/Marketer/my/social-links" `
    -Headers $marketerHeaders `
    -Body '"linkedin.com/in/testmarketer"'

# Test: Update Skills
Test-Endpoint `
    -Name "Update Skills" `
    -Method "PUT" `
    -Url "$baseUrl/api/Marketer/my/skills" `
    -Headers $marketerHeaders `
    -Body '"SEO, Marketing, Content"'

# Test: Update CV (File Upload)
Test-MultipartUpload `
    -Name "Update CV" `
    -Url "$baseUrl/api/Marketer/my/cv" `
    -FilePath $dummyPdfPath `
    -ParamName "cvFile" `
    -Token $marketerToken

# Test: Update National ID (File Upload)
Test-MultipartUpload `
    -Name "Update National ID" `
    -Url "$baseUrl/api/Marketer/my/national-id" `
    -FilePath $dummyImagePath `
    -ParamName "nationalIdFile" `
    -Token $marketerToken


Write-Host ""
Write-Info "========================================="
Write-Info "Testing Personality Test"
Write-Info "========================================="
Write-Host ""

# Test: Submit Personality Test
$personalityTest = @{
    answers = @(
        @{ questionId = 1; answer = 5 },
        @{ questionId = 2; answer = 3 }
    )
}

Test-Endpoint `
    -Name "Submit Personality Test" `
    -Method "POST" `
    -Url "$baseUrl/api/Marketer/my/personality-test" `
    -Headers $marketerHeaders `
    -Body $personalityTest

# Test: Get Personality Test Results
Test-Endpoint `
    -Name "Get Personality Test Results" `
    -Method "GET" `
    -Url "$baseUrl/api/Marketer/my/personality-test" `
    -Headers $marketerHeaders

Write-Host ""
Write-Info "========================================="
Write-Info "Testing Applications & AI Suggestions"
Write-Info "========================================="
Write-Host ""

# Test: Get AI Suggestions
Test-Endpoint `
    -Name "Get AI Suggestions" `
    -Method "GET" `
    -Url "$baseUrl/api/Marketer/my/ai-suggestions" `
    -Headers $marketerHeaders

# Test: Get My Applications
$myApplications = Test-Endpoint `
    -Name "Get My Applications" `
    -Method "GET" `
    -Url "$baseUrl/api/Marketer/my/applications" `
    -Headers $marketerHeaders

# If we have applications, test specific ID endpoints
if ($myApplications -and $myApplications.data -and $myApplications.data.data -and $myApplications.data.data.Count -gt 0) {
    $applicationId = $myApplications.data.data[0].id
    
    # Test: Get Application By ID
    Test-Endpoint `
        -Name "Get Application By ID" `
        -Method "GET" `
        -Url "$baseUrl/api/Marketer/my/applications/$applicationId" `
        -Headers $marketerHeaders
        
    # Test: Withdraw Application
    Test-Endpoint `
        -Name "Withdraw Application" `
        -Method "POST" `
        -Url "$baseUrl/api/Marketer/my/applications/$applicationId/withdraw" `
        -Headers $marketerHeaders
}

Write-Host ""
Write-Info "========================================="
Write-Info "Testing Review Endpoints"
Write-Info "========================================="
Write-Host ""

# Test: Get My Reviews
Test-Endpoint `
    -Name "Get My Reviews" `
    -Method "GET" `
    -Url "$baseUrl/api/marketer/my/reviews" `
    -Headers $marketerHeaders

# Test: Get Reviews Given
Test-Endpoint `
    -Name "Get Reviews Given" `
    -Method "GET" `
    -Url "$baseUrl/api/marketer/my/reviews/given" `
    -Headers $marketerHeaders

# Test: Get Average Rating
Test-Endpoint `
    -Name "Get Average Rating" `
    -Method "GET" `
    -Url "$baseUrl/api/marketer/my/average-rating" `
    -Headers $marketerHeaders

Write-Host ""
Write-Info "========================================="
Write-Info "Testing Tracking Link Endpoints"
Write-Info "========================================="
Write-Host ""

# Test: Get My Tracking Links
$trackingLinks = Test-Endpoint `
    -Name "Get My Tracking Links" `
    -Method "GET" `
    -Url "$baseUrl/api/marketer/my/tracking-links" `
    -Headers $marketerHeaders

if ($trackingLinks -and $trackingLinks.data -and $trackingLinks.data.data -and $trackingLinks.data.data.Count -gt 0) {
    $linkId = $trackingLinks.data.data[0].id
    
    # Test: Get Tracking Link By ID
    Test-Endpoint `
        -Name "Get Tracking Link By ID" `
        -Method "GET" `
        -Url "$baseUrl/api/marketer/my/tracking-links/$linkId" `
        -Headers $marketerHeaders
        
    # Test: Get Tracking Link Statistics
    Test-Endpoint `
        -Name "Get Tracking Link Statistics" `
        -Method "GET" `
        -Url "$baseUrl/api/marketer/my/tracking-links/$linkId/statistics" `
        -Headers $marketerHeaders
}

Write-Host ""
Write-Info "========================================="
Write-Info "Testing Admin Endpoints for Marketer"
Write-Info "========================================="
Write-Host ""

# Test: Get Pending Verification Marketers (Admin)
$pendingMarketers = Test-Endpoint `
    -Name "Get Pending Verification Marketers (Admin)" `
    -Method "GET" `
    -Url "$baseUrl/api/Marketer/admin/pending-verification" `
    -Headers $adminHeaders

# If we found pending marketers, let's verify one
if ($pendingMarketers -and $pendingMarketers.data -and $pendingMarketers.data.data -and $pendingMarketers.data.data.Count -gt 0) {
    $targetMarketerId = $pendingMarketers.data.data[0].id
    
    Write-Info "Admin Actions on Marketer ID: $targetMarketerId"

    # Test: Verify Marketer
    Test-Endpoint `
        -Name "Verify Marketer (Admin)" `
        -Method "PUT" `
        -Url "$baseUrl/api/Marketer/$targetMarketerId/verify" `
        -Headers $adminHeaders
        
    # Test: Update Performance Score
    Test-Endpoint `
        -Name "Update Performance Score (Admin)" `
        -Method "PUT" `
        -Url "$baseUrl/api/Marketer/$targetMarketerId/performance-score" `
        -Headers $adminHeaders `
        -Body 4.8
        
    # Test: Unverify Marketer
    Test-Endpoint `
        -Name "Unverify Marketer (Admin)" `
        -Method "PUT" `
        -Url "$baseUrl/api/Marketer/$targetMarketerId/unverify" `
        -Headers $adminHeaders
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
