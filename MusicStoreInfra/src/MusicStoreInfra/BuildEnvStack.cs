using Amazon.CDK;
using Amazon.CDK.AWS.ECR;

namespace MusicStoreInfra
{
    public class BuildEnvStack : Stack
    {
        internal const string ecrRepoNameExport = "music-store-ecr-repo-name";

        public BuildEnvStack(Construct scope, string id = "Music-Store-Build-Env-Stack", IStackProps props = null) : base(scope, id, props)
        {
            var ecrRepo = new Repository(this, "ECR-repo", new RepositoryProps
            {
                RepositoryName = "music-store"
            });

            new CfnOutput(this, "Ecr-Repo-Name", new CfnOutputProps
            {
                Value = ecrRepo.RepositoryName,
                ExportName = ecrRepoNameExport
            });

            new CfnOutput(this, "Ecr-Repo-URI", new CfnOutputProps
            {
                Value = ecrRepo.RepositoryUri
            });
        }
    }
}
