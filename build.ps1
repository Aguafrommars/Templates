$fileversion = "$env:SemVer.0"
$path = (Get-Location).Path

dotnet msbuild -target:Pack -p:Configuration=Release -p:PackageVersion=$env:SemVer -p:OutputPath=$path\artifacts -p:FileVersion=$fileversion
  