# Category API Testing Guide

## Base URL
```
https://localhost:7253/api/category
```

## Authentication
For Admin endpoints, you need to include JWT token in the header:
```
Authorization: Bearer {your_jwt_token}
```

---

## 📋 GET Endpoints (Public - No Auth Required)

### 1. Get All Categories
**Endpoint:** `GET /api/category`

**Request:**
```http
GET https://localhost:7253/api/category
```

**Expected Response (200 OK):**
```json
{
  "success": true,
  "message": "Categories retrieved successfully",
  "data": [
    {
      "id": 1,
      "nameEn": "Technology",
      "nameAr": "التكنولوجيا",
      "slug": "technology",
      "icon": "💻",
      "parentId": null,
      "parentName": null,
      "childrenCount": 2,
      "campaignsCount": 5
    }
  ]
}
```

---

### 2. Get Root Categories
**Endpoint:** `GET /api/category/roots`

**Request:**
```http
GET https://localhost:7253/api/category/roots
```

**Expected Response (200 OK):**
```json
{
  "success": true,
  "message": "Root categories retrieved successfully",
  "data": [
    {
      "id": 1,
      "nameEn": "Technology",
      "nameAr": "التكنولوجيا",
      "slug": "technology",
      "icon": "💻",
      "parentId": null,
      "parentName": null,
      "childrenCount": 2,
      "campaignsCount": 5
    }
  ]
}
```

---

### 3. Get Category by ID
**Endpoint:** `GET /api/category/{id}`

**Request:**
```http
GET https://localhost:7253/api/category/1
```

**Expected Response (200 OK):**
```json
{
  "success": true,
  "message": "Category retrieved successfully",
  "data": {
    "id": 1,
    "nameEn": "Technology",
    "nameAr": "التكنولوجيا",
    "slug": "technology",
    "icon": "💻",
    "parentId": null,
    "parentName": null,
    "childrenCount": 2,
    "campaignsCount": 5,
    "children": [
      {
        "id": 9,
        "nameEn": "Software",
        "nameAr": "البرمجيات",
        "slug": "software",
        "icon": "💿",
        "parentId": 1,
        "parentName": "Technology",
        "childrenCount": 0,
        "campaignsCount": 2
      }
    ],
    "parent": null
  }
}
```

**Error Response (404 Not Found):**
```json
{
  "success": false,
  "message": "Category not found"
}
```

---

### 4. Get Category Children
**Endpoint:** `GET /api/category/{id}/children`

**Request:**
```http
GET https://localhost:7253/api/category/1/children
```

**Expected Response (200 OK):**
```json
{
  "success": true,
  "message": "Category children retrieved successfully",
  "data": [
    {
      "id": 9,
      "nameEn": "Software",
      "nameAr": "البرمجيات",
      "slug": "software",
      "icon": "💿",
      "parentId": 1,
      "parentName": "Technology",
      "childrenCount": 0,
      "campaignsCount": 2
    }
  ]
}
```

---

### 5. Get Category Hierarchy (Tree)
**Endpoint:** `GET /api/category/hierarchy`

**Request:**
```http
GET https://localhost:7253/api/category/hierarchy
```

**Expected Response (200 OK):**
```json
{
  "success": true,
  "message": "Category hierarchy retrieved successfully",
  "data": {
    "rootCategories": [
      {
        "id": 1,
        "nameEn": "Technology",
        "nameAr": "التكنولوجيا",
        "slug": "technology",
        "icon": "💻",
        "parentId": null,
        "parentName": null,
        "childrenCount": 2,
        "campaignsCount": 5,
        "children": [
          {
            "id": 9,
            "nameEn": "Software",
            "nameAr": "البرمجيات",
            "slug": "software",
            "icon": "💿",
            "parentId": 1,
            "parentName": "Technology",
            "childrenCount": 0,
            "campaignsCount": 2,
            "children": []
          }
        ]
      }
    ]
  }
}
```

---

### 6. Get Category by Slug
**Endpoint:** `GET /api/category/slug/{slug}`

**Request:**
```http
GET https://localhost:7253/api/category/slug/technology
```

**Expected Response (200 OK):**
```json
{
  "success": true,
  "message": "Category retrieved successfully",
  "data": {
    "id": 1,
    "nameEn": "Technology",
    "nameAr": "التكنولوجيا",
    "slug": "technology",
    "icon": "💻",
    "parentId": null,
    "parentName": null,
    "childrenCount": 2,
    "campaignsCount": 5
  }
}
```

