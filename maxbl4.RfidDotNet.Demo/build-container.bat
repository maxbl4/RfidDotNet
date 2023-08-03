dotnet publish -o bin/_build
docker build --pull --platform=linux/arm64 -t maxbl4/rfid .
rem docker build --pull --platform=linux/arm64 -t maxbl4/rfid:net8 -f .\Dockerfile ..
docker push maxbl4/rfid
