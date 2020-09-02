using Amazon.CDK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MusicStoreInfra
{
    sealed class Program
    {
        public static void Main(string[] args)
        {
            var app = new App();

            Stack[] stacks = {
                new BuildEnvStack(app),
                new MusicStoreInfraStack(app)
            };

            var stackNames = stacks.Select(stack => stack.StackName);
            string allStackNames = string.Join("\", \"", stackNames);

            Console.WriteLine($"Stacks that can be deployed: \"{allStackNames}\".");

            app.Synth();
        }
    }
}
