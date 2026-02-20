# Campaign API Testing Guide

## Base URL
```
https://localhost:7253/api/campaign
```

## Authentication
Different endpoints require different roles:
- **Public**: No authentication required
- **Marketer**: Include JWT token with Marketer role
- **Company**: Include JWT token with Company role  
- **Admin**: Include JWT token with Admin role

**Authorization Header:**
```
Authorization: Bearer {your_jwt_token}
```

---

## 🌐 Public Endpoints (No Auth Required)

### 1. Get All Campaigns
**Endpoint:** `GET /api/campaign`

**Query Parameters:**
- `page` (optional): Page number (default: 1)
- `pageSize` (optional): Items per page (default: 10)
- `status` (optional): Campaign status (0=Pending, 1=Active, 2=Inactive, 3=Paused, 4=Completed, 5=Rejected)
- `categoryId` (optional): Filter by category
- `companyId` (optional): Filter by company
- `startDateFrom` (optional): Filter start date from
- `startDateTo` (optional): Filter start date to

**Request:**
```http
GET https://localhost:7253/api/campaign?page=1&pageSize=10
```

**Expected Response (200 OK):**
```json
{
  "success": true,
  "message": "Campaigns retrieved successfully",
  "data": {
    "items": [
      {
        "id": 1,
        "title": "Summer Sale Campaign",
        "description": "Promote our summer sale with 20% discount",
        "commissionType": 0,
        "commissionValue": 15.5,
        "budget": 10000.00,
        "startDate": "2026-02-10T00:00:00",
        "endDate": "2026-03-10T00:00:00",
        "status": 1,
        "promotionalMaterials": "Banners and social media content available",
        "trackingBaseUrl": "https://example.com/track",
        "responseNote": null,
        "createdAt": "2026-02-05T10:00:00",
        "companyId": 1,
        "companyName": "TechCorp Inc.",
        "categoryId": 1,
        "categoryName": "Technology",
        "applicationsCount": 5,
        "acceptedApplicationsCount": 3,
        "isActive": true,
        "daysRemaining": 28,
        "approvedByName": "Admin User"
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 25,
    "totalPages": 3
  }
}
```

---

### 2. Get Active Campaigns
**Endpoint:** `GET /api/campaign/active`

**Query Parameters:**
- `page` (optional): Page number
- `pageSize` (optional): Items per page

**Request:**
```http
GET https://localhost:7253/api/campaign/active?page=1&pageSize=10
```

**Expected Response:** Same structure as Get All Campaigns, but only returns active campaigns within date range.

---

### 3. Get Campaign by ID
**Endpoint:** `GET /api/campaign/{id}`

**Request:**
```http
GET https://localhost:7253/api/campaign/1
```

**Expected Response (200 OK):**
```json
{
  "success": true,
  "message": "Campaign retrieved successfully",
  "data": {
    "id": 1,
    "title": "Summer Sale Campaign",
    "description": "Promote our summer sale with 20% discount",
    "commissionType": 0,
    "commissionValue": 15.5,
    "budget": 10000.00,
    "startDate": "2026-02-10T00:00:00",
    "endDate": "2026-03-10T00:00:00",
    "status": 1,
    "createdAt": "2026-02-05T10:00:00",
    "companyId": 1,
    "companyName": "TechCorp Inc.",
    "categoryId": 1,
    "categoryName": "Technology",
    "applicationsCount": 5,
    "acceptedApplicationsCount": 3,
    "isActive": true,
    "daysRemaining": 28,
    "company": {
      "id": 1,
      "companyName": "TechCorp Inc.",
      "logoUrl": "https://example.com/logo.png",
      "website": "https://techcorp.com",
      "isVerified": true
    },
    "category": {
      "id": 1,
      "nameEn": "Technology",
      "nameAr": "التكنولوجيا",
      "slug": "technology",
      "icon": "💻"
    },
    "statistics": null
  }
}
```

**Error Response (404 Not Found):**
```json
{
  "success": false,
  "message": "Campaign not found"
}
```

---

### 4. Search Campaigns
**Endpoint:** `GET /api/campaign/search`

