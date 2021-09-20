$fileversion = "$env:SemVer.0"
$path = (Get-Location).Path

dotnet pack .\TheIdServer.Duende\Template.csproj -c Release -p:PackageVersion=$env:Version -p:OutputPath=$path\artifacts -p:FileVersion=$fileversion
dotnet pack .\TheIdServer.IS4\Template.csproj -c Release -p:PackageVersion=$env:Version -p:OutputPath=$path\artifacts -p:FileVersion=$fileversion
