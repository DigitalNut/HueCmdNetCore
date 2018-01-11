# HueCmdNetCore
Windows Command Line Tool for controlling the Philips Hue. Use it in scripts/batch files.
HueCmd - .Net Core 2.0 version. Works on Windows desktop, Linux (x64 and ARM) and Windows IOT (ARM). 

Prebuilt executable is in the EXE folder for desktop and ARM version (works on Windows 10 IOT Raspberry PI).

```
Command line options:
-ip address          - [optional] IP address of HUE, otherwise will try to
                        locate it automatically                        
-key <key>           - [mandatory] App Key needed to connect to the Hue
-light <id>,[id]     - Lights to control. Enter ID value of 1 to max lights. 0 is only valid with -status
					   More then one light can be entered using comma as separator
-brightness <number> - Brightness level 0 - 255
-color <color>       - Color value in hex 'rrggbb'. E.g. 00FF00 
					   or common color names (e.g. red, blue, ...)
                       or <color> can also be 'Once' for alert once or 'Multi' for multiple alerts
                       or <color> can also be 'ColorLoop' to start a color loop
                       or <color> can be 'None' to stop ColorLoop or Multi Alert
-on                  - Turns the light off
-off                 - Turns the light on
-status              - Returns the status of the bridge or light. Use the -light option to specify the light.
                       Use 0 for status of all lights
                       This option overrides other commands
-register <appName> <appkey>
                        - Registry App Name & App Key with the Hue. Requires Name and Key.
                        A Key must be registered with the Hue before using the other options
                        Example: HueCmd -register HueCmd SomeKey1234
Examples:
        HueCmd -key SomeKey1234 -light 4 -brightness 10         Sets light 4 to brightness level of 10
        HueCmd -key SomeKey1234 -light 3,7 -color ff0000        Sets lights 3 and 7 to red
```

uses https://github.com/Q42/Q42.HueApi
