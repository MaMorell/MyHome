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

function Update-WebAppsettings {
    param(
        [string]$ApiServiceBaseUrl
    )
    
    $configPath = "C:\GIT\other\MyHome\MyHome.Web\appsettings.json"

    if (-not (Test-Path $configPath)) {
        Write-Error "Configuration file not found at: $configPath"
        exit 1
    }

    $config = Get-Content -Path $configPath -Raw | ConvertFrom-Json

    $config.ApiService.BaseUrl = $ApiServiceBaseUrl

    $updatedJson = $config | ConvertTo-Json -Depth 10

    $updatedJson | Set-Content -Path $configPath -Encoding UTF8

    Write-Host "Configuration updated successfully!"
}

function Update-ApiAppsettings {
    param(
        [string]$MyUplinkClientSecret,
        [string]$TibberApiAccessToken,
        [string]$ApplicationInsightsConnectionString
    )
    
    $configPath = "C:\GIT\other\MyHome\MyHome.ApiService\appsettings.json"

    if (-not (Test-Path $configPath)) {
        Write-Error "Configuration file not found at: $configPath"
        exit 1
    }

    $config = Get-Content -Path $configPath -Raw | ConvertFrom-Json

    $config.UpLinkOptions.ClientSecret = $MyUplinkClientSecret
    $config.TibberApiClient.AccessToken = $TibberApiAccessToken
    $config.APPLICATIONINSIGHTS_CONNECTION_STRING = $ApplicationInsightsConnectionString

    $updatedJson = $config | ConvertTo-Json -Depth 10

    $updatedJson | Set-Content -Path $configPath -Encoding UTF8

    Write-Host "Configuration updated successfully!"
}

if ((Test-Path $PublishPath) -eq $false) {
    throw "Network share '$PublishPath' not found"
}

Update-WebAppsettings 'http://0.0.0.0:5001/'
Update-ApiAppsettings $MyUplinkClientSecret $TibberApiAccessToken 'InstrumentationKey=01f9ce82-2749-434d-9a43-cdd996c12dae;IngestionEndpoint=https://swedencentral-0.in.applicationinsights.azure.com/;ApplicationId=49260d0a-163f-48a9-b79a-7a1b2e373bf0'
Push-Location "C:\GIT\other\MyHome\MyHome.ApiService\"
dotnet publish --configuration Release --output "$PublishPath\MyHomeApi" --runtime $RunTime
Pop-Location

Push-Location "C:\GIT\other\MyHome\MyHome.Web\"
dotnet publish --configuration Release --output "$PublishPath\MyHomeWebUi" --runtime $RunTime
Pop-Location

Update-WebAppsettings 'https+http://apiservice'
Update-ApiAppsettings "" "" 'InstrumentationKey=00000000-0000-0000-0000-000000000000;IngestionEndpoint=https://swedencentral-0.in.applicationinsights.azure.com/;ApplicationId=00000000-0000-0000-0000-000000000000'

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