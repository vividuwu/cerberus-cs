﻿using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Cerberus.Database;

namespace Cerberus {
    public static class Program {
        public static async Task Main(string[] args) {
            DotEnv envVars = new DotEnv();
            string token = envVars.Get("DISCORD_TOKEN");
            string dbAddress = envVars.Get("REDIS_ADDRESS");
            string vrcUsername = envVars.Get("VRC_USERNAME");
            string vrcPassword = envVars.Get("VRC_PASSWORD");
            bool usingOtp = Boolean.Parse(envVars.Get("VRC_OTP"));
            
            DatabaseMiddleware db = await DatabaseMiddleware.ConnectAsync(dbAddress);

            // string otp = null;
            // if (usingOtp) {
            //     Console.Write("VRChat OTP > ");
            //     otp = Console.ReadLine();
            // }

            VrchatLoginCredentials credentials = new VrchatLoginCredentials { 
                Username = vrcUsername,
                Password = vrcPassword,
                UsingOtp = usingOtp,
                // OtpCode = otp  
            };

            IHost host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((ctx, collection) => {
                collection.AddSingleton<VRChatAPI>();
                collection.AddSingleton<DatabaseMiddleware>(db);
                collection.AddSingleton<VrchatLoginCredentials>(credentials);
                collection.AddSingleton<String>(token);
                collection.AddSingleton<IHostedService, LoonieBot>();
                collection.AddLogging();
            })
            .ConfigureLogging((context, builder) => {
                builder.ClearProviders();
                builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                builder.AddConsole();
            })
            .Build();

            await host.RunAsync();
        }
    }
}