**Query Parameters:**
- `keyword` (optional): Search in title and description
- `categoryId` (optional): Filter by category
- `minCommission` (optional): Minimum commission value
- `commissionType` (optional): 0=Percentage, 1=Fixed
- `startDateFrom` (optional): Start date from
- `startDateTo` (optional): Start date to
- `isActive` (optional): true/false
- `page` (optional): Page number
- `pageSize` (optional): Items per page

**Request:**
```http
GET https://localhost:7253/api/campaign/search?keyword=sale&minCommission=10&isActive=true&page=1
```

**Expected Response:** Same structure as Get All Campaigns.

---

### 5. Get Campaigns by Category
**Endpoint:** `GET /api/campaign/category/{categoryId}`

**Request:**
```http
GET https://localhost:7253/api/campaign/category/1?page=1&pageSize=10
```

**Expected Response:** Same structure as Get All Campaigns.

---

### 6. Get Campaigns by Company
**Endpoint:** `GET /api/campaign/company/{companyId}`

**Request:**
```http
GET https://localhost:7253/api/campaign/company/1?page=1&pageSize=10
```

**Expected Response:** Same structure as Get All Campaigns.

---

### 7. Get Campaigns by Status
**Endpoint:** `GET /api/campaign/status/{status}`

**Status Values:**
- 0 = Pending
- 1 = Active
- 2 = Inactive
- 3 = Paused
- 4 = Completed
- 5 = Rejected

**Request:**
```http
GET https://localhost:7253/api/campaign/status/1?page=1&pageSize=10
```

**Expected Response:** Same structure as Get All Campaigns.

---

## 👤 Marketer Endpoints

### 8. Get Recommended Campaigns
**Endpoint:** `GET /api/campaign/recommended`  
**Auth:** Marketer role required

**Query Parameters:**
- `limit` (optional): Number of recommendations (default: 10)

**Request:**
```http
GET https://localhost:7253/api/campaign/recommended?limit=10
Authorization: Bearer {marketer_token}
```

**Expected Response (200 OK):**
```json
{
  "success": true,
  "message": "Recommended campaigns retrieved successfully",
  "data": {
    "items": [
      {
        "id": 5,
        "title": "AI-Recommended Campaign",
        "commissionValue": 20.0,
        "status": 1,
        "isActive": true
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 5
  }
}
```

---

### 9. Apply to Campaign
**Endpoint:** `POST /api/campaign/{id}/apply`  
**Auth:** Marketer role required

**Request:**
```http
POST https://localhost:7253/api/campaign/1/apply
Authorization: Bearer {marketer_token}
```

**Expected Response (200 OK):**
```json
{
  "success": true,
  "message": "Application submitted successfully",
  "data": {
    "id": 10,
    "campaignId": 1,
    "campaignTitle": "Summer Sale Campaign",
    "marketerId": 5,
    "marketerName": "John Doe",
    "status": 0,
    "aiMatchScore": 85.5,
    "appliedAt": "2026-02-09T14:30:00",
    "respondedAt": null,
    "responseNote": null
  }
}
```

**Error Responses:**
```json
// Already applied
{
  "success": false,
  "message": "You have already applied to this campaign"
}

// Campaign not active
{
  "success": false,
  "message": "Campaign is not active"
}
```

---

### 10. Withdraw Application
**Endpoint:** `POST /api/campaign/applications/{applicationId}/withdraw`  
**Auth:** Marketer role required

**Request:**
```http
POST https://localhost:7253/api/campaign/applications/10/withdraw
Authorization: Bearer {marketer_token}
```

**Expected Response (200 OK):**
```json
{
  "success": true,
  "message": "Application withdrawn successfully",
  "data": true
}
```

**Error Response:**
```json
{
  "success": false,
  "message": "Can only withdraw pending applications"
}
```

---

## 🏢 Company Endpoints

### 11. Get My Campaigns
**Endpoint:** `GET /api/campaign/my-campaigns`  
**Auth:** Company role required

**Query Parameters:** Same as Get All Campaigns

