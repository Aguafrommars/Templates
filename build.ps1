$fileversion = "$env:SemVer.0"
$path = (Get-Location).Path

dotnet pack .\TheIdServer.Duende\Template.csproj -c Release -p:PackageVersion=$env:Version -p:OutputPath=$path\artifacts -p:FileVersion=$fileversion
