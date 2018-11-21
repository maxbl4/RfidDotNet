dotnet publish -o bin/_build
docker build --pull --platform armhf -t maxbl4/chafon:arm .
docker build --pull --platform amd64 -t maxbl4/chafon:amd64 .
docker push maxbl4/chafon:arm
docker push maxbl4/chafon:amd64