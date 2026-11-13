PUSHBACK HELPER v2.7
Release date: 2026/11/13

### REQUIREMENTS ###
- Microsoft Flight Simulator 2024. It should also work with MSFS 2020; if you run into problems there, keep using v2.6.
- .NET Framework 4.7.2
- Microsoft Visual C++ 2015-2022 Redistributable (x64), normally already installed together with MSFS

### INSTALLATION ###
Extract all files to any location and run PushbackHelper.exe. There is no need to put into the community folder location.

No extra modules are needed. The parking brake of the Fenix A319/A320/A321, FlyByWire A32NX, Headwind A330 and
FSLabs A32x is read directly from the aircraft through SimConnect.

### USAGE ###
- To show/hide UI press the Page Up key
- To connect or disconnect jetway simply click the JETWAY button.
- To call the fuel truck simply click the FUEL button.
- To start or stop push back simply click the TUG button.
- The tug will wait in place until either FORWARD or REVERSE is selected (remove the parking brake first or it will move slowly).
- Once moving, select LEFT or RIGHT to steer the tug.
- When done, click STOP. Apply the parking brake. Click the TUG button again to remove the tug.
- There are indicators near LEFT and RIGHT buttons to show the current turn level (1 to 3). Click the button on the side you are already turning to turn tighter and click the other side to straighten out.

### v2.7 CHANGES ###
- [FEATURE] Built with the MSFS 2024 SDK SimConnect client. It should also work with MSFS 2020; if you run into problems there, keep using v2.6.
- [FEATURE] Reworked tug steering: turn levels 1, 2 and 3 set the tug angle (20, 40, 60 degrees). Higher levels turn tighter and slow the aircraft down instead of speeding it up.
- [BUGFIX] The tug no longer detaches at the maximum turn level.
- [BUGFIX] Smoother turns: the tug follows the aircraft heading every frame instead of once per second.
- [FEATURE] Parking brake read from the aircraft's own systems on Fenix A319/A320/A321 (also set), FlyByWire A32NX, Headwind A330 and FSLabs A32x.
- [BUGFIX] More reliable connection while the sim is loading.
- [BUGFIX] A lost connection or sim crash is detected and the app closes instead of staying connected.
- [BUGFIX] No more error pop-ups during pushback if the connection drops.

### v2.6 CHANGES ###
- [FEATURE] Added multiple rotation speeds to left and right

### v2.5 CHANGES ###
- [FEATURE] Adds more ground services
- [FEATURE] App no longer steals focus from MSFS so audio remains
- [FEATURE] App will auto exit upon exit of MSFS
- [BUGFIX] Adds better exception handling

### v2.4 CHANGES ###
- [FEATURE] Adds tug speed setting
- [FEATURE] Adds parking brake button

### v2.3 CHANGES ###
- [FEATURE] Door buttons added to open/close doors
- [FEATURE] Changed pushback method to new method with forward tug ability
- [FEATURE] Added ability to scale app size
- [BUGFIX] Fix crash on sim disconnect

### v2.2 CHANGES ###
- [FEATURE] Fuel button added to call fuel truck.

### v2.1 CHANGES ###
- [FEATURE] Jetway button added to manage jetway connection.
- [FEATURE] Close button added to exit application.
- [FEATURE] Moving UI is possible now. Just hold left click on the gray area between jetway and close buttons to move it.
- [FEATURE] When you close the application last location is being saved and restored when you start application.
- [BUGFIX] Crash to desktop when click buttons before app connects to the sim is fixed.

### v2.0 CHANGES ###
- UI is recreated using WPF.
- Shotcut key is defined. Press Page Up to show/hide UI.

NOTE:  If you have multiple monitor you can move UI by pressing Shift + Windows + Left or Right arrow keys together.

### SUPPORT ###
You can access project page, new versions and issues by https://github.com/metindikbas/msfs-pushback-helper-app

### CONTRIBUTION ###
You can use this project and application under GNU license.
You can also help this project to grow by contributing on Github.

### CREATOR ###
dikbas.metin@gmail.com
https://www.linkedin.com/in/metindikbas

### CONTRIBUTERS ###
gustavosjp - https://github.com/gustavosjp
HydronicDev - https://github.com/HydronicDev
alex-johnson - https://github.com/alex-johnson
aphalon - https://github.com/aphalon
