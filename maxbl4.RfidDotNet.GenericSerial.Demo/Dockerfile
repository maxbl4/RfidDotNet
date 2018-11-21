FROM microsoft/dotnet:2.1-sdk
#RUN apt update && apt install socat build-essential cmake -y
#WORKDIR /root
#RUN git clone https://github.com/jcurl/SerialPortStream.git
#WORKDIR /root/SerialPortStream/serialinux
#RUN bash build.sh
#RUN cp bin/usr/local/lib/libnserial* /usr/lib/
COPY lib/armhf/* /usr/lib/
COPY bin/_build /app
WORKDIR /app
ENTRYPOINT ["dotnet", "maxbl4.RfidDotNet.GenericSerial.Demo.dll"]