---

### 7. Get Category Campaigns (Paged)
**Endpoint:** `GET /api/category/{id}/campaigns?page=1&pageSize=10`

**Request:**
```http
GET https://localhost:7253/api/category/1/campaigns?page=1&pageSize=10
```

**Expected Response (200 OK):**
```json
{
  "success": true,
  "message": "Category campaigns retrieved successfully",
  "data": {
    "data": [],
    "page": 1,
    "pageSize": 10,
    "totalCount": 0,
    "totalPages": 0
  }
}
```

---

## 🔐 POST Endpoints (Admin Only - Auth Required)

### 8. Create Category
**Endpoint:** `POST /api/category`

**Headers:**
```
Authorization: Bearer {admin_jwt_token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "nameEn": "New Category",
  "nameAr": "فئة جديدة",
  "slug": "new-category",
  "icon": "⭐",
  "parentId": null
}
```

**Expected Response (200 OK):**
```json
{
  "success": true,
  "message": "Category created successfully",
  "data": {
    "id": 15,
    "nameEn": "New Category",
    "nameAr": "فئة جديدة",
    "slug": "new-category",
    "icon": "⭐",
    "parentId": null,
    "parentName": null,
    "childrenCount": 0,
    "campaignsCount": 0
  }
}
```

**Error Response (400 Bad Request - Duplicate Slug):**
```json
{
  "success": false,
  "message": "Category with this slug already exists"
}
```

**Error Response (400 Bad Request - Invalid Parent):**
```json
{
  "success": false,
  "message": "Parent category not found"
}
```

---

### 9. Create Categories Bulk
**Endpoint:** `POST /api/category/bulk`

**Headers:**
```
Authorization: Bearer {admin_jwt_token}
Content-Type: application/json
```

**Request Body:**
```json
[
  {
    "nameEn": "Category 1",
    "nameAr": "فئة 1",
    "slug": "category-1",
    "icon": "📦",
    "parentId": null
  },
  {
    "nameEn": "Category 2",
    "nameAr": "فئة 2",
    "slug": "category-2",
    "icon": "📦",
    "parentId": null
  }
]
```

**Expected Response (200 OK):**
```json
{
  "success": true,
  "message": "2 categories created successfully",
  "data": [
    {
      "id": 16,
      "nameEn": "Category 1",
      "nameAr": "فئة 1",
      "slug": "category-1",
      "icon": "📦",
      "parentId": null,
      "parentName": null,
      "childrenCount": 0,
      "campaignsCount": 0
    },
    {
      "id": 17,
      "nameEn": "Category 2",
      "nameAr": "فئة 2",
      "slug": "category-2",
      "icon": "📦",
      "parentId": null,
      "parentName": null,
      "childrenCount": 0,
      "campaignsCount": 0
    }
  ]
}
```

---

## ✏️ PUT Endpoints (Admin Only - Auth Required)

### 10. Update Category
**Endpoint:** `PUT /api/category/{id}`

**Headers:**
```
Authorization: Bearer {admin_jwt_token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "nameEn": "Updated Category Name",
  "nameAr": "اسم الفئة المحدث",
  "slug": "updated-category",
  "icon": "🔄"
}
```

**Expected Response (200 OK):**
```json
{
  "success": true,
  "message": "Category updated successfully",
  "data": {
    "id": 1,
    "nameEn": "Updated Category Name",
    "nameAr": "اسم الفئة المحدث",
    "slug": "updated-category",
    "icon": "🔄",
    "parentId": null,
    "parentName": null,
    "childrenCount": 2,
    "campaignsCount": 5
  }
}
```

**Error Response (400 Bad Request - Circular Reference):**
```json
{
  "success": false,
  "message": "Cannot set parent to a descendant category"
}
```

---

## 🗑️ DELETE Endpoints (Admin Only - Auth Required)

### 11. Delete Category
**Endpoint:** `DELETE /api/category/{id}`

**Headers:**
```
Authorization: Bearer {admin_jwt_token}
```

**Request:**
```http
DELETE https://localhost:7253/api/category/15
```

**Expected Response (200 OK):**
```json
{
  "success": true,
  "message": "Category deleted successfully",
  "data": true
}
```

**Error Response (400 Bad Request - Has Children):**
```json
{
  "success": false,
  "message": "Cannot delete category with children. Please delete or move children first."
}
```