**Request:**
```http
GET https://localhost:7253/api/campaign/my-campaigns?page=1&pageSize=10
Authorization: Bearer {company_token}
```

**Expected Response:** Same structure as Get All Campaigns, but only company's own campaigns.

---

### 12. Get My Campaign by ID
**Endpoint:** `GET /api/campaign/my-campaigns/{id}`  
**Auth:** Company role required

**Request:**
```http
GET https://localhost:7253/api/campaign/my-campaigns/1
Authorization: Bearer {company_token}
```

**Expected Response:** Same structure as Get Campaign by ID, but includes statistics.

---

### 13. Create Campaign
**Endpoint:** `POST /api/campaign`  
**Auth:** Company role required

**Request Body:**
```json
{
  "title": "New Product Launch Campaign",
  "description": "Promote our new product line with exclusive offers",
  "categoryId": 1,
  "commissionType": 0,
  "commissionValue": 12.5,
  "budget": 5000.00,
  "startDate": "2026-02-15T00:00:00",
  "endDate": "2026-03-15T00:00:00",
  "promotionalMaterials": "Product images and marketing copy available",
  "trackingBaseUrl": "https://example.com/track"
}
```

**Validation Rules:**
- `title`: Required, 10-200 characters
- `categoryId`: Required, must exist
- `commissionValue`: Required, must be > 0
- `endDate`: Must be after startDate
- `startDate`: Cannot be in the past

**Expected Response (201 Created):**
```json
{
  "success": true,
  "message": "Campaign created successfully and pending admin approval",
  "data": {
    "id": 25,
    "title": "New Product Launch Campaign",
    "status": 0,
    "createdAt": "2026-02-09T15:00:00"
  }
}
```

**Error Responses:**
```json
// Company not verified
{
  "success": false,
  "message": "Company must be verified to create campaigns"
}

// Invalid dates
{
  "success": false,
  "message": "End date must be after start date"
}
```

---

### 14. Update Campaign
**Endpoint:** `PUT /api/campaign/{id}`  
**Auth:** Company role required

**Request Body:** (all fields optional)
```json
{
  "description": "Updated campaign description",
  "promotionalMaterials": "Updated materials available",
  "budget": 6000.00
}
```

**Restrictions:**
- Can only edit Pending or Paused campaigns fully
- Active campaigns: Only Description and PromotionalMaterials can be updated
- Cannot edit Completed or Rejected campaigns

**Expected Response (200 OK):**
```json
{
  "success": true,
  "message": "Campaign updated successfully",
  "data": {
    "id": 25,
    "title": "New Product Launch Campaign",
    "description": "Updated campaign description",
    "status": 0
  }
}
```

---

### 15. Pause Campaign
**Endpoint:** `PUT /api/campaign/{id}/pause`  
**Auth:** Company role required

**Request:**
```http
PUT https://localhost:7253/api/campaign/1/pause
Authorization: Bearer {company_token}
```

**Expected Response (200 OK):**
```json
{
  "success": true,
  "message": "Campaign paused successfully",
  "data": true
}
```

**Error Response:**
```json
{
  "success": false,
  "message": "Only active campaigns can be paused"
}
```

---

### 16. Resume Campaign
**Endpoint:** `PUT /api/campaign/{id}/resume`  
**Auth:** Company role required

**Request:**
```http
PUT https://localhost:7253/api/campaign/1/resume
Authorization: Bearer {company_token}
```

**Expected Response (200 OK):**
```json
{
  "success": true,
  "message": "Campaign resumed successfully",
  "data": true
}
```

---

### 17. Update Campaign Status
**Endpoint:** `PUT /api/campaign/{id}/status`  
**Auth:** Company role required

**Request Body:** Status value (1=Active, 2=Inactive, 3=Paused, 4=Completed)
```json
1
```

**Expected Response (200 OK):**
```json
{
  "success": true,
  "message": "Campaign status updated to Active",
  "data": true
}
```

**Error Response:**
```json
{
  "success": false,
  "message": "Only admin can set campaign to Pending or Rejected status"
}
```

---

