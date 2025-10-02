# Kaafi Device Monitor

A .NET Web API for managing ZKTeco biometric device enrollments using the ZKTime 5.0 SDK (zkemkeeper.dll). This API provides endpoints for device connection, employee enrollment, and fingerprint capture with JWT authentication.

## Features

- **Device Connection**: Connect to ZKTeco devices via TCP/IP
- **Employee Management**: Create and manage employee records
- **Fingerprint Enrollment**: Multi-step fingerprint capture (3 captures per finger)
- **Database Storage**: Store enrollment templates in SQL Server
- **JWT Authentication**: Secure endpoints with BioTime 9.5 compatible JWT tokens
- **Mock Mode**: Development mode without actual hardware

## Requirements

- .NET 9.0 or later
- SQL Server (optional - defaults to in-memory database)
- ZKTeco device with zkemkeeper.dll SDK (Windows only for hardware mode)

## Installation

1. Clone the repository:
```bash
git clone https://github.com/bitbirr/Kaafi-device-monitor.git
cd Kaafi-device-monitor
```

2. Restore dependencies:
```bash
dotnet restore
```

3. Configure the application:
   - Edit `appsettings.json` to configure SQL Server connection string (optional)
   - Update JWT settings if needed

4. Run the application:
```bash
dotnet run
```

The API will be available at `https://localhost:5001` or `http://localhost:5000`.

## Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=KaafiDeviceMonitor;Trusted_Connection=true;TrustServerCertificate=true;"
  },
  "Jwt": {
    "SecretKey": "YourVerySecretKeyForJWTTokenGeneration123456789",
    "Issuer": "KaafiDeviceMonitor",
    "Audience": "KaafiDeviceMonitorAPI",
    "ExpirationHours": 8
  }
}
```

If `ConnectionStrings.DefaultConnection` is empty, the application uses an in-memory database.

## API Documentation

### Authentication

All endpoints except `/api/auth/token` require JWT authentication. Include the token in the Authorization header:

```
Authorization: Bearer <your-token>
```

### Endpoints

#### 1. Get JWT Token

**POST** `/api/auth/token`

Get an authentication token for API access.

**Request:**
```json
{
  "username": "biotime",
  "password": "biotime9.5"
}
```

**Response:**
```json
{
  "success": true,
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresIn": 28800
}
```

#### 2. Connect to Device

**POST** `/api/connect`

Connect to a ZKTeco device.

**Request:**
```json
{
  "ipAddress": "192.168.1.201",
  "port": 4370
}
```

**Response:**
```json
{
  "success": true,
  "message": "Connected successfully",
  "deviceId": "BFQC123456789"
}
```

#### 3. Check Device Status

**GET** `/api/status`

Check if the device is connected.

**Response:**
```json
{
  "isConnected": true,
  "deviceId": "BFQC123456789"
}
```

#### 4. Start Enrollment

**POST** `/api/start-enroll`

Start the enrollment process for an employee.

**Request:**
```json
{
  "employeeCode": "EMP001",
  "fullName": "John Doe",
  "fingerIndex": 0
}
```

- `fingerIndex`: 0-9 (0 = right thumb, 1 = right index, etc.)

**Response:**
```json
{
  "success": true,
  "message": "Enrollment started successfully. Please place finger 3 times.",
  "employeeId": 1
}
```

#### 5. Capture Fingerprint

**POST** `/api/capture-fingerprint`

Capture a fingerprint (call 3 times with different capture numbers).

**Request:**
```json
{
  "employeeId": 1,
  "fingerIndex": 0,
  "captureNumber": 1
}
```

- `captureNumber`: 1, 2, or 3

**Response:**
```json
{
  "success": true,
  "message": "Fingerprint 1 captured successfully",
  "captureCount": 1,
  "isComplete": false,
  "template": null
}
```

After the 3rd capture:
```json
{
  "success": true,
  "message": "Fingerprint 3 captured successfully",
  "captureCount": 3,
  "isComplete": true,
  "template": "MOCK_TEMPLATE_3_abcd1234..."
}
```

#### 6. Save Enrollment

**POST** `/api/save-enroll`

Save the captured fingerprint template to the database.

**Request:**
```json
{
  "employeeId": 1,
  "fingerIndex": 0,
  "template": "MOCK_TEMPLATE_3_abcd1234..."
}
```

**Response:**
```json
{
  "success": true,
  "message": "Enrollment saved successfully",
  "enrollmentId": 1
}
```

#### 7. Disconnect Device

**POST** `/api/disconnect`

Disconnect from the device.

**Response:**
```json
{
  "success": true,
  "message": "Disconnected successfully"
}
```

## Enrollment Workflow

1. **Get Token**: Authenticate to get a JWT token
2. **Connect**: Connect to the ZKTeco device
3. **Start Enrollment**: Initialize enrollment for an employee
4. **Capture Fingerprints**: Place finger 3 times (capture 1, 2, 3)
5. **Save Enrollment**: Store the final template in the database
6. **Disconnect**: Disconnect from the device (optional)

### Example Workflow with cURL

```bash
# 1. Get JWT token
TOKEN=$(curl -X POST http://localhost:5000/api/auth/token \
  -H "Content-Type: application/json" \
  -d '{"username":"biotime","password":"biotime9.5"}' \
  | jq -r '.token')

# 2. Connect to device
curl -X POST http://localhost:5000/api/connect \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"ipAddress":"192.168.1.201","port":4370}'

# 3. Start enrollment
curl -X POST http://localhost:5000/api/start-enroll \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"employeeCode":"EMP001","fullName":"John Doe","fingerIndex":0}'

# 4. Capture fingerprints (repeat 3 times)
curl -X POST http://localhost:5000/api/capture-fingerprint \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"employeeId":1,"fingerIndex":0,"captureNumber":1}'

curl -X POST http://localhost:5000/api/capture-fingerprint \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"employeeId":1,"fingerIndex":0,"captureNumber":2}'

RESPONSE=$(curl -X POST http://localhost:5000/api/capture-fingerprint \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"employeeId":1,"fingerIndex":0,"captureNumber":3}')

TEMPLATE=$(echo $RESPONSE | jq -r '.template')

# 5. Save enrollment
curl -X POST http://localhost:5000/api/save-enroll \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d "{\"employeeId\":1,\"fingerIndex\":0,\"template\":\"$TEMPLATE\"}"
```

## Database Schema

### Employee Table
- `Id` (int, PK)
- `Code` (string, unique)
- `FullName` (string)

### Enrollment Table
- `Id` (int, PK)
- `EmployeeId` (int, FK)
- `DeviceId` (string)
- `EnrollId` (int)
- `FingerIndex` (int)
- `Template` (string)
- `CreatedAt` (datetime)

## Development

### Mock Mode

When running on non-Windows platforms or without the zkemkeeper.dll installed, the application automatically runs in mock mode. This allows development and testing without actual hardware.

Mock mode:
- Simulates device connections
- Generates fake fingerprint templates
- Allows full API testing

### Building for Production

```bash
dotnet publish -c Release -o ./publish
```

### Running Tests

```bash
dotnet test
```

## ZKTeco SDK

For production use on Windows with actual ZKTeco devices:

1. Install ZKTime 5.0 SDK
2. Ensure `zkemkeeper.dll` is registered in Windows
3. The application will automatically detect and use the SDK

## Security

- All enrollment endpoints are protected with JWT authentication
- JWT tokens expire after 8 hours (configurable)
- Use strong secret keys in production
- Use HTTPS in production environments

## License

MIT License

## Support

For issues and questions, please open an issue on GitHub.
