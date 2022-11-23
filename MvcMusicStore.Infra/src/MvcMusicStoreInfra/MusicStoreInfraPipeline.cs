using Amazon.CDK;
using Amazon.CDK.Pipelines;
using Constructs;

namespace MvcMusicStoreInfra
{
    public class MusicStoreInfraPipeline : Stack
    {
        internal MusicStoreInfraPipeline(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            var pipeline = new CodePipeline(this, "pipeline", new CodePipelineProps
            {
                PipelineName = "MusicStore-Pipeline",
                Synth = new ShellStep("Synth", new ShellStepProps
                {
                    Input = CodePipelineSource.GitHub("aws-samples/dotnet-modernization-music-store", "XNT306_CatalogAPI_CDK_Completed"),
                    PrimaryOutputDirectory = "MvcMusicStore.Infra/cdk.out",
                    InstallCommands = new string[]{
                        "npm install -g aws-cdk",
                    },
                    Commands = new string[] {
                            "cd MvcMusicStore.Infra",
                            "dotnet build src",
                            "cdk synth"
                        }
                }),

            });

            pipeline.AddStage(new MusicStoreInfraPipelineStage(this, "Deploy"));
        }
    }
}
