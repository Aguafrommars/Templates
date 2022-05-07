#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:latest AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:latest AS build
WORKDIR /src
COPY ["src/TIS/TIS.csproj", "src/TIS/"]
COPY ["src/TIS.BlazorApp/TIS.BlazorApp.csproj", "src/TIS.BlazorApp/"]
RUN dotnet restore "src/TIS/TIS.csproj"
COPY . .
WORKDIR "/src/src/TIS"
RUN dotnet build "TIS.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TIS.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TIS.dll"]