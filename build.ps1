$fileversion = "$env:SemVer.0"
$path = (Get-Location).Path

dotnet pack -c Release -p:PackageVersion=$env:SemVer -p:OutputPath=$path\artifacts -p:FileVersion=$fileversion
