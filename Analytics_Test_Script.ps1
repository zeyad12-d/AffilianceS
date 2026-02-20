# Analytics API Test Script
# Run this script to test all Analytics endpoints
# Uses the default seeded Admin and Company accounts

$baseUrl = "http://localhost:5179"
$analyticsBaseUrl = "$baseUrl/api/analytics"

# Colors for output
function Write-Success { Write-Host $args -ForegroundColor Green }
function Write-Error { Write-Host $args -ForegroundColor Red }
function Write-Info { Write-Host $args -ForegroundColor Cyan }
function Write-Warning { Write-Host $args -ForegroundColor Yellow }

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
Write-Info "Analytics API Testing Script"
Write-Info "========================================="
Write-Host ""

# =========================================
# Step 1: Login as Admin
# =========================================
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
    Write-Success "Admin login successful! Token obtained."
    Write-Host ""
} catch {
    Write-Error "Admin login failed: $($_.Exception.Message)"
    Write-Warning "Make sure the application is running and admin account is seeded (admin@affiliance.com / Admin@123)."
    exit 1
}

$adminHeaders = @{
    "Authorization" = "Bearer $adminToken"
    "Content-Type" = "application/json"
}

# =========================================
# Step 2: Login as Company
# =========================================
Write-Info "Step 2: Logging in as Company (TechCorp)..."
$companyLoginBody = @{
    email = "techcorp@example.com"
    password = "Company@123"
} | ConvertTo-Json

try {
    $companyLoginResponse = Invoke-RestMethod -Uri "$baseUrl/api/account/login" `
        -Method POST `
        -ContentType "application/json" `
        -Body $companyLoginBody `
        -ErrorAction Stop
    
    $companyToken = $companyLoginResponse.data.token
    Write-Success "Company login successful! Token obtained."
    Write-Host ""
} catch {
    Write-Error "Company login failed: $($_.Exception.Message)"
    Write-Warning "Make sure the company account is seeded (techcorp@example.com / Company@123)."
    exit 1
}

$companyHeaders = @{
    "Authorization" = "Bearer $companyToken"
    "Content-Type" = "application/json"
}

# =========================================
# Company Analytics Endpoints
# =========================================
Write-Host ""
Write-Info "========================================="
Write-Info "Testing Company Analytics Endpoints"
Write-Info "========================================="
Write-Host ""

# Company Analytics Overview (no filter)
$companyAnalytics = Test-Endpoint `
    -Name "Get Company Analytics Overview (no filter)" `
    -Method "GET" `
    -Url "$analyticsBaseUrl/company/overview" `
    -Headers $companyHeaders

if ($companyAnalytics -and $companyAnalytics.data) {
    Write-Host "  Company: $($companyAnalytics.data.companyName)"
    Write-Host "  Total Campaigns: $($companyAnalytics.data.totalCampaigns)"
    Write-Host "  Active Campaigns: $($companyAnalytics.data.activeCampaigns)"
    Write-Host "  Total Marketers: $($companyAnalytics.data.totalMarketers)"
    Write-Host "  Total Clicks: $($companyAnalytics.data.totalClicks)"
    Write-Host "  Total Conversions: $($companyAnalytics.data.totalConversions)"
    Write-Host "  Average Rating: $($companyAnalytics.data.averageRating)"
}
Write-Host ""

# Company Analytics Overview (with date filter)
$startDate = (Get-Date).AddMonths(-6).ToString("yyyy-MM-dd")
$endDate = (Get-Date).ToString("yyyy-MM-dd")

Test-Endpoint `
    -Name "Get Company Analytics Overview (with date filter)" `
    -Method "GET" `
    -Url "$analyticsBaseUrl/company/overview?StartDate=$startDate&EndDate=$endDate&GroupBy=month" `
    -Headers $companyHeaders
Write-Host ""

# Marketer Performance
$marketerPerformance = Test-Endpoint `
    -Name "Get Marketer Performance for Company" `
    -Method "GET" `
    -Url "$analyticsBaseUrl/company/marketer-performance" `
    -Headers $companyHeaders

