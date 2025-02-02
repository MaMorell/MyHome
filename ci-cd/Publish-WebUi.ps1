[CmdletBinding()]
param (
    [Parameter(Mandatory = $true)]
    [string]$MyUplinkClientSecret,
    [Parameter(Mandatory = $true)]
    [string]$TibberApiAccessToken,
    [Parameter(Mandatory = $true)]
    [string]$TuyaAccessId,
    [Parameter(Mandatory = $true)]
    [string]$TuyaApiSecret,
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
        [string]$ApplicationInsightsConnectionString,
        [string]$TuyaAccessId,
        [string]$TuyaApiSecret
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
    $config.FloorHeater.AccessId = $TuyaAccessId
    $config.FloorHeater.ApiSecret = $TuyaApiSecret

    $updatedJson = $config | ConvertTo-Json -Depth 10

    $updatedJson | Set-Content -Path $configPath -Encoding UTF8

    Write-Host "Configuration updated successfully!"
}

if ((Test-Path $PublishPath) -eq $false) {
    throw "Network share '$PublishPath' not found"
}

Update-WebAppsettings 'http://192.168.10.244:5001/'
Update-ApiAppsettings $MyUplinkClientSecret $TibberApiAccessToken 'InstrumentationKey=01f9ce82-2749-434d-9a43-cdd996c12dae;IngestionEndpoint=https://swedencentral-0.in.applicationinsights.azure.com/;ApplicationId=49260d0a-163f-48a9-b79a-7a1b2e373bf0' $TuyaAccessId $TuyaApiSecret
Push-Location "C:\GIT\other\MyHome\MyHome.ApiService\"
dotnet publish --configuration Release --output "$PublishPath\MyHomeApi" --runtime $RunTime
Pop-Location

Push-Location "C:\GIT\other\MyHome\MyHome.Web\"
dotnet publish --configuration Release --output "$PublishPath\MyHomeWebUi" --runtime $RunTime
Pop-Location

Update-WebAppsettings 'https+http://apiservice'
Update-ApiAppsettings "" "" 'InstrumentationKey=00000000-0000-0000-0000-000000000000;IngestionEndpoint=https://swedencentral-0.in.applicationinsights.azure.com/;ApplicationId=00000000-0000-0000-0000-000000000000' "" ""