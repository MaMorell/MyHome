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
write-Host "sudo systemctl stop MyHome.ApiService 2>/dev/null || true"
write-Host "sudo systemctl stop MyHome.Web 2>/dev/null || true"

write-Host "screen"
write-Host "dotnet MyHome.ApiService.dll --urls=http://0.0.0.0:5001/"
write-Host "dotnet MyHome.Web.dll --urls=http://0.0.0.0:5002/"

write-Host "ctrl A + ctrl D"
