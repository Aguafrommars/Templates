#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["src/Template/Template.csproj", "src/Template/"]
COPY ["src/Template.BlazorApp/Template.BlazorApp.csproj", "src/Template.BlazorApp/"]
RUN dotnet restore "src/Template/Template.csproj"
COPY . .
WORKDIR "/src/src/Template"
RUN dotnet build "Template.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Template.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Template.dll"]