#FROM microsoft/dotnet:2.1-sdk as builder
#COPY . /build
#WORKDIR /build/chafon
#RUN dotnet publish -o /app
FROM microsoft/dotnet:2.1-sdk
COPY chafon/lib/* /usr/lib/
COPY chafon/app /app
WORKDIR /app
ENTRYPOINT ["dotnet", "chafon.dll"]