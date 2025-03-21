# 1. Qurilish bosqichi (Build stage)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 2. Loyihani konteynerga nusxalash
COPY . .

# 3. NuGet paketlarni yuklash
RUN dotnet restore

# 4. Loyihani build qilish
RUN dotnet publish -c Release -o /app/publish

# 5. Ishga tushirish bosqichi (Runtime stage)
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime
WORKDIR /app

# 6. Qurilgan fayllarni nusxalash
COPY --from=build /app/publish .

# 7. yt-dlp va ffmpeg ni o‘rnatish (Alpine Linux uchun)
RUN apk update && apk add --no-cache yt-dlp ffmpeg

# 8. Botni ishga tushirish
CMD ["dotnet", "InstagramSaver.dll"]
