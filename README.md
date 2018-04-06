# EspBrowser
Management tool for ESP8266

This project is based on popular [ESPlorer](https://esp8266.ru/esplorer/) made in JAVA.  
It was created to simplify communication with ESP8266 board installed NodeMCU firmware.  
  
ESP Browser in written in pure .NET C# using WPF so it is runnable only on Microsoft Windows OS.  
Only prerequisite is .NET Framework 4.5 and higher.  

### ESP Browser features

**Editor for Lua scripts with basic functionalities:**
- Line numbering
- Word wrapping
- Quick find
- Find and Replace dialog
- Syntax coloring for Lua keywords
- Simple intellisense

  
**File operations on NodeMCU built-in filesystem called SPIFFS:**
- Open file from device in editor
- Download file from device to disk
- Upload file from disk to device
- Upload file opened in editor to device
- Print content of file to console
- Run script file on device by *dofile* command
- Compile script file on device to .lc file
- Rename file on device
- Delete file from device
- Format SPIFFS filesystem

### Download portable binary
- [EspBrowser.zip](http://www.mdk.sk/EspBrowser.zip)

### Release history

- 6\. April 2018 Initial release (1.0.0.0)
