using Amazon.CDK;
using Amazon.CDK.AWS.ECR;

namespace MusicStoreInfra
{
    public class BuildInfraStack : Stack
    {
        internal string ContextVar(string contextVar, string defaultVal)
        {
            object val = this.Node.TryGetContext(contextVar);
            string strVal = val?.ToString();
            return string.IsNullOrEmpty(strVal) ? defaultVal : strVal;
        }

        internal const string EcrRepoNameExport = "music-store-ecr-repo-name";
        internal string EcrRepoName => this.ContextVar("ecrRepoName", "music-store");


        public BuildInfraStack(Construct scope, string id = "Music-Store-CICD-Infra", IStackProps props = null) : base(scope, id, props)
        {
            var ecrRepo = new Repository(this, "ECR-repo", new RepositoryProps
            {
                RepositoryName = this.EcrRepoName
            });

            new CfnOutput(this, "Ecr-Repo-Name", new CfnOutputProps
            {
                Value = ecrRepo.RepositoryName,
                ExportName = EcrRepoNameExport
            });

            new CfnOutput(this, "Ecr-Repo-URI", new CfnOutputProps
            {
                Value = ecrRepo.RepositoryUri
            });
        }
    }
}
