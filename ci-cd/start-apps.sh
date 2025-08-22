#!/bin/bash

# Configuration
API_PATH="/home/shared/MyHomeApi"
WEB_PATH="/home/shared/MyHomeWebUi"

# Stop existing sessions if they exist
screen -S myhome-api -X quit 2>/dev/null || true
screen -S myhome-web -X quit 2>/dev/null || true

# Wait a moment for clean shutdown
sleep 1

# Start API service
cd "$API_PATH"
screen -dmS myhome-api ./MyHome.ApiService --urls "http://0.0.0.0:5001"

# Start Web UI
cd "$WEB_PATH"
screen -dmS myhome-web ./MyHome.Web --urls "http://0.0.0.0:5002"

echo "Applications started!"
echo "Attach with: screen -r myhome-api or screen -r myhome-web"