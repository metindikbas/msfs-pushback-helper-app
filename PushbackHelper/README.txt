PUSHBACK HELPER v2.6
Release date: 2024/05/01

### INSTALLATION ###
Extract all files to any location and run PushbackHelper.exe. There is no need to put into the community folder location.

To work with local variables like the parking brakes of the A32NX from FlyByWire, you need to download the `FSUIPC-WASM` from
Pete & John Dowson.

- Download the FSUIPC-WASMv0.5.1.zip or higher from here http://www.fsuipc.com/download/FSUIPC-WASMv0.5.1.zip and extract it
- Extract the `fsuipc-lvar-module.zip` to your community folder

### USAGE ###
- To show/hide UI press the Page Up key
- To connect or disconnect jetway simply click the JETWAY button.
- To call the fuel truck simply click the FUEL button.
- To start or stop push back simply click the TUG button.
- The tug will wait in place until either FORWARD or REVERSE is selected (remove the parking brake first or it will move slowly).
- Once moving, select LEFT or RIGHT to steer the tug.
- When done, click STOP. Apply the parking brake. Click the TUG button again to remove the tug.
- There are indicators near LEFT and RIGHT buttons to show current rotation speed. Click the button on the side you are already turning to increase speed and click the other side to decrease.

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
