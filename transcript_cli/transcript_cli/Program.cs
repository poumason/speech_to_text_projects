﻿using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace transcript_cli
{
    class Program
    {
        static void Main(string[] args)
        {
            var app = new CommandLineApplication();
            app.Name = "aitrans";
            app.HelpOption("-?|-h|--help");

            app.OnExecute(() =>
            {
                Console.WriteLine("Hello World!");
                return 0;
            });

            app.Command("subscription", (command) =>
            {
                command.Description = "Set Your Subscription Key and Your Service Region";
                command.HelpOption("-?|-h|--help");
                var keyOpt = command.Option("-k|--key <subscription-key>",
                                       "Your Subscription Key",
                                       CommandOptionType.SingleValue);
                var regionOpt = command.Option("-r|--region <service-region>",
                                       "Your Service Region",
                                       CommandOptionType.SingleValue);

                command.OnExecute(() =>
                {
                    const string AZURE_SUBSCRIPTION_KEY = "azure-subscription-key";
                    const string AZURE_SERVICE_REGION = "azure-service-region";

                    string subscriptionKey = keyOpt.Value();
                    string serviceRegion = regionOpt.Value();

                    Environment.SetEnvironmentVariable(AZURE_SUBSCRIPTION_KEY, subscriptionKey);
                    Environment.SetEnvironmentVariable(AZURE_SERVICE_REGION, serviceRegion);

                    Console.WriteLine($"before: {Environment.GetEnvironmentVariable(AZURE_SUBSCRIPTION_KEY)}, {Environment.GetEnvironmentVariable(AZURE_SERVICE_REGION)}");
                    Console.WriteLine($"after: {Environment.GetEnvironmentVariable(AZURE_SUBSCRIPTION_KEY)}, {Environment.GetEnvironmentVariable(AZURE_SERVICE_REGION)}");

                    return 0;
                });

            });

            app.Command("hide", (command) =>
            {

                command.Description = "Instruct the ninja to hide in a specific location.";
                command.HelpOption("-?|-h|--help");

                var locationArgument = command.Argument("[location]",
                                           "Where the ninja should hide.");

                command.OnExecute(() =>
                {
                    var location = locationArgument.Value != null
                      ? locationArgument.Value
                      : "in a trash can";
                    Console.WriteLine("Ninja is hidden " + location);

                    return 0;
                });

            });

            app.Command("attack", (command) =>
            {
                command.Description = "Instruct the ninja to go and attack!";
                command.HelpOption("-?|-h|--help");

                var excludeOption = command.Option("-e|--exclude <exclusions>",
                                        "Things to exclude while attacking.",
                                        CommandOptionType.MultipleValue);

                var screamOption = command.Option("-s|--scream",
                                       "Scream while attacking",
                                       CommandOptionType.NoValue);

                command.OnExecute(() =>
                {
                    var exclusions = excludeOption.Values;
                    Console.WriteLine(string.Join(',', exclusions.ToList()));
                    var attacking = (new List<string>
                {
                    "dragons",
                    "badguys",
                    "civilians",
                    "animals"
                }).Where(x => !exclusions.Contains(x));

                    Console.Write("Ninja is attacking " + string.Join(", ", attacking));

                    if (screamOption.HasValue())
                    {
                        Console.Write(" while screaming");
                    }

                    Console.WriteLine();

                    return 0;

                });
            });
            app.Execute(args);
        }
    }
}