[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://www.gnu.org/licenses/gpl-3.0)

# Pushback Helper

Pushback Helper is an application to make push back operations easier.

- No need to contact ATC for the jetway connection
- No need to contact ATC for using push back
- No need to contact ATC for calling fuel truck, catering, luggage, or power cart service
- Open and close aircraft doors easily (A320, 787, 747 only)

![Screenshot 2024-05-01 121301](https://github.com/metindikbas/msfs-pushback-helper-app/assets/5838059/9fc6ebc3-749a-4fbb-9605-aafd3ec8f3f0)

## Download link
[Latest Release v2.6](https://github.com/metindikbas/msfs-pushback-helper-app/releases/download/v2.6/msfs-pushback-helper-app-v2.6.zip)

## Requirements
- .NET Framework 4.7.2

## Installation
Extract all files to any location and run PushbackHelper.exe. There is no need to put into the community folder location.

To work with local variables like the parking brakes of the A32NX from FlyByWire, you need to download the `FSUIPC-WASM` module from 
Pete & John Dowson http://www.fsuipc.com. 

- Download the [FSUIPC-WASMv0.5.1.zip](http://www.fsuipc.com/download/FSUIPC-WASMv0.5.1.zip) or higher and extract it
- Extract the `fsuipc-lvar-module.zip` to your community folder

## Auto-launch
If you wish to auto-launch the app upon the start of MSFS, follow these steps:
1) Navigate to the MSFS installation folder and then subfolder LocalCache
    a) Typically C:\Users\[YOUR USERNAME]\AppData\Local\Packages\Microsoft.FlightSimulator_<RANDOMLETTERS>\LocalCache
    b) This can vary depending on install drive or if you have the Steam version.
2) In the LocalCache folder, find the exe.xml file and edit with your text editor of choice.
3) Add the following snippet under the SimBase.Document tree, but substitute the path of where you installed MSFS Pushback Helper App.

```
<Launch.Addon>
    <Name>PushbackHelper</Name>
    <Disabled>False</Disabled>
    <Path>[PATH TO PUSHBACK HELPER EXE FILE]</Path>
</Launch.Addon>
```

4) This is an **example** of an updated exe.xml file:

```
<?xml version="1.0" encoding="Windows-1252"?>
<SimBase.Document Type="SimConnect" version="1,0">
  <Descr>Auto launch external applications on MSFS start</Descr>
  <Filename>exe.xml</Filename>
  <Disabled>False</Disabled>
  <Launch.Addon>
     <Name>PushbackHelper</Name>
     <Disabled>False</Disabled>
     <Path>C:\Users\alexj\AppData\Local\Packages\Microsoft.FlightSimulator_8wekyb3d8bbwe\LocalCache\Packages\community\msfs-pushback-helper-app\PushbackHelper.exe</Path>
  </Launch.Addon>
</SimBase.Document>
```

# Usage
- To show/hide UI press the **Page Up** key
- To connect or disconnect jetway simply click the **Jetway** button.
- To call the fuel truck simply click the **Fuel** button.
- To start or stop push back simply click the **Tug** button.
- The tug will wait in place until either **Forward** or **Reverse** is selected (remove the parking brake first or it will move slowly).
- Once moving, select **Left** or **Right** to steer the tug.
- There are indicators (3 dots) near the **Left** and **Right** buttons to show current rotation speed while turning.
- If you continue clicking the direction you are turning **Left** or **Right**, the rotation speed will increase.
- To decrease rotation speed, simply click the other direction button **Left** or **Right**
- When done, click **Stop**. Apply the parking brake. Click the **Tug** button again to remove the tug.

## NEW CHANGES
- [FEATURE] Added multiple rotation speeds to left and right

## v2.5 CHANGES
- [FEATURE] Adds more ground services
- [FEATURE] App no longer steals focus from MSFS so audio remains
- [FEATURE] App will auto exit upon exit of MSFS
- [FEATURE] Add service to work with local variables from MSFS
- [FEATURE] Map parking brakes of A32NX from FlyByWire
- [BUGFIX] Adds better exception handling

## v2.4 Changes
- [FEATURE] Adds tug speed setting
- [FEATURE] Adds parking brake button

## v2.3 Changes
- [FEATURE] Door buttons added to open/close doors
- [FEATURE] Changed pushback method to new method with forward tug ability
- [FEATURE] Added ability to scale app size
- [BUGFIX] Fix crash on sim disconnect

## v2.2 Changes
- [FEATURE] Fuel button added to call fuel truck.

## v2.1 Changes
- [FEATURE] Jetway button added to manage jetway connection.
- [FEATURE] Close button added to exit application.
- [FEATURE] Moving UI is possible now. Just hold left click on the gray area between jetway and close buttons to move it.
- [FEATURE] When you close the application last location is being saved and restored when you start application.
- [FEATURE] MSFS connection status text moved to top of aircraft image and it is more readable now. 
- [BUGFIX] Crash to desktop when click buttons before app connects to the sim is fixed.

## v2.0 Changes
- UI is recreated using WPF
- Shotcut key is defined. Press  Page Up to show/hide UI. If you have multiple monitor you can move UI by pressing Shift + Windows + Left or Right arrow keys together.

## License
[GNU](https://www.gnu.org/licenses/gpl-3.0.en.html)

## Creator
- https://www.linkedin.com/in/metindikbas

## Contributors
- gustavosjp - https://github.com/gustavosjp
- HydronicDev - https://github.com/HydronicDev
- alex-johnson - https://github.com/alex-johnson
- aphalon - https://github.com/aphalon
