# Templates
dotnet new templates

Create a [TheIdServer](https://github.com/Aguafrommars/TheIdServer) solution with [`dotnet new`](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-new) cli tool.

## Install

```bash
dotnet new -i TheIdServer.Template
```

## Use

```bash
> dotnet new tis -o TheIdServer
The template "TheIdServer" was created successfully.

Processing post-creation actions...
Running 'dotnet restore' on TheIdServer\TheIdServer.sln...
  Determining projects to restore...
  Restored C:\Projects\Perso\Templates\artifacts\TheIdServer\test\WebAssembly.Net.Http\WebAssembly.Net.Http.csproj (in 114 ms).
  Restored C:\Projects\Perso\Templates\artifacts\TheIdServer\src\TheIdServer.BlazorApp\TheIdServer.BlazorApp.csproj (in 916 ms).
  Restored C:\Projects\Perso\Templates\artifacts\TheIdServer\test\Microsoft.AspNetCore.Components.Testing\Microsoft.AspNetCore.Components.Testing.csproj (in 1.08 sec).
  Restored C:\Projects\Perso\Templates\artifacts\TheIdServer\src\TheIdServer\TheIdServer.csproj (in 2.03 sec).
  Restored C:\Projects\Perso\Templates\artifacts\TheIdServer\test\TheIdServer.Test\TheIdServer.Test.csproj (in 2.04 sec).
  Restored C:\Projects\Perso\Templates\artifacts\TheIdServer\test\TheIdServer.IntegrationTest\TheIdServer.IntegrationTest.csproj (in 2.04 sec).
Restore succeeded.
```

The above commande create a Visual Studio solution in *TheIdServer* subfolder. 

```cs
TheIdServer
├─── src
|    ├─── TheIdServer // Server project
|    └─── TheIdServer.BlazorApp // Blazor application project
└─── test
     ├─── Microsoft.AspNetCore.Components.Testing // Components testing utilities
     ├─── TheIdServer.IntegrationTest // Server integration tests
     ├─── TheIdServer.Test // Server tests
     └─── WebAssembly.Net.Http // WASM test utilities
```