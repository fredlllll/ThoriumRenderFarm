﻿using System.Linq;
using Thorium_Shared;
using Thorium_Shared.Logging;

namespace Thorium_Client
{
    public class Program
    {
        static void Main(string[] args)
        {
            Logging.SetupLogging();

            PluginLoader.LoadPlugins();

            var client = new ThoriumClient();
            client.Start();

            if(args.Contains("-menu"))
            {
                ConsoleMenu menu = new ConsoleMenu();
                //TODO?
                menu.Run();
            }
        }
    }
}