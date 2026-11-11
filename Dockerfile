# 1. Stage Build (компиляция .NET 10)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Копируем файлы проектов и восстанавливаем зависимости
COPY ["EzShop.sln", "./"]
COPY ["EzShop.Domain/EzShop.Domain.csproj", "EzShop.Domain/"]
COPY ["EzShop.DAL/EzShop.DAL.csproj", "EzShop.DAL/"]
COPY ["EzShop.BLL/EzShop.BLL.csproj", "EzShop.BLL/"]
COPY ["EzShop.Web/EzShop.Web.csproj", "EzShop.Web/"]
RUN dotnet restore "EzShop.sln"

# Копируем весь остальной код и публикуем в Release
COPY . .
WORKDIR "/src/EzShop.Web"
RUN dotnet publish "EzShop.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 2. Stage Runtime (легковесный образ для запуска)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Порт по умолчанию (переопределяется переменной окружения PORT от Render/Railway)
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "EzShop.Web.dll"]