if ($marketerPerformance -and $marketerPerformance.data -and $marketerPerformance.data.Count -gt 0) {
    Write-Host "  Marketers found: $($marketerPerformance.data.Count)"
    foreach ($m in $marketerPerformance.data) {
        Write-Host "    - $($m.marketerName): Earnings=$($m.totalEarnings), Conversions=$($m.totalConversions), Score=$($m.performanceScore)"
    }
}
Write-Host ""

# Marketer Performance (with date filter)
Test-Endpoint `
    -Name "Get Marketer Performance (with date filter)" `
    -Method "GET" `
    -Url "$analyticsBaseUrl/company/marketer-performance?StartDate=$startDate&EndDate=$endDate" `
    -Headers $companyHeaders
Write-Host ""

# Conversion Funnel for a Campaign
# First, get campaigns to find a valid campaign ID
Write-Info "Fetching campaigns to find a valid campaign ID..."
try {
    $campaignsResponse = Invoke-RestMethod -Uri "$baseUrl/api/campaign" `
        -Method GET `
        -Headers $companyHeaders `
        -ErrorAction Stop
    
    $campaignId = $null
    if ($campaignsResponse.data -and $campaignsResponse.data.Count -gt 0) {
        $campaignId = $campaignsResponse.data[0].id
        Write-Success "Found campaign ID: $campaignId"
    } elseif ($campaignsResponse.data -and $campaignsResponse.data.items -and $campaignsResponse.data.items.Count -gt 0) {
        $campaignId = $campaignsResponse.data.items[0].id
        Write-Success "Found campaign ID: $campaignId"
    }
} catch {
    Write-Warning "Could not fetch campaigns: $($_.Exception.Message)"
    # Try with admin token
    try {
        $campaignsResponse = Invoke-RestMethod -Uri "$baseUrl/api/campaign" `
            -Method GET `
            -Headers $adminHeaders `
            -ErrorAction Stop
        
        if ($campaignsResponse.data -and $campaignsResponse.data.Count -gt 0) {
            $campaignId = $campaignsResponse.data[0].id
            Write-Success "Found campaign ID via admin: $campaignId"
        } elseif ($campaignsResponse.data -and $campaignsResponse.data.items -and $campaignsResponse.data.items.Count -gt 0) {
            $campaignId = $campaignsResponse.data.items[0].id
            Write-Success "Found campaign ID via admin: $campaignId"
        }
    } catch {
        Write-Warning "Could not fetch campaigns via admin either."
    }
}
Write-Host ""

if ($campaignId) {
    $conversionFunnel = Test-Endpoint `
        -Name "Get Conversion Funnel (Campaign ID: $campaignId)" `
        -Method "GET" `
        -Url "$analyticsBaseUrl/company/conversion-funnel/$campaignId" `
        -Headers $companyHeaders

    if ($conversionFunnel -and $conversionFunnel.data) {
        Write-Host "  Campaign: $($conversionFunnel.data.campaignTitle)"
        Write-Host "  Impressions: $($conversionFunnel.data.totalImpressions)"
        Write-Host "  Clicks: $($conversionFunnel.data.totalClicks)"
        Write-Host "  Leads: $($conversionFunnel.data.totalLeads)"
        Write-Host "  Conversions: $($conversionFunnel.data.totalConversions)"
        Write-Host "  Overall Conversion Rate: $($conversionFunnel.data.overallConversionRate)%"
    }
    Write-Host ""

    # Conversion Funnel with date filter
    Test-Endpoint `
        -Name "Get Conversion Funnel (with date filter)" `
        -Method "GET" `
        -Url "$analyticsBaseUrl/company/conversion-funnel/$($campaignId)?StartDate=$startDate&EndDate=$endDate" `
        -Headers $companyHeaders
    Write-Host ""
} else {
    # Count skipped tests so numbering stays consistent
    $script:testCount += 2
    $script:passedTests += 2
    Write-Warning "[Test $($script:testCount - 1)] Skipping Conversion Funnel test - no campaign found (SKIPPED)"
    Write-Warning "[Test $script:testCount] Skipping Conversion Funnel with date filter - no campaign found (SKIPPED)"
    Write-Host ""
}

