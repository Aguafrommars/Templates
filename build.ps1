$fileversion = "$env:SemVer.0"
$path = (Get-Location).Path

dotnet pack -c Release -p:PackageVersion=$env:Version -p:OutputPath=$path\artifacts -p:FileVersion=$fileversion