### 18. Delete Campaign
**Endpoint:** `DELETE /api/campaign/{id}`  
**Auth:** Company role required

**Request:**
```http
DELETE https://localhost:7253/api/campaign/1
Authorization: Bearer {company_token}
```

**Expected Response (200 OK):**
```json
// Soft delete (has activity)
{
  "success": true,
  "message": "Campaign marked as inactive (soft delete) due to existing activity",
  "data": true
}

// Hard delete (no activity)
{
  "success": true,
  "message": "Campaign deleted successfully",
  "data": true
}
```

**Error Response:**
```json
{
  "success": false,
  "message": "Cannot delete active campaigns. Please pause or complete it first."
}
```

---

## 📊 Application Management (Company)

### 19. Get Campaign Applications
**Endpoint:** `GET /api/campaign/{id}/applications`  
**Auth:** Company role required

**Query Parameters:**
- `status` (optional): 0=Pending, 1=Accepted, 2=Rejected, 3=Withdrawn
- `page` (optional): Page number
- `pageSize` (optional): Items per page

**Request:**
```http
GET https://localhost:7253/api/campaign/1/applications?status=0&page=1&pageSize=10
Authorization: Bearer {company_token}
```

**Expected Response (200 OK):**
```json
{
  "success": true,
  "message": "Applications retrieved successfully",
  "data": {
    "items": [
      {
        "id": 15,
        "campaignId": 1,
        "campaignTitle": "Summer Sale Campaign",
        "marketerId": 8,
        "marketerName": "Jane Smith",
        "status": 0,
        "aiMatchScore": 92.3,
        "appliedAt": "2026-02-08T10:00:00",
        "respondedAt": null,
        "responseNote": null
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 15
  }
}
```

---

### 20. Approve Application
**Endpoint:** `POST /api/campaign/applications/{applicationId}/approve`  
**Auth:** Company role required

**Request Body:** (optional)
```json
{
  "applicationId": 15,
  "note": "Approved - Great profile!"
}
```

**Request:**
```http
POST https://localhost:7253/api/campaign/applications/15/approve
Authorization: Bearer {company_token}
Content-Type: application/json

{
  "note": "Approved - Great profile!"
}
```

**Expected Response (200 OK):**
```json
{
  "success": true,
  "message": "Application approved and tracking link created successfully",
  "data": true
}
```

**What Happens:**
- Application status → Accepted
- Tracking link automatically created for marketer
- RespondedAt timestamp set

**Error Response:**
```json
{
  "success": false,
  "message": "Only pending applications can be approved"
}
```

---

### 21. Reject Application
**Endpoint:** `POST /api/campaign/applications/{applicationId}/reject`  
**Auth:** Company role required

**Request Body:** (note is required)
```json
{
  "applicationId": 15,
  "note": "Profile doesn't match our requirements"
}
```

**Request:**
```http
POST https://localhost:7253/api/campaign/applications/15/reject
Authorization: Bearer {company_token}
Content-Type: application/json

{
  "note": "Profile doesn't match our requirements"
}
```

**Expected Response (200 OK):**
```json
{
  "success": true,
  "message": "Application rejected successfully",
  "data": true
}
```

**Error Response:**
```json
{
  "success": false,
  "message": "Rejection note is required"
}
```

---

### 22. Get Campaign Statistics
**Endpoint:** `GET /api/campaign/{id}/statistics`  
**Auth:** Company role required

**Query Parameters:**
- `from` (optional): Date range from
- `to` (optional): Date range to

**Request:**
```http
GET https://localhost:7253/api/campaign/1/statistics?from=2026-02-01&to=2026-02-09
Authorization: Bearer {company_token}
```

**Expected Response (200 OK):**
```json
{
  "success": true,
  "message": "Statistics retrieved successfully",
  "data": {
    "totalApplications": 20,
    "pendingApplications": 5,
    "acceptedApplications": 12,
    "rejectedApplications": 2,
    "withdrawnApplications": 1,
    "totalClicks": 1250,
    "totalConversions": 85,
    "totalEarnings": 4250.50,
    "totalSpent": 4250.50,
    "remainingBudget": 5749.50,
    "dateFrom": "2026-02-01T00:00:00",
    "dateTo": "2026-02-09T23:59:59",
    "conversionRate": 6.80,
    "averageRoi": 15.25
  }
}
```

