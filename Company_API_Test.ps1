Add-Type -AssemblyName System.Net.Http

$baseUrl = "http://localhost:5179"

# Colors
function Write-Success { Write-Host $args -ForegroundColor Green }
function Write-Error { Write-Host $args -ForegroundColor Red }
function Write-Info { Write-Host $args -ForegroundColor Cyan }

# Skip SSL for localhost
[System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}

Write-Info "Company API Test Script"
Write-Host ""

# Helper: invoke and print
function Invoke-AndPrint($name, $request) {
    Write-Info "== $name =="
    try {
        $resp = Invoke-RestMethod @request -ErrorAction Stop
        Write-Success "SUCCESS: $name"
        $resp | ConvertTo-Json -Depth 5 | Write-Host
        return $resp
    } catch {
        Write-Error "FAILED: $name - $($_.Exception.Message)"
        try { $_.Exception.Response.GetResponseStream() | ForEach-Object { [System.IO.StreamReader]::new($_).ReadToEnd() } | Write-Host } catch {}
        return $null
    }
}

# 1) Company login (use seeded account)
$companyLoginBody = @{ email = "techcorp@example.com"; password = "Company@123" } | ConvertTo-Json
$loginReq = @{ Uri = "$baseUrl/api/account/login"; Method = 'POST'; ContentType = 'application/json'; Body = $companyLoginBody }
$companyLogin = Invoke-AndPrint "Company Login" $loginReq
if ($companyLogin -and $companyLogin.data -and $companyLogin.data.token) {
    $companyToken = $companyLogin.data.token
    $companyHeaders = @{ Authorization = "Bearer $companyToken"; "Content-Type" = "application/json" }
} else {
    Write-Error "Company login failed, aborting company tests."
    exit 1
}

# 2) Admin login (for admin endpoints)
$adminLoginBody = @{ email = "admin@affiliance.com"; password = "Admin@123" } | ConvertTo-Json
$adminReq = @{ Uri = "$baseUrl/api/account/login"; Method = 'POST'; ContentType = 'application/json'; Body = $adminLoginBody }
$adminLogin = Invoke-AndPrint "Admin Login" $adminReq
if ($adminLogin -and $adminLogin.data -and $adminLogin.data.token) {
    $adminToken = $adminLogin.data.token
    $adminHeaders = @{ Authorization = "Bearer $adminToken"; "Content-Type" = "application/json" }
} else {
    Write-Warning "Admin login failed - admin-only tests will be skipped."
}

# 3) Get my-profile
$req = @{ Uri = "$baseUrl/api/Company/my-profile"; Method = 'GET'; Headers = $companyHeaders }
$myProfile = Invoke-AndPrint "Get My Company Profile" $req

# 4) Update my-profile
$updateBody = @{
    campanyName = "TechCorp Solutions - TEST"
    address = "123 Test St, Local"
    phoneNumber = "+971501234567"
    website = "https://techcorp.example.test"
    description = "Updated via API Test"
} | ConvertTo-Json
$req = @{ Uri = "$baseUrl/api/Company/my-profile"; Method = 'PUT'; Headers = $companyHeaders; ContentType = 'application/json'; Body = $updateBody }
Invoke-AndPrint "Update My Profile" $req

# 5) Upload logo (multipart)
$dummyImage = Join-Path $env:TEMP "company_logo.png"
$pngBase64 = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII="
[IO.File]::WriteAllBytes($dummyImage, [Convert]::FromBase64String($pngBase64))

$client = New-Object System.Net.Http.HttpClient
$client.DefaultRequestHeaders.Authorization = New-Object System.Net.Http.Headers.AuthenticationHeaderValue('Bearer', $companyToken)
$content = New-Object System.Net.Http.MultipartFormDataContent
$fileStream = [System.IO.File]::OpenRead($dummyImage)
$fileContent = New-Object System.Net.Http.StreamContent($fileStream)
$fileContent.Headers.ContentType = [System.Net.Http.Headers.MediaTypeHeaderValue]::Parse('image/png')
$content.Add($fileContent, 'logoFile', [System.IO.Path]::GetFileName($dummyImage))
try {
    $resp = $client.PutAsync("$baseUrl/api/Company/my-logo", $content).Result
    if ($resp.IsSuccessStatusCode) { Write-Success "Upload Logo: Success"; $resp.Content.ReadAsStringAsync().Result | Write-Host } else { Write-Error "Upload Logo: HTTP $($resp.StatusCode)"; $resp.Content.ReadAsStringAsync().Result | Write-Host }
} catch { Write-Error "Upload Logo failed: $($_.Exception.Message)" }
$fileStream.Close(); $client.Dispose()

# 6) Get my-statistics
$req = @{ Uri = "$baseUrl/api/Company/my-statistics"; Method = 'GET'; Headers = $companyHeaders }
Invoke-AndPrint "Get My Statistics" $req

# 7) Admin: get pending companies
if ($adminToken) {
    $req = @{ Uri = "$baseUrl/api/Company/admin/pending?page=1&pageSize=10"; Method = 'GET'; Headers = $adminHeaders }
    $pending = Invoke-AndPrint "Admin Get Pending Companies" $req
    if ($pending -and $pending.data -and $pending.data.data -and $pending.data.data.Count -gt 0) {
        $targetId = $pending.data.data[0].id
        # Approve
        $req = @{ Uri = "$baseUrl/api/Company/$targetId/approve"; Method = 'POST'; Headers = $adminHeaders; ContentType = 'application/json'; Body = '{}' }
        Invoke-AndPrint "Admin Approve Company $targetId" $req
    } else {
        Write-Info "No pending companies to approve."
    }
}

Write-Info "Company tests completed."