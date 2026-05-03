FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY SmartDocumentProcessing/SmartDocumentProcessing.csproj SmartDocumentProcessing/
RUN dotnet restore SmartDocumentProcessing/SmartDocumentProcessing.csproj

COPY . .
RUN dotnet publish SmartDocumentProcessing/SmartDocumentProcessing.csproj -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

ENV ASPNETCORE_URLS=http://0.0.0.0:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "SmartDocumentProcessing.dll"]