---

## 👨‍💼 Admin Endpoints

### 23. Approve Campaign
**Endpoint:** `PUT /api/campaign/{id}/admin/approve`  
**Auth:** Admin role required

**Request Body:** (optional response note)
```json
"Campaign meets all requirements - Approved!"
```

**Request:**
```http
PUT https://localhost:7253/api/campaign/25/admin/approve
Authorization: Bearer {admin_token}
Content-Type: application/json

"Campaign meets all requirements - Approved!"
```

**Expected Response (200 OK):**
```json
{
  "success": true,
  "message": "Campaign approved successfully",
  "data": {
    "id": 25,
    "title": "New Product Launch Campaign",
    "status": 1,
    "approvedByName": "Admin User"
  }
}
```

**What Happens:**
- Campaign status → Active
- ApprovedBy field set to admin's ID
- Campaign becomes visible to marketers

**Error Response:**
```json
{
  "success": false,
  "message": "Only pending campaigns can be approved"
}
```

---

### 24. Reject Campaign
**Endpoint:** `PUT /api/campaign/{id}/admin/reject`  
**Auth:** Admin role required

**Request Body:** (note is required)
```json
"Campaign does not meet community guidelines"
```

**Request:**
```http
PUT https://localhost:7253/api/campaign/25/admin/reject
Authorization: Bearer {admin_token}
Content-Type: application/json

"Campaign does not meet community guidelines"
```

**Expected Response (200 OK):**
```json
{
  "success": true,
  "message": "Campaign rejected",
  "data": {
    "id": 25,
    "title": "New Product Launch Campaign",
    "status": 5,
    "responseNote": "Campaign does not meet community guidelines"
  }
}
```

**Error Response:**
```json
{
  "success": false,
  "message": "Rejection note is required"
}
```

---

## 📝 Common HTTP Status Codes

- **200 OK**: Request successful
- **201 Created**: Resource created successfully (Create Campaign)
- **400 Bad Request**: Invalid request data or business rule violation
- **401 Unauthorized**: Missing or invalid authentication token
- **403 Forbidden**: Insufficient permissions for the requested action
- **404 Not Found**: Campaign or resource not found

---

## 🔍 Testing Tips

1. **Test Order:**
   - First test public endpoints (no auth)
   - Login as different roles to get tokens
   - Test role-specific endpoints in order

2. **Campaign Lifecycle:**
   ```
   Create (Company) → Pending
   Approve (Admin) → Active
   Pause (Company) → Paused
   Resume (Company) → Active
   Complete/Delete (Company)
   ```

3. **Application Lifecycle:**
   ```
   Apply (Marketer) → Pending
   Approve (Company) → Accepted + TrackingLink created
   OR
   Reject (Company) → Rejected
   OR
   Withdraw (Marketer) → Withdrawn
   ```

4. **Use the PowerShell Script:**
   Run `Campaign_Test_Script.ps1` to automatically test all endpoints.

---

## 🎯 Quick Test Checklist

- [ ] Get all campaigns (public)
- [ ] Get active campaigns (public)
- [ ] Search campaigns (public)
- [ ] Get recommended campaigns (marketer)
- [ ] Apply to campaign (marketer)
- [ ] Withdraw application (marketer)
- [ ] Create campaign (company)
- [ ] Update campaign (company)
- [ ] Get my campaigns (company)
- [ ] Get campaign applications (company)
- [ ] Approve application (company)
- [ ] Reject application (company)
- [ ] Get campaign statistics (company)
- [ ] Pause/Resume campaign (company)
- [ ] Approve campaign (admin)
- [ ] Reject campaign (admin)
- [ ] Delete campaign (company)

---

## 📞 Support

If you encounter any issues:
1. Check application is running on `https://localhost:7253`
2. Verify authentication tokens are valid
3. Ensure test data exists (categories, companies, marketers)
4. Check server logs for detailed error messages
