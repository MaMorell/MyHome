# --- Configuration ---
$PI_IP = "10.10.10.113"
$PI_USER = "mormat"
$REMOTE_DIR = "/home/$PI_USER/docker/myhomeapp"
$COMPOSE_SOURCE = "./MyHome.AppHost/aspire-output/docker-compose.yaml" 

$STAGE_DIR = "$env:TEMP\MyHomeDeploy"
$API_TAR = "$STAGE_DIR\myhome-api.tar.gz"
$WEB_TAR = "$STAGE_DIR\myhome-web.tar.gz"
$ENV_FILE = "$STAGE_DIR\.env"

if (Test-Path $STAGE_DIR) { Remove-Item -Recurse -Force $STAGE_DIR }
New-Item -ItemType Directory -Path $STAGE_DIR | Out-Null

Write-Host "🚀 1. Building ARM64 Archives..." -ForegroundColor Cyan
dotnet publish "./MyHome.ApiService/MyHome.ApiService.csproj" --os linux --arch arm64 /t:PublishContainer -p:ContainerArchiveOutputPath="$API_TAR" -p:ContainerRepository="myhome-api"
dotnet publish "./MyHome.Web/MyHome.Web.csproj" --os linux --arch arm64 /t:PublishContainer -p:ContainerArchiveOutputPath="$WEB_TAR" -p:ContainerRepository="myhome-web"

Write-Host "📝 2. Creating .env file (Linux Line Endings)..." -ForegroundColor Cyan
$EnvContent = @(
    "MYHOME_API_IMAGE=myhome-api:latest",
    "MYHOME_WEB_IMAGE=myhome-web:latest",
    "MYHOME_API_PORT=5000",
    "MYHOME_WEB_PORT=5001",
    "UPLINKOPTIONSCLIENTSECRET=foo",
    "TIBBERAPICLIENTACCESSTOKEN=foo",
    "THERMOSTATEBECOPASSWORD=foo"
) -join "`n" # Uses Join with \n to ensure Linux compatibility

[System.IO.File]::WriteAllText($ENV_FILE, $EnvContent)
Copy-Item $COMPOSE_SOURCE -Destination "$STAGE_DIR\docker-compose.yml"

# Write-Host "📂 3. Preparing Pi directory..." -ForegroundColor Cyan
# ssh "${PI_USER}@${PI_IP}" "sudo mkdir -p $REMOTE_DIR && sudo chown ${PI_USER}:${PI_USER} $REMOTE_DIR"

Write-Host "📤 4. Transferring files..." -ForegroundColor Cyan
scp "$API_TAR" "$WEB_TAR" "$ENV_FILE" "$STAGE_DIR\docker-compose.yml" "${PI_USER}@${PI_IP}:$REMOTE_DIR/"

Write-Host "📦 5. Loading images and starting up..." -ForegroundColor Cyan
$SSH_CMD = "cd $REMOTE_DIR && docker load -i myhome-api.tar.gz && docker load -i myhome-web.tar.gz && rm *.tar.gz && docker compose up -d && docker image prune -f"
ssh "${PI_USER}@${PI_IP}" $SSH_CMD

Write-Host "🎉 Solution Deployed! Web: http://$PI_IP:5001" -ForegroundColor Green

Remove-Item -Recurse -Force $STAGE_DIR