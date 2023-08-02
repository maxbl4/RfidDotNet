dotnet publish -o bin/_build
docker build --pull -t maxbl4/chafon:arm-net8 .
docker push maxbl4/chafon:arm-net8
