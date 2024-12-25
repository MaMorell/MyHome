[CmdletBinding()]
param (
    [Parameter(Mandatory = $true)]
    [string]$MyUplinkClientSecret,
    [Parameter(Mandatory = $true)]
    [string]$TibberApiAccessToken,
    [Parameter(Mandatory = $false)]
    [string]$PublishPath = "\\192.168.10.244\pimylifeupshare",
    [Parameter(Mandatory = $false)]
    [string]$RunTime = "linux-arm64"
)

function Update-Appsettings {
    param(
        [string]$MyUplinkClientSecret,
        [string]$TibberApiAccessToken
    )
    
    $configPath = "C:\GIT\other\MyHome\MyHome.ApiService\appsettings.json"

    if (-not (Test-Path $configPath)) {
        Write-Error "Configuration file not found at: $configPath"
        exit 1
    }

    $config = Get-Content -Path $configPath -Raw | ConvertFrom-Json

    $config.UpLinkCredentials.ClientSecret = $MyUplinkClientSecret
    $config.TibberApiClient.AccessToken = $TibberApiAccessToken

    $updatedJson = $config | ConvertTo-Json -Depth 10

    $updatedJson | Set-Content -Path $configPath -Encoding UTF8

    Write-Host "Configuration updated successfully!"
}

if ((Test-Path $PublishPath) -eq $false) {
    throw "Network share '$PublishPath' not found"
}

Update-Appsettings $MyUplinkClientSecret $TibberApiAccessToken
Push-Location "C:\GIT\other\MyHome\MyHome.ApiService\"
dotnet publish --configuration Release --output "$PublishPath\MyHomeApi" --runtime $RunTime
Pop-Location

Push-Location "C:\GIT\other\MyHome\MyHome.Web\"
dotnet publish --configuration Release --output "$PublishPath\MyHomeWebUi" --runtime $RunTime
Pop-Location

Update-Appsettings "" ""


write-Host "How to run on RP:"

write-Host "# Stop existing processes"
write-Host "sudo pkill -f MyHome.ApiService.dll && sudo pkill -f MyHome.Web.dll"

write-Host "# Verify processes are stopped"
write-Host "ps aux | grep MyHome"

write-Host "# Start API Service (detached screen)"
write-Host "screen -dmS api bash -c 'cd shared/MyHomeApi && dotnet MyHome.ApiService.dll --urls=http://0.0.0.0:5001/'"

write-Host "# Start Web UI (detached screen)"
write-Host "screen -dmS web bash -c 'cd shared/MyHomeWebUi && dotnet MyHome.Web.dll --urls=http://0.0.0.0:5002/'"

write-Host "# To view running screens: screen -ls"
write-Host "# To attach to a screen: screen -r api or screen -r web"
write-Host "# To kill all screens: killall screen"
write-Host "# To detach from a screen: ctrl+A, ctrl+D"