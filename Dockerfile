# Menggunakan image base .NET SDK
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Menyalin file dan folder yang diperlukan
COPY "EduTechDAD.sln" "EduTechDAD.sln"
COPY "API/API.csproj" "API/API.csproj"
COPY "Application/Application.csproj" "Application/Application.csproj"
COPY "Persistence/Persistence.csproj" "Persistence/Persistence.csproj"
COPY "Domain/Domain.csproj" "Domain/Domain.csproj"
COPY "Infrastructure/Infrastructure.csproj" "Infrastructure/Infrastructure.csproj"

# Restore dependencies
RUN dotnet restore "EduTechDAD.sln"

# Salin file lainnya dan build
COPY . .
WORKDIR /app
RUN dotnet publish -c Release -o out

# Menggunakan image base .NET runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Menyalin build output dari tahap sebelumnya
COPY --from=build-env /app/out .

# Menetapkan variabel lingkungan
ENV ConnectionStrings__KoneksiKePostgreSQL="Host=postgres_container;Database=DbLMSEduTechIBE;Username=root;Password=root;"
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80

# Expose port 80
EXPOSE 80

# Menjalankan aplikasi
ENTRYPOINT ["dotnet", "API.dll"]