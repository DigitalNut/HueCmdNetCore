using Q42.HueApi;
using Q42.HueApi.ColorConverters.HSB;
using Q42.HueApi.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HueCmdNetCore
{
    partial class Program
    {
        static readonly string VersionString = "1.1";

        struct StructCmdLineOptions
        {
            public string ip;
            public Byte? brightness;
            public string color;
            public bool? on;
            public bool? off;
            public string[] light;
            public string key;
            public string Register;
            public bool? status;
        }

        static StructCmdLineOptions CmdLineOptions;

        static void Main(string[] args)
        {
            // Hack to get the Raspberry PI to connect when running under Mono (on Linux). Otherwise Mono on the Pi doesn't connect 
            //  to any HTTPS sites that don't have a certificate. This just bypasses the cert test
            // http://stackoverflow.com/questions/3285489/mono-problems-with-cert-and-mozroots
            //ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) => { return true; };

            if (!ParseCmdLine(args))
            {
                Environment.Exit(1);
                return;
            }

            try
            {
                if (!String.IsNullOrEmpty(CmdLineOptions.Register))
                {
                    Task t = Register();
                    t.Wait();
                }
                else if (CmdLineOptions.status != null)
                {
                    RunStatusCommand().Wait();
                    //t.Wait();
                }
                else
                {
                    Task t = RunLightCommand();
                    t.Wait();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not connect to Hue.");
                string err = e.Message;
                if (e.InnerException != null)
                    err = e.InnerException.Message;
                Console.WriteLine("Additional detail: " + err);
            }

            Environment.Exit(0);
        }



        async static Task Register()
        {
            string ip = await GetOrFindIP();

            if (String.IsNullOrEmpty(ip))
                return;

            ILocalHueClient client = new LocalHueClient(ip);
            var appKey = await client.RegisterAsync(CmdLineOptions.Register, CmdLineOptions.key);
        }

        async static Task RunLightCommand()
        {
            LocalHueClient client = await GetClient();

            if (client == null)
                return;

            var command = new LightCommand();
            if (CmdLineOptions.on != null)
                command.On = true;
            else if (CmdLineOptions.off != null)
                command.On = false;

            if (CmdLineOptions.brightness != null)
                command.Brightness = CmdLineOptions.brightness;

            if (!String.IsNullOrEmpty(CmdLineOptions.color))
            {
                if (String.Compare(CmdLineOptions.color, "Once", true) == 0)
                    command.Alert = Alert.Once;
                else if (String.Compare(CmdLineOptions.color, "Multi", true) == 0)
                    command.Alert = Alert.Multiple;
                else if (String.Compare(CmdLineOptions.color, "ColorLoop", true) == 0)
                    command.Effect = Effect.ColorLoop;
                else if (String.Compare(CmdLineOptions.color, "None", true) == 0)
                {
                    command.Effect = Effect.None;
                    command.Alert = Alert.None;
                }
                else if (CmdLineOptions.color.Length == 6 && ((CmdLineOptions.color[0] >= '0' && CmdLineOptions.color[0] <= '9') ||
                                                             (CmdLineOptions.color[0] >= 'a' && CmdLineOptions.color[0] <= 'f') ||
                                                             (CmdLineOptions.color[0] >= 'A' && CmdLineOptions.color[0] <= 'F')))
                {
                    //if (CmdLineOptions.color.Length != 6)
                    //{
                    //    Console.WriteLine("Color value must be 6 characters. E.g. 00ff00");
                    //    return;
                    //}
                    command.SetColor(new Q42.HueApi.ColorConverters.RGBColor(CmdLineOptions.color));
                }
                else
                {
                    // treat the color cmd line arg as a color name (e.g. "red")
                    // FromName will return RGB(0,0,0) if the name is not valid
                    System.Drawing.Color cc = System.Drawing.Color.FromName(CmdLineOptions.color);
                    if (cc.ToArgb() == 0)
                    {
                        Console.WriteLine("Incorrect color option. Check spelling. Black in not allowed.");
                        return;
                    }
                    command.SetColor(new Q42.HueApi.ColorConverters.RGBColor((int)cc.R, (int)cc.G, (int)cc.B));
                }
            }

            //List<string> lights = new List<string>();
            //lights.Add(CmdLineOptions.light);
            await client.SendCommandAsync(command, CmdLineOptions.light );
            //await client.SendCommandAsync(command, new List<string> { CmdLineOptions.light });
        }


        static async Task RunStatusCommand()
        {
            LocalHueClient client = await GetClient();

            if (client == null)
                return;

            if (CmdLineOptions.light.Count() == 0)
            {
                var config = await client.GetBridgeAsync();
                if (config != null)
                {
                    Console.WriteLine("        Model: " + config.Config.ModelId);
                    Console.WriteLine("         Name: " + config.Config.Name);
                    Console.WriteLine("   SW Version: " + config.Config.SoftwareVersion);
                    Console.WriteLine("  API Version: " + config.Config.ApiVersion);
                    Console.WriteLine("  Portal Conn: " + config.Config.PortalConnection);
                }
            }
            else if (CmdLineOptions.light.Count() == 1 && String.Compare(CmdLineOptions.light[0], "0") == 0)
            {
                var lights = await client.GetLightsAsync();
                if (lights == null)
                {
                    Console.WriteLine("Could not enumerate list. Hue may not be reachable.");
                    return;
                }
                foreach (Light light in lights)
                {
                    PrintLightDetail(light);
                    Console.WriteLine("");
                }
            }
            else if (CmdLineOptions.light.Count() >= 1)
            {
                var light = await client.GetLightAsync(CmdLineOptions.light[0]);
                if (light == null)
                {
                    Console.WriteLine("Light ID not found");
                    return;
                }

                PrintLightDetail(light);
            }
            else
            {
                Console.WriteLine("incorrect 'light' command line argument.");
                return;
            }
        }

        static void PrintLightDetail(Light light)
        {
            Console.WriteLine("Device ======================================== ");
            Console.WriteLine("         ID: " + light.Id);
            Console.WriteLine("       Name: " + light.Name);
            Console.WriteLine("State ----------------------------------------- ");
            Console.WriteLine("  Reachable: " + light.State.IsReachable);
            Console.WriteLine("         On: " + light.State.On);
            Console.WriteLine(" Brightness: " + light.State.Brightness);
            Console.WriteLine("     Effect: " + light.State.Effect);
            Console.WriteLine("      Alert: " + light.State.Alert);
            Console.WriteLine("Info ------------------------------------------ ");
            Console.WriteLine("       Type: " + light.Type);
            Console.WriteLine("   Model ID: " + light.ModelId);
            Console.WriteLine("Manufacture: " + light.ManufacturerName);
            Console.WriteLine("  Unique ID: " + light.UniqueId);
            Console.WriteLine(" SW Version: " + light.SoftwareVersion);
        }

        /// <summary>
        /// Helper function to create a HueCient
        /// </summary>
        /// <returns></returns>
        static async Task<LocalHueClient> GetClient()
        {
            LocalHueClient client = null;

            string ip = await GetOrFindIP();

            if (String.IsNullOrEmpty(ip))
                return null;

            client = new LocalHueClient(ip);
            client.Initialize(CmdLineOptions.key);

            if (!client.IsInitialized)
                return null;

            return client;
        }

        /// <summary>
        /// Return the command line IP address that was entered by the user or IP found by the bridge locater service
        /// </summary>
        /// <param name="ip"></param>
        static async Task<string> GetOrFindIP()
        {
            string ip = CmdLineOptions.ip;

            if (String.IsNullOrEmpty(CmdLineOptions.ip))
            {
                IBridgeLocator locator = new HttpBridgeLocator();
                IEnumerable<Q42.HueApi.Models.Bridge.LocatedBridge> bridgeIPs = await locator.LocateBridgesAsync(TimeSpan.FromSeconds(10));

                ////For Windows 8 and .NET45 projects you can use the SSDPBridgeLocator which actually scans your network. 
                ////See the included BridgeDiscoveryTests and the specific .NET and .WinRT projects
                //SSDPBridgeLocator locator = new SSDPBridgeLocator();
                //IEnumerable<string> bridgeIPs = await locator.LocateBridgesAsync(TimeSpan.FromSeconds(10));

                if (bridgeIPs.Any())
                {
                    ip = bridgeIPs.First().IpAddress;
                    Console.WriteLine("Bridge found using IP address: " + ip);
                }
                else
                {
                    Console.WriteLine("Scan did not find a Hue Bridge. Try suppling a IP address for the bridge");
                    return null;
                }
            }

            return ip;
        }
    }
}