---

### 12. Delete Category Safe
**Endpoint:** `DELETE /api/category/{id}/safe`

**Headers:**
```
Authorization: Bearer {admin_jwt_token}
```

**Request:**
```http
DELETE https://localhost:7253/api/category/15/safe
```

**Expected Response (200 OK):**
```json
{
  "success": true,
  "message": "Category deleted successfully",
  "data": true
}
```

**Error Response (400 Bad Request - Has Campaigns):**
```json
{
  "success": false,
  "message": "Cannot delete category. It has 3 associated campaign(s)."
}
```

---

## 🧪 Testing Checklist

### GET Endpoints (No Auth)
- [ ] ✅ Get All Categories
- [ ] ✅ Get Root Categories
- [ ] ✅ Get Category by ID (valid ID)
- [ ] ✅ Get Category by ID (invalid ID - should return 404)
- [ ] ✅ Get Category Children (valid parent ID)
- [ ] ✅ Get Category Children (invalid parent ID)
- [ ] ✅ Get Category Hierarchy
- [ ] ✅ Get Category by Slug (valid slug)
- [ ] ✅ Get Category by Slug (invalid slug - should return 404)
- [ ] ✅ Get Category Campaigns

### POST Endpoints (Admin Auth Required)
- [ ] ✅ Create Category (valid data)
- [ ] ✅ Create Category (duplicate slug - should fail)
- [ ] ✅ Create Category (invalid parent ID - should fail)
- [ ] ✅ Create Category (missing required fields - should fail)
- [ ] ✅ Create Categories Bulk (valid data)
- [ ] ✅ Create Categories Bulk (partial failure)

### PUT Endpoints (Admin Auth Required)
- [ ] ✅ Update Category (valid data)
- [ ] ✅ Update Category (circular reference - should fail)
- [ ] ✅ Update Category (invalid ID - should fail)
- [ ] ✅ Update Category (duplicate slug - should fail)

### DELETE Endpoints (Admin Auth Required)
- [ ] ✅ Delete Category (category without children)
- [ ] ✅ Delete Category (category with children - should fail)
- [ ] ✅ Delete Category Safe (category without campaigns)
- [ ] ✅ Delete Category Safe (category with campaigns - should fail)

---

## 📝 Postman Collection JSON

يمكنك استيراد هذا الـ JSON في Postman:

```json
{
  "info": {
    "name": "Category API",
    "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
  },
  "item": [
    {
      "name": "Get All Categories",
      "request": {
        "method": "GET",
        "header": [],
        "url": {
          "raw": "{{baseUrl}}/api/category",
          "host": ["{{baseUrl}}"],
          "path": ["api", "category"]
        }
      }
    },
    {
      "name": "Get Root Categories",
      "request": {
        "method": "GET",
        "header": [],
        "url": {
          "raw": "{{baseUrl}}/api/category/roots",
          "host": ["{{baseUrl}}"],
          "path": ["api", "category", "roots"]
        }
      }
    },
    {
      "name": "Create Category",
      "request": {
        "method": "POST",
        "header": [
          {
            "key": "Authorization",
            "value": "Bearer {{adminToken}}",
            "type": "text"
          },
          {
            "key": "Content-Type",
            "value": "application/json",
            "type": "text"
          }
        ],
        "body": {
          "mode": "raw",
          "raw": "{\n  \"nameEn\": \"New Category\",\n  \"nameAr\": \"فئة جديدة\",\n  \"slug\": \"new-category\",\n  \"icon\": \"⭐\",\n  \"parentId\": null\n}"
        },
        "url": {
          "raw": "{{baseUrl}}/api/category",
          "host": ["{{baseUrl}}"],
          "path": ["api", "category"]
        }
      }
    }
  ]
}
```

---

## 🔑 Getting Admin Token

1. Login as Admin:
```http
POST https://localhost:7253/api/account/login
Content-Type: application/json

{
  "email": "admin@affiliance.com",
  "password": "Admin@123"
}
```

2. Copy the `token` from response
3. Use it in Authorization header: `Bearer {token}`

---

## ⚠️ Common Issues

1. **401 Unauthorized**: Make sure you're logged in as Admin and token is valid
2. **404 Not Found**: Check if category ID exists
3. **400 Bad Request**: Check validation errors in response message
4. **Duplicate Slug**: Slug must be unique across all categories
