using Amazon.CDK;

namespace MusicStoreInfra
{
    sealed class Program
    {
        public static void Main(string[] args)
        {
            var app = new App();

            new BuildInfraStack(app);
            new HostingInfraStack(app);

            app.Synth();
        }
    }
}
