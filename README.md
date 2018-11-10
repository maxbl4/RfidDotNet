# RfidDotNet
Net Standard 2.0 library to work with Rfid readers

Provides abstracted interface for reading tags. You simply get Observable\<Tag>.
  Supported protocols:
  * Alien Technology reader protocol over TCP with automatic reconnection support.
  * (Work in progress) Chianese binary protocol over serial port. Serial port code depends on [SerialPortStream](https://github.com/jcurl/SerialPortStream) package. You have to build C driver to be able to work on linux, follow instruction on project page.
