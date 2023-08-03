FROM mcr.microsoft.com/dotnet/sdk:8.0-preview as base
COPY . /src
WORKDIR /src
RUN dotnet restore
WORKDIR /src/maxbl4.RfidDotNet.Demo
RUN dotnet publish -o /src/_build

FROM mcr.microsoft.com/dotnet/sdk:8.0-preview as final
WORKDIR /app
COPY --from=base /src/_build . 
ENTRYPOINT ["dotnet", "maxbl4.RfidDotNet.Demo.dll"]