#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/sdk:latest AS build
WORKDIR /src
COPY ["src/TIS.BlazorApp/TIS.BlazorApp.csproj", "src/TIS.BlazorApp/"]
RUN dotnet restore "src/TIS.BlazorApp/TIS.BlazorApp.csproj"
COPY . .
WORKDIR "/src/src/TIS.BlazorApp"
RUN dotnet build "TIS.BlazorApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TIS.BlazorApp.csproj" -c Release -o /app/publish

FROM nginx:1.26.1 AS final
WORKDIR /usr/share/nginx/html
COPY --from=publish /app/publish/wwwroot .
COPY --from=build /src/src/TIS.BlazorApp/nginx.conf /etc/nginx/nginx.conf