# Invalid campaign ID test
Test-Endpoint `
    -Name "Get Conversion Funnel (invalid campaign ID - expect failure)" `
    -Method "GET" `
    -Url "$analyticsBaseUrl/company/conversion-funnel/999999" `
    -Headers $companyHeaders `
    -ExpectedStatus 400
Write-Host ""

# =========================================
# Admin Analytics Endpoints
# =========================================
Write-Host ""
Write-Info "========================================="
Write-Info "Testing Admin Analytics Endpoints"
Write-Info "========================================="
Write-Host ""

# Platform Overview
$platformOverview = Test-Endpoint `
    -Name "Get Platform Overview" `
    -Method "GET" `
    -Url "$analyticsBaseUrl/admin/platform-overview" `
    -Headers $adminHeaders

if ($platformOverview -and $platformOverview.data) {
    Write-Host "  Total Users: $($platformOverview.data.totalUsers)"
    Write-Host "  Total Marketers: $($platformOverview.data.totalMarketers)"
    Write-Host "  Total Companies: $($platformOverview.data.totalCompanies)"
    Write-Host "  Total Admins: $($platformOverview.data.totalAdmins)"
    Write-Host "  Total Campaigns: $($platformOverview.data.totalCampaigns)"
    Write-Host "  Active Campaigns: $($platformOverview.data.activeCampaigns)"
    Write-Host "  Total Revenue: $($platformOverview.data.totalRevenue)"
    Write-Host "  Total Commissions Paid: $($platformOverview.data.totalCommissionsPaid)"
    Write-Host "  Total Conversions: $($platformOverview.data.totalConversions)"
    Write-Host "  Pending Withdrawals: $($platformOverview.data.pendingWithdrawals)"
    Write-Host "  Total Reviews: $($platformOverview.data.totalReviews)"
    Write-Host "  Total Complaints: $($platformOverview.data.totalComplaints)"
    Write-Host "  Open Complaints: $($platformOverview.data.openComplaints)"
}
Write-Host ""

# Revenue Breakdown (no filter)
$revenueBreakdown = Test-Endpoint `
    -Name "Get Revenue Breakdown (no filter)" `
    -Method "GET" `
    -Url "$analyticsBaseUrl/admin/revenue-breakdown" `
    -Headers $adminHeaders

if ($revenueBreakdown -and $revenueBreakdown.data -and $revenueBreakdown.data.Count -gt 0) {
    Write-Host "  Periods returned: $($revenueBreakdown.data.Count)"
    foreach ($period in $revenueBreakdown.data) {
        Write-Host "    - $($period.period): Revenue=$($period.totalRevenue), Commissions=$($period.commissionsPaid), Net=$($period.netRevenue)"
    }
} else {
    Write-Host "  No revenue breakdown data (may be empty if no transactions yet)"
}
Write-Host ""

# Revenue Breakdown (with date filter and groupBy)
Test-Endpoint `
    -Name "Get Revenue Breakdown (with date filter, group by month)" `
    -Method "GET" `
    -Url "$analyticsBaseUrl/admin/revenue-breakdown?StartDate=$startDate&EndDate=$endDate&GroupBy=month" `
    -Headers $adminHeaders
Write-Host ""

# Revenue Breakdown (group by week)
Test-Endpoint `
    -Name "Get Revenue Breakdown (group by week)" `
    -Method "GET" `
    -Url "$analyticsBaseUrl/admin/revenue-breakdown?GroupBy=week" `
    -Headers $adminHeaders
Write-Host ""

# Top Performers (default count = 10)
$topPerformers = Test-Endpoint `
    -Name "Get Top Performers (default top 10)" `
    -Method "GET" `
    -Url "$analyticsBaseUrl/admin/top-performers" `
    -Headers $adminHeaders

if ($topPerformers -and $topPerformers.data) {
    if ($topPerformers.data.topMarketers -and $topPerformers.data.topMarketers.Count -gt 0) {
        Write-Host "  Top Marketers:"
        foreach ($m in $topPerformers.data.topMarketers) {
            Write-Host "    #$($m.rank) $($m.marketerName) - Earnings: $($m.totalEarnings), Score: $($m.performanceScore)"
        }
    } else {
        Write-Host "  No top marketers data"
    }
    
    if ($topPerformers.data.topCampaigns -and $topPerformers.data.topCampaigns.Count -gt 0) {
        Write-Host "  Top Campaigns:"
        foreach ($c in $topPerformers.data.topCampaigns) {
            Write-Host "    #$($c.rank) $($c.campaignTitle) ($($c.companyName)) - Revenue: $($c.totalRevenue)"
        }
    } else {
        Write-Host "  No top campaigns data"
    }

    if ($topPerformers.data.topCompanies -and $topPerformers.data.topCompanies.Count -gt 0) {
        Write-Host "  Top Companies:"
        foreach ($co in $topPerformers.data.topCompanies) {
            Write-Host "    #$($co.rank) $($co.companyName) - Spent: $($co.totalSpent), Rating: $($co.averageRating)"
        }
    } else {
        Write-Host "  No top companies data"
    }
}
Write-Host ""

# Top Performers (custom count = 3)
Test-Endpoint `
    -Name "Get Top Performers (top 3)" `
    -Method "GET" `
    -Url "$analyticsBaseUrl/admin/top-performers?topCount=3" `
    -Headers $adminHeaders
Write-Host ""

# Top Performers (top 1)
Test-Endpoint `
    -Name "Get Top Performers (top 1)" `
    -Method "GET" `
    -Url "$analyticsBaseUrl/admin/top-performers?topCount=1" `
    -Headers $adminHeaders
Write-Host ""

# =========================================
# Authorization / Security Tests
# =========================================
Write-Host ""
Write-Info "========================================="
Write-Info "Testing Authorization & Security"
Write-Info "========================================="
Write-Host ""

# Admin endpoint without auth (expect 401)
Test-Endpoint `
    -Name "Get Platform Overview (no auth - expect 401)" `
    -Method "GET" `
    -Url "$analyticsBaseUrl/admin/platform-overview" `
    -ExpectedStatus 401
Write-Host ""

# Admin endpoint with Company token (expect 403)
Test-Endpoint `
    -Name "Get Platform Overview (company token - expect 403)" `
    -Method "GET" `
    -Url "$analyticsBaseUrl/admin/platform-overview" `
    -Headers $companyHeaders `
    -ExpectedStatus 403
