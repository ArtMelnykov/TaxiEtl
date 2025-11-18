#--------------------------Restoring dependencies and building---------------------------------------------------------------

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

#--------------------------Optimizing Docker Caching-------------------------------------------------------------------------

COPY ["TaxiDataETL.sln", "."]
COPY ["src/TaxiDataETL.CLI/TaxiDataETL.CLI.csproj", "src/TaxiDataETL.CLI/"]
COPY ["src/TaxiDataETL.Core/TaxiDataETL.Core.csproj", "src/TaxiDataETL.Core/"]
COPY ["src/TaxiDataETL.Data/TaxiDataETL.Data.csproj", "src/TaxiDataETL.Data/"]
COPY ["src/TaxiDataETL.Tests/TaxiDataETL.Tests.csproj", "src/TaxiDataETL.Tests/"]

#--------------------------Restoring NuGet packages--------------------------------------------------------------------------

RUN dotnet restore "TaxiDataETL.sln"

#--------------------------Copy all remaining source code--------------------------------------------------------------------

COPY . .

#--------------------------Running tests-------------------------------------------------------------------------------------

# Check if everything is okay with tests
FROM build AS test
WORKDIR "/src"
# Run tests against the whole solution from its root.
RUN dotnet test "TaxiDataETL.sln"

#--------------------------Application publish-------------------------------------------------------------------------------

FROM test AS publish
WORKDIR "/src"
RUN dotnet publish "src/TaxiDataETL.CLI/TaxiDataETL.CLI.csproj" -c Release -o /app/publish /p:UseAppHost=false

#--------------------------Final image---------------------------------------------------------------------------------------

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
# Copy only compiled artifacts from the /app/publish folder from the ‘publish’ stage.
COPY --from=publish /app/publish .
# Explain how to launch our application
ENTRYPOINT ["dotnet", "TaxiDataETL.CLI.dll"]
