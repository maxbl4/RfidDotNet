dotnet publish -o bin/_build
docker build --pull --platform armhf -t maxbl4/chafon-arm .
docker push maxbl4/chafon-arm