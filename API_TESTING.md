# Kaafi Device Monitor - API Testing Guide

This document provides a comprehensive guide to testing all API endpoints using various tools.

## Prerequisites

1. The API must be running: `dotnet run`
2. Default URL: `http://localhost:5000` or `https://localhost:5001`
3. Install `jq` for JSON parsing: `sudo apt-get install jq` (Linux) or `brew install jq` (macOS)

## Authentication

All endpoints except `/api/auth/token` require JWT authentication. Get a token first:

```bash
# Get Token
TOKEN=$(curl -s -X POST http://localhost:5000/api/auth/token \
  -H "Content-Type: application/json" \
  -d '{"username":"biotime","password":"biotime9.5"}' | jq -r '.token')

echo $TOKEN
```

## API Endpoints Testing

### 1. Authentication

#### Get Token (Success)
```bash
curl -X POST http://localhost:5000/api/auth/token \
  -H "Content-Type: application/json" \
  -d '{
    "username": "biotime",
    "password": "biotime9.5"
  }' | jq .
```

Expected Response:
```json
{
  "success": true,
  "token": "eyJhbGc...",
  "expiresIn": 28800
}
```

#### Get Token (Failure - Wrong Credentials)
```bash
curl -X POST http://localhost:5000/api/auth/token \
  -H "Content-Type: application/json" \
  -d '{
    "username": "wrong",
    "password": "wrong"
  }' | jq .
```

Expected Response:
```json
{
  "success": false,
  "message": "Invalid credentials"
}
```

### 2. Device Connection

#### Connect to Device
```bash
curl -X POST http://localhost:5000/api/connect \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "ipAddress": "192.168.1.201",
    "port": 4370
  }' | jq .
```

Expected Response:
```json
{
  "success": true,
  "message": "Connected successfully",
  "deviceId": "MOCK-192-168-1-201"
}
```

#### Check Device Status
```bash
curl -X GET http://localhost:5000/api/status \
  -H "Authorization: Bearer $TOKEN" | jq .
```

Expected Response:
```json
{
  "isConnected": true,
  "deviceId": "MOCK-192-168-1-201"
}
```

#### Disconnect from Device
```bash
curl -X POST http://localhost:5000/api/disconnect \
  -H "Authorization: Bearer $TOKEN" | jq .
```

Expected Response:
```json
{
  "success": true,
  "message": "Disconnected successfully"
}
```

### 3. Enrollment Workflow

#### Start Enrollment
```bash
curl -X POST http://localhost:5000/api/start-enroll \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "employeeCode": "EMP001",
    "fullName": "John Doe",
    "fingerIndex": 0
  }' | jq .
```

Expected Response:
```json
{
  "success": true,
  "message": "Enrollment started successfully. Please place finger 3 times.",
  "employeeId": 1
}
```

#### Capture Fingerprint (1st)
```bash
curl -X POST http://localhost:5000/api/capture-fingerprint \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "employeeId": 1,
    "fingerIndex": 0,
    "captureNumber": 1
  }' | jq .
```

Expected Response:
```json
{
  "success": true,
  "message": "Fingerprint 1 captured successfully",
  "captureCount": 1,
  "isComplete": false,
  "template": null
}
```

#### Capture Fingerprint (2nd)
```bash
curl -X POST http://localhost:5000/api/capture-fingerprint \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "employeeId": 1,
    "fingerIndex": 0,
    "captureNumber": 2
  }' | jq .
```

Expected Response:
```json
{
  "success": true,
  "message": "Fingerprint 2 captured successfully",
  "captureCount": 2,
  "isComplete": false,
  "template": null
}
```

#### Capture Fingerprint (3rd - Final)
```bash
CAPTURE_RESPONSE=$(curl -s -X POST http://localhost:5000/api/capture-fingerprint \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "employeeId": 1,
    "fingerIndex": 0,
    "captureNumber": 3
  }')

echo $CAPTURE_RESPONSE | jq .
TEMPLATE=$(echo $CAPTURE_RESPONSE | jq -r '.template')
```

Expected Response:
```json
{
  "success": true,
  "message": "Fingerprint 3 captured successfully",
  "captureCount": 3,
  "isComplete": true,
  "template": "MOCK_TEMPLATE_3_..."
}
```

#### Save Enrollment
```bash
curl -X POST http://localhost:5000/api/save-enroll \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d "{
    \"employeeId\": 1,
    \"fingerIndex\": 0,
    \"template\": \"$TEMPLATE\"
  }" | jq .
```

Expected Response:
```json
{
  "success": true,
  "message": "Enrollment saved successfully",
  "enrollmentId": 1
}
```

### 4. Error Cases

#### Unauthorized Access (No Token)
```bash
curl -X POST http://localhost:5000/api/connect \
  -H "Content-Type: application/json" \
  -d '{
    "ipAddress": "192.168.1.201"
  }'
```

Expected Response: HTTP 401 Unauthorized

#### Invalid Finger Index
```bash
curl -X POST http://localhost:5000/api/start-enroll \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "employeeCode": "EMP001",
    "fullName": "John Doe",
    "fingerIndex": 15
  }' | jq .
```

