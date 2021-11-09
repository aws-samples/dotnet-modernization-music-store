using Amazon.CDK;
using Amazon.CDK.AWS.CodeCommit;

namespace MvcMusicStoreInfra
{
    public class MusicStoreInfraPipeline : Stack
    {
        internal MusicStoreInfraPipeline(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            //Add CodeCommit Repo
            var repo = new Repository(this, "MusicStoreRepo", new RepositoryProps
            {
                RepositoryName = "MusicStoreRepo"
            });

            new CfnOutput(this, "RepoHTTP", new CfnOutputProps { Value = repo.RepositoryCloneUrlHttp });
        }
    }
}
