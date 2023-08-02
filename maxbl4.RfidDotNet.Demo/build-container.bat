dotnet publish -o bin/_build
docker build --pull -t maxbl4/rfid:net8 .
docker push maxbl4/rfid:net8