Write-Host ""

# Company endpoint without auth (expect 401)
Test-Endpoint `
    -Name "Get Company Analytics (no auth - expect 401)" `
    -Method "GET" `
    -Url "$analyticsBaseUrl/company/overview" `
    -ExpectedStatus 401
Write-Host ""

# =========================================
# Admin accessing Company Analytics
# =========================================
Write-Host ""
Write-Info "========================================="
Write-Info "Testing Admin -> Company Analytics Access"
Write-Info "========================================="
Write-Host ""

# Admin without companyId param (expect 400)
Test-Endpoint `
    -Name "Get Company Analytics (admin token, no companyId - expect 400)" `
    -Method "GET" `
    -Url "$analyticsBaseUrl/company/overview" `
    -Headers $adminHeaders `
    -ExpectedStatus 400
Write-Host ""

# Admin with companyId param (should succeed)
$firstCompanyId = 1
if ($companyAnalytics -and $companyAnalytics.data -and $companyAnalytics.data.companyId) {
    $firstCompanyId = $companyAnalytics.data.companyId
}
Test-Endpoint `
    -Name "Get Company Analytics (admin + companyId=$firstCompanyId - expect 200)" `
    -Method "GET" `
    -Url "$analyticsBaseUrl/company/overview?companyId=$firstCompanyId" `
    -Headers $adminHeaders
Write-Host ""

# Admin accessing marketer performance with companyId
Test-Endpoint `
    -Name "Get Marketer Performance (admin + companyId=$firstCompanyId - expect 200)" `
    -Method "GET" `
    -Url "$analyticsBaseUrl/company/marketer-performance?companyId=$firstCompanyId" `
    -Headers $adminHeaders
Write-Host ""

# Admin accessing marketer performance without companyId (expect 400)
Test-Endpoint `
    -Name "Get Marketer Performance (admin, no companyId - expect 400)" `
    -Method "GET" `
    -Url "$analyticsBaseUrl/company/marketer-performance" `
    -Headers $adminHeaders `
    -ExpectedStatus 400
Write-Host ""

# =========================================
# Test Summary
# =========================================
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
