using Amazon.CDK;

namespace MvcMusicStoreInfra
{
    public class MusicStoreInfraPipelineStage : Stage
    {
        public MusicStoreInfraPipelineStage(Construct scope, string id, StageProps props = null) : base(scope, id, props)
        {
            new MusicStoreInfraResourcesStack(this, "MusicStoreInfraResourcesStack");
        }

    }
}