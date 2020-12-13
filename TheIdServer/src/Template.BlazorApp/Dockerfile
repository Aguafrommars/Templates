#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["src/Template.BlazorApp/Template.BlazorApp.csproj", "src/Template.BlazorApp/"]
RUN dotnet restore "src/Template.BlazorApp/Template.BlazorApp.csproj"
COPY . .
WORKDIR "/src/src/Template.BlazorApp"
RUN dotnet build "Template.BlazorApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Template.BlazorApp.csproj" -c Release -o /app/publish

FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html
COPY --from=publish /app/publish/wwwroot .
COPY --from=build /src/src/Template.BlazorApp/nginx.conf /etc/nginx/nginx.conf