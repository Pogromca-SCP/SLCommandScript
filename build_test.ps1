param(
    [Parameter(Mandatory = $true)]
    [string] $pluginName,

    [Parameter(Mandatory = $true)]
    [string] $workspacePath,

    [Parameter(Mandatory = $false)]
    [string] $referencesVariable = $null,

    [Parameter(Mandatory = $false)]
    [string] $depotDownloaderVersion = "2.5.0",

    [Parameter(Mandatory = $false)]
    [bool] $runTests = $true,

    [Parameter(Mandatory = $false)]
    [Int32] $initialTestRuns = 3,

    [Parameter(Mandatory = $false)]
    [string[]] $dependencies = @()
)

if (-not [string]::IsNullOrWhiteSpace($referencesVariable)) {
    Set-Item "Env:\$referencesVariable" -Value "D:/plugin/SCPSL_REFERENCES/SCPSL_Data/Managed"
}

New-Item -ItemType Directory -Force -Path D:/plugin
New-Item -ItemType Directory -Force -Path D:/plugin/DepotDownloader
Invoke-WebRequest -Uri "https://github.com/SteamRE/DepotDownloader/releases/download/DepotDownloader_$depotDownloaderVersion/depotdownloader-$depotDownloaderVersion.zip" -OutFile "D:/plugin/depotdownloader.zip"
Expand-Archive -Path D:/plugin/depotdownloader.zip -PassThru -DestinationPath D:/plugin/DepotDownloader

New-Item -ItemType Directory -Force -Path D:/plugin/SCPSL_REFERENCES
Start-Process -NoNewWindow -Wait -FilePath "D:/plugin/DepotDownloader/DepotDownloader.exe" -WorkingDirectory "D:/plugin/DepotDownloader" -ArgumentList "-app 996560","-dir D:/plugin/SCPSL_REFERENCES"

dotnet restore
dotnet build --no-restore --configuration Release

if ($runTests -and $initialTestRuns -gt 0) {
    $workspacePath/init_tests.ps1
}

if ($runTests) {
    dotnet test --no-build --verbosity normal
}

New-Item -ItemType Directory -Force -Path D:/plugin/Artifacts
Copy-Item "$workspacePath/$pluginName/bin/Release/net48/$pluginName.dll" -Destination D:/plugin/Artifacts

if ($dependencies.Length -gt 0) {
    $assemblies = $dependencies | ForEach-Object -Process { "$workspacePath/$_/bin/Release/net48/$_.dll" }
    Compress-Archive $assemblies -DestinationPath D:/plugin/Artifacts/dependencies.zip
}
