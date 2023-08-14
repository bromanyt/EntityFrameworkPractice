FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS publish
WORKDIR /src
COPY ["WebRetail.csproj", "./"]
RUN dotnet restore "WebRetail.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet publish "WebRetail.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebRetail.dll"]
