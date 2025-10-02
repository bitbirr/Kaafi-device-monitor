#!/bin/bash
# Kaafi Device Monitor - Complete Enrollment Workflow Demo
# This script demonstrates the complete enrollment API workflow

API_URL="http://localhost:5000"
USERNAME="biotime"
PASSWORD="biotime9.5"
DEVICE_IP="192.168.1.201"
DEVICE_PORT=4370

echo "================================="
echo "Kaafi Device Monitor - Demo"
echo "================================="
echo ""

# Step 1: Get JWT Token
echo "Step 1: Getting JWT Token..."
TOKEN_RESPONSE=$(curl -s -X POST "$API_URL/api/auth/token" \
  -H "Content-Type: application/json" \
  -d "{\"username\":\"$USERNAME\",\"password\":\"$PASSWORD\"}")

TOKEN=$(echo $TOKEN_RESPONSE | jq -r '.token')

if [ "$TOKEN" == "null" ] || [ -z "$TOKEN" ]; then
    echo "Failed to get token!"
    echo $TOKEN_RESPONSE | jq .
    exit 1
fi

echo "✓ Token obtained successfully"
echo ""

# Step 2: Connect to Device
echo "Step 2: Connecting to device at $DEVICE_IP:$DEVICE_PORT..."
CONNECT_RESPONSE=$(curl -s -X POST "$API_URL/api/connect" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d "{\"ipAddress\":\"$DEVICE_IP\",\"port\":$DEVICE_PORT}")

echo $CONNECT_RESPONSE | jq .
SUCCESS=$(echo $CONNECT_RESPONSE | jq -r '.success')

if [ "$SUCCESS" != "true" ]; then
    echo "Failed to connect to device!"
    exit 1
fi

DEVICE_ID=$(echo $CONNECT_RESPONSE | jq -r '.deviceId')
echo "✓ Connected to device: $DEVICE_ID"
echo ""

# Step 3: Start Enrollment
echo "Step 3: Starting enrollment for employee..."
EMPLOYEE_CODE="EMP$(date +%s)"
FULL_NAME="Test Employee $(date +%H:%M:%S)"
FINGER_INDEX=0

START_ENROLL_RESPONSE=$(curl -s -X POST "$API_URL/api/start-enroll" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d "{\"employeeCode\":\"$EMPLOYEE_CODE\",\"fullName\":\"$FULL_NAME\",\"fingerIndex\":$FINGER_INDEX}")

echo $START_ENROLL_RESPONSE | jq .
EMPLOYEE_ID=$(echo $START_ENROLL_RESPONSE | jq -r '.employeeId')

if [ "$EMPLOYEE_ID" == "null" ] || [ -z "$EMPLOYEE_ID" ]; then
    echo "Failed to start enrollment!"
    exit 1
fi

echo "✓ Enrollment started for Employee ID: $EMPLOYEE_ID"
echo ""

# Step 4: Capture Fingerprints (3 times)
echo "Step 4: Capturing fingerprints (3 times)..."

for CAPTURE_NUM in 1 2 3; do
    echo "  Capturing fingerprint $CAPTURE_NUM of 3..."
    
    CAPTURE_RESPONSE=$(curl -s -X POST "$API_URL/api/capture-fingerprint" \
      -H "Authorization: Bearer $TOKEN" \
      -H "Content-Type: application/json" \
      -d "{\"employeeId\":$EMPLOYEE_ID,\"fingerIndex\":$FINGER_INDEX,\"captureNumber\":$CAPTURE_NUM}")
    
    echo "  " $(echo $CAPTURE_RESPONSE | jq -c '{success, message, captureCount, isComplete}')
    
    if [ $CAPTURE_NUM -eq 3 ]; then
        TEMPLATE=$(echo $CAPTURE_RESPONSE | jq -r '.template')
    fi
    
    sleep 1
done

if [ "$TEMPLATE" == "null" ] || [ -z "$TEMPLATE" ]; then
    echo "Failed to capture fingerprints!"
    exit 1
fi

echo "✓ All fingerprints captured successfully"
echo ""

# Step 5: Save Enrollment
echo "Step 5: Saving enrollment to database..."
SAVE_RESPONSE=$(curl -s -X POST "$API_URL/api/save-enroll" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d "{\"employeeId\":$EMPLOYEE_ID,\"fingerIndex\":$FINGER_INDEX,\"template\":\"$TEMPLATE\"}")

echo $SAVE_RESPONSE | jq .
ENROLLMENT_ID=$(echo $SAVE_RESPONSE | jq -r '.enrollmentId')

if [ "$ENROLLMENT_ID" == "null" ] || [ -z "$ENROLLMENT_ID" ]; then
    echo "Failed to save enrollment!"
    exit 1
fi

echo "✓ Enrollment saved with ID: $ENROLLMENT_ID"
echo ""

# Step 6: Check Status
echo "Step 6: Checking device status..."
STATUS_RESPONSE=$(curl -s -X GET "$API_URL/api/status" \
  -H "Authorization: Bearer $TOKEN")

echo $STATUS_RESPONSE | jq .
echo ""

echo "================================="
echo "Demo completed successfully!"
echo "================================="
echo "Summary:"
echo "  Employee Code: $EMPLOYEE_CODE"
echo "  Employee Name: $FULL_NAME"
echo "  Employee ID: $EMPLOYEE_ID"
echo "  Enrollment ID: $ENROLLMENT_ID"
echo "  Device ID: $DEVICE_ID"
echo "  Finger Index: $FINGER_INDEX"
echo "================================="
