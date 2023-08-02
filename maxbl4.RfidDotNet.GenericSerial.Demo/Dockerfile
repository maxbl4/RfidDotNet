FROM mcr.microsoft.com/dotnet/sdk:8.0-preview-bookworm-slim-arm64v8
COPY bin/_build /app
WORKDIR /app
ENTRYPOINT ["dotnet", "maxbl4.RfidDotNet.GenericSerial.Demo.dll"]