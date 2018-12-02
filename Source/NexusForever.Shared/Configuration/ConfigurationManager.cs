﻿using Microsoft.Extensions.Configuration;

namespace NexusForever.Shared.Configuration
{

    public static class ConfigurationManager<T>
    {
        public static T Config { get; private set; }

        public static IConfiguration Configuration { get; private set; }


        public static void Initialise(string file)
        {
            SharedConfiguration.Initialize(file);
            Config = Configuration.Get<T>();
        }
    }
}
