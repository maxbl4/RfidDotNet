dotnet publish -o bin/_build
docker build --pull --platform=linux/arm64 -t maxbl4/chafon:arm .
docker push maxbl4/chafon:arm