Expected Response:
```json
{
  "success": false,
  "message": "Finger index must be between 0 and 9"
}
```

#### Device Not Connected
```bash
# First disconnect
curl -s -X POST http://localhost:5000/api/disconnect \
  -H "Authorization: Bearer $TOKEN" > /dev/null

# Then try to enroll
curl -X POST http://localhost:5000/api/start-enroll \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "employeeCode": "EMP001",
    "fullName": "John Doe",
    "fingerIndex": 0
  }' | jq .
```

Expected Response:
```json
{
  "success": false,
  "message": "Device is not connected. Please connect first."
}
```

## Complete Workflow Script

Run the entire enrollment workflow with one command:

```bash
./demo.sh
```

## Postman Collection

Import this JSON into Postman for easier testing:

```json
{
  "info": {
    "name": "Kaafi Device Monitor API",
    "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
  },
  "variable": [
    {
      "key": "baseUrl",
      "value": "http://localhost:5000"
    },
    {
      "key": "token",
      "value": ""
    }
  ],
  "item": [
    {
      "name": "1. Get Token",
      "event": [
        {
          "listen": "test",
          "script": {
            "exec": [
              "pm.collectionVariables.set('token', pm.response.json().token);"
            ]
          }
        }
      ],
      "request": {
        "method": "POST",
        "header": [],
        "body": {
          "mode": "raw",
          "raw": "{\n  \"username\": \"biotime\",\n  \"password\": \"biotime9.5\"\n}",
          "options": {
            "raw": {
              "language": "json"
            }
          }
        },
        "url": "{{baseUrl}}/api/auth/token"
      }
    },
    {
      "name": "2. Connect Device",
      "request": {
        "method": "POST",
        "header": [
          {
            "key": "Authorization",
            "value": "Bearer {{token}}"
          }
        ],
        "body": {
          "mode": "raw",
          "raw": "{\n  \"ipAddress\": \"192.168.1.201\",\n  \"port\": 4370\n}",
          "options": {
            "raw": {
              "language": "json"
            }
          }
        },
        "url": "{{baseUrl}}/api/connect"
      }
    },
    {
      "name": "3. Get Status",
      "request": {
        "method": "GET",
        "header": [
          {
            "key": "Authorization",
            "value": "Bearer {{token}}"
          }
        ],
        "url": "{{baseUrl}}/api/status"
      }
    },
    {
      "name": "4. Start Enrollment",
      "request": {
        "method": "POST",
        "header": [
          {
            "key": "Authorization",
            "value": "Bearer {{token}}"
          }
        ],
        "body": {
          "mode": "raw",
          "raw": "{\n  \"employeeCode\": \"EMP001\",\n  \"fullName\": \"John Doe\",\n  \"fingerIndex\": 0\n}",
          "options": {
            "raw": {
              "language": "json"
            }
          }
        },
        "url": "{{baseUrl}}/api/start-enroll"
      }
    },
    {
      "name": "5. Capture Fingerprint",
      "request": {
        "method": "POST",
        "header": [
          {
            "key": "Authorization",
            "value": "Bearer {{token}}"
          }
        ],
        "body": {
          "mode": "raw",
          "raw": "{\n  \"employeeId\": 1,\n  \"fingerIndex\": 0,\n  \"captureNumber\": 1\n}",
          "options": {
            "raw": {
              "language": "json"
            }
          }
        },
        "url": "{{baseUrl}}/api/capture-fingerprint"
      }
    },
    {
      "name": "6. Save Enrollment",
      "request": {
        "method": "POST",
        "header": [
          {
            "key": "Authorization",
            "value": "Bearer {{token}}"
          }
        ],
        "body": {
          "mode": "raw",
          "raw": "{\n  \"employeeId\": 1,\n  \"fingerIndex\": 0,\n  \"template\": \"MOCK_TEMPLATE_3_...\"\n}",
          "options": {
            "raw": {
              "language": "json"
            }
          }
        },
        "url": "{{baseUrl}}/api/save-enroll"
      }
    },
    {
      "name": "7. Disconnect",
      "request": {
        "method": "POST",
        "header": [
          {
            "key": "Authorization",
            "value": "Bearer {{token}}"
          }
        ],
        "url": "{{baseUrl}}/api/disconnect"
      }
    }
  ]
}
```

## Finger Index Reference

| Index | Finger |
|-------|--------|
| 0     | Right Thumb |
| 1     | Right Index |
| 2     | Right Middle |
| 3     | Right Ring |
| 4     | Right Little |
| 5     | Left Thumb |
| 6     | Left Index |
| 7     | Left Middle |
| 8     | Left Ring |
| 9     | Left Little |

## Notes

- In mock mode (non-Windows or without zkemkeeper.dll), the API simulates device operations
- JWT tokens expire after 8 hours (28800 seconds)
- Enrollment templates are stored in the database (InMemory by default, SQL Server when configured)
- Each employee can have multiple enrollments for different fingers on different devices
- The same finger can be re-enrolled, which will update the existing enrollment
