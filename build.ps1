$fileversion = "$env:SemVer.0"
$path = (Get-Location).Path

dotnet pack .\Template.Duende.csproj -c Release -p:PackageVersion=$env:Version -p:OutputPath=$path\artifacts -p:FileVersion=$fileversion
dotnet pack .\Template.ISç.csproj -c Release -p:PackageVersion=$env:Version -p:OutputPath=$path\artifacts -p:FileVersion=$fileversion
