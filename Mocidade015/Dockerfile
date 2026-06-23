# Estágio de Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Mocidade015.csproj", "./"]
RUN dotnet restore "Mocidade015.csproj"
COPY . .
RUN dotnet publish "Mocidade015.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Estágio de Execução
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .

# Render usa uma porta variável; o .NET precisa ouvir nela
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Cultura padrão pt-BR para formatação de datas
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

ENTRYPOINT ["dotnet", "Mocidade015.dll"]
