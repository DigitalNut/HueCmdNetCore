using System;
using System.Collections.Generic;
using System.Text;

namespace HueCmdNetCore
{
    partial class Program
    {
        static bool ParseCmdLine(string[] args)
        {
            int len = args.Length;

            if (len == 0)
            {
                PrintUsage();
                return false;
            }

            // initialize light member, so we don't have to check for it being NULL.
            CmdLineOptions.light = new string[0];

            try
            {
                int i = 0;
                while (i < len)
                {
                    switch (args[i])
                    {
                        case "-ip":
                            i++;
                            CmdLineOptions.ip = args[i];
                            break;
                        case "-light":
                            i++;
                            ParseLights(args[i]);
                            break;
                        case "-brightness":
                            i++;
                            CmdLineOptions.brightness = Byte.Parse(args[i]);
                            break;
                        case "-color":
                            i++;
                            CmdLineOptions.color = args[i].Trim();
                            break;
                        case "-key":
                            i++;
                            CmdLineOptions.key = args[i];
                            break;
                        case "-register":
                            i++;
                            CmdLineOptions.Register = args[i];
                            i++;
                            CmdLineOptions.key = args[i];
                            break;
                        case "-status":
                            CmdLineOptions.status = true;
                            break;
                        case "-on":
                            CmdLineOptions.on = true;
                            break;
                        case "-off":
                            CmdLineOptions.off = true;
                            break;
                        default:
                            PrintUsage();
                            return false;
                    }
                    i++;
                }
            }
            catch
            {
                Console.WriteLine("Invalid or missing parameters. Please check.");
                Console.WriteLine("Enter command with no parameters to see help");
                return false;
            }

            if (String.IsNullOrEmpty(CmdLineOptions.key))
            {
                Console.WriteLine("Missing key. Is you don't have a key, use the register option to assign one");
                return false;
            }

            if (!String.IsNullOrEmpty(CmdLineOptions.Register))
            {
                if (String.IsNullOrEmpty(CmdLineOptions.key))
                {
                    Console.WriteLine("Missing key. Register options requires both app name and key");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Parse the '-light' argument 
        /// </summary>
        /// <param name="lightArgs"></param>
        private static void ParseLights(string lightArgs)
        {
            if (String.IsNullOrEmpty(lightArgs))
                return;

            CmdLineOptions.light = lightArgs.Split(',');
        }

        /// <summary>
        /// Print usage
        /// </summary>
        static void PrintUsage()
        {
            Console.WriteLine("HueCmdNetCore " + VersionString );
            Console.WriteLine(" by DigitalNut");
            Console.WriteLine(" Controls the Philip Hue from your command line\n");
            Console.WriteLine("-ip address          - [optional] IP address of HUE, otherwise will try to \n\t\t\tlocate it automatically");
            Console.WriteLine("-key <key>           - [mandatory] App Key needed to connect to the Hue");
            Console.WriteLine("-light <id>,[id]     - Light to control. Enter 1 to max lights. 0 is only valid with -status");
            Console.WriteLine("                       More then one light can be entered using comma as separator");
            Console.WriteLine("-brightness <number> - Brightness level 0 - 255");
            Console.WriteLine("-color <color>       - Color value in hex 'rrggbb'. E.g. 00FF00");
            Console.WriteLine("                       or <color> can also be 'Once' for alert once or 'Multi' for multiple alerts");
            Console.WriteLine("                       or <color> can also be 'ColorLoop' to start a color loop");
            Console.WriteLine("                       or <color> can be 'None' to stop ColorLoop or Multi Alert");
            Console.WriteLine("-on                  - Turns the light off");
            Console.WriteLine("-off                 - Turns the light on");
            Console.WriteLine("-status              - Returns the status of the bridge or light. Use the -light option to specify the light.");
            Console.WriteLine("                       Use 0 for status of all lights");
            Console.WriteLine("                       This option overrides other commands");
            Console.WriteLine("-register <appName> <appkey>  \n\t\t\t- Registry App Name & App Key with the Hue. Requires Name and Key. \n" +
                              "\t\t\tA Key must be registered with the Hue before using the other options \n" +
                              "\t\t\tExample: HueCmd -register HueCmd SomeKey1234");
            Console.WriteLine("Examples:");
            Console.WriteLine("\tHueCmd -key SomeKey1234 -light 4 -brightness 10\t\tSets light 4 to brightness level of 10");
            Console.WriteLine("\tHueCmd -key SomeKey1234 -light 3 -color ff0000\t\tSets light 3 to red");
        }
    }
}
