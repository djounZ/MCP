# API Security Implementation Guide

## 🔐 Security Overview

This API implements multiple layers of security including authentication, authorization, and rate limiting.

## Authentication Methods

### 1. JWT Bearer Authentication (Recommended)

**Login to get JWT token:**
```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "admin123"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresIn": 3600,
  "tokenType": "Bearer"
}
```

**Use token in requests:**
```http
GET /api/health/detailed
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### 2. API Key Authentication

**Add API key header:**
```http
GET /api/health/detailed
X-API-Key: api-key-12345
```

## Available Users & API Keys

### Test Users (JWT Login)
- **admin** / admin123 (roles: admin, user)
- **user** / user123 (roles: user)
- **test** / test123 (roles: user)

### Test API Keys
- **api-key-12345** → admin-user (admin role)
- **api-key-67890** → regular-user (user role)
- **test-api-key** → test-user (user role)

## Endpoints & Security

### Public Endpoints (No Authentication)
- `GET /api/health` - Basic health check

### User Endpoints (Requires Authentication)
- `GET /api/health/detailed` - Detailed health info
- `GET /api/auth/validate` - Validate token
- `POST /api/auth/refresh` - Refresh JWT token

### Admin-Only Endpoints
- `GET /api/health/admin` - Admin system metrics

### Authentication Endpoints
- `POST /api/auth/login` - Get JWT token

## Rate Limiting

- **Global**: 100 requests/minute per user
- **Standard API**: 10 requests/minute
- **Admin endpoints**: 50 requests/minute

## Example Usage

### 1. Login and Access Protected Resource
```bash
# 1. Login
curl -X POST http://localhost:5200/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}'

# 2. Use token (replace TOKEN with actual token)
curl -X GET http://localhost:5200/api/health/detailed \
  -H "Authorization: Bearer TOKEN"
```

### 2. Using API Key
```bash
curl -X GET http://localhost:5200/api/health/detailed \
  -H "X-API-Key: api-key-12345"
```

### 3. Access Admin Endpoint
```bash
# Only works with admin role
curl -X GET http://localhost:5200/api/health/admin \
  -H "Authorization: Bearer ADMIN_TOKEN"
```

## Security Features

### ✅ Implemented
- JWT Bearer authentication with configurable expiry
- API Key authentication with user mapping
- Role-based authorization (admin, user)
- Rate limiting with different tiers
- Secure token validation
- HTTPS redirection
- CORS configuration
- Request logging and monitoring

### 🔒 Production Considerations
1. **Use HTTPS only** (`RequireHttpsMetadata = true`)
2. **Strong JWT secret** (256+ bits, stored in secure config)
3. **Database user store** (replace mock user validation)
4. **Token refresh mechanism** with secure storage
5. **Audit logging** for security events
6. **API key rotation** policies
7. **Rate limiting per endpoint** customization
8. **Input validation** middleware

## Configuration

### JWT Settings (appsettings.json)
```json
{
  "Jwt": {
    "SecretKey": "YourVeryLongSecretKeyHere...",
    "Issuer": "MCP.WebApi",
    "Audience": "MCP.WebApi.Users",
    "ExpiryMinutes": 60
  }
}
```

### API Keys (appsettings.json)
```json
{
  "ApiKeys": {
    "your-api-key": "username"
  }
}
```

## Error Responses

### 401 Unauthorized
```json
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "status": 401
}
```

### 403 Forbidden
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.3",
  "title": "Forbidden", 
  "status": 403
}
```

### 429 Too Many Requests
```
Too many requests. Please try again later.
```
