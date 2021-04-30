using Amazon.CDK;
using Amazon.CDK.AWS.ECR;
using Amazon.CDK.AWS.SecretsManager;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.RDS;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.ECS.Patterns;
using Amazon.CDK.AWS.ElasticLoadBalancingV2;
using System.Collections.Generic;

namespace MusicStoreInfra
{
    public class HostingInfraStack : Stack
    {
        internal string EcsClusterName => this.CmdLineArg("EcsClusterName", "Music-Store");
        internal string EcsServiceName => this.CmdLineArg("EcsServiceName", "music-store");
        internal string DbUsername => CmdLineArg("DbUsername", "sa");
        internal string EcrRepoName => this.CmdLineArg("EcrRepoName", "music-store");
        internal string ImageTag => this.CmdLineArg("ContainerImageTag", "latest");
        internal int ReplicaCount => int.Parse(this.Node.TryGetContext("ReplicaCount")?.ToString() ?? "1");

        internal HostingInfraStack(Construct scope, string id = "Music-Store-Hosting-Infra", IStackProps props = null) : base(scope, id, props)
        {
            var dbPasswordSecret = new Amazon.CDK.AWS.SecretsManager.Secret(this, "DbPasswordSecret", new SecretProps
            {
                SecretName = "music-store-database-password",
                GenerateSecretString = new SecretStringGenerator
                {
                    PasswordLength = 12,
                    ExcludeCharacters = "/@\";{}`<>() "
                }
            });

            var vpc = new Vpc(this, "music-store-hosting-vpc", new VpcProps { MaxAzs = 3 });

            var dbServer = new DatabaseInstance(this, "music-store-RDS-SQL-Server", new DatabaseInstanceProps
            {
                Vpc = vpc,
                VpcSubnets = new SubnetSelection { SubnetType = SubnetType.PRIVATE },
                DeletionProtection = false,
                RemovalPolicy = RemovalPolicy.DESTROY,
                InstanceIdentifier = "Music-Store-DB-Server",
                Engine = DatabaseInstanceEngine.SqlServerWeb(new SqlServerWebInstanceEngineProps { Version = SqlServerEngineVersion.VER_15 }),
                Credentials = Credentials.FromPassword(this.DbUsername, dbPasswordSecret.SecretValue)
            });

            var ecsCluster = new Cluster(this, "music-store-ECS-cluster", 
                new ClusterProps { Vpc = vpc, ClusterName = this.EcsClusterName }
            );

            string ecrRepoName = this.EcrRepoName;
            IRepository ecrRepo = Repository.FromRepositoryName(this, "music-store-existing-ECR-repo", ecrRepoName);

            var lbFargateSvc = new ApplicationLoadBalancedFargateService(this, "msuic-store-ecs-service",
                new ApplicationLoadBalancedFargateServiceProps
                {
                    Cluster = ecsCluster,
                    DesiredCount = this.ReplicaCount,
                    ServiceName = this.EcsServiceName,
                    LoadBalancer = new ApplicationLoadBalancer(this, "music-store-ALB",
                        new Amazon.CDK.AWS.ElasticLoadBalancingV2.ApplicationLoadBalancerProps
                        {
                            LoadBalancerName = "Music-Store-ALB",
                            Vpc = vpc,
                            InternetFacing = true,
                        }),
                    TaskImageOptions = new ApplicationLoadBalancedTaskImageOptions
                    {
                        Image = ContainerImage.FromEcrRepository(ecrRepo, this.ImageTag),
                        Environment = new Dictionary<string, string>
                        {
                            ["ConnectionStrings__MusicStoreEntities"] = FormatConnectionString(dbServer.DbInstanceEndpointAddress, "MusicStore", dbPasswordSecret.SecretValue),
                            ["ConnectionStrings__IdentityConnection"] = FormatConnectionString(dbServer.DbInstanceEndpointAddress, "Identity", dbPasswordSecret.SecretValue)
                        }
                    }
                });

            dbServer.Connections.AllowDefaultPortFrom(lbFargateSvc.Service.Connections.SecurityGroups[0]);
        }

        private string FormatConnectionString(string serverAddress, string dbName, object dbPassword) =>
            $"Server={serverAddress}; Database={dbName}; User Id={this.DbUsername}; Password={dbPassword}";

        internal string CmdLineArg(string contextVar, string defaultVal)
        {
            object val = this.Node.TryGetContext(contextVar);
            string strVal = val?.ToString();
            return string.IsNullOrEmpty(strVal) ? defaultVal : strVal;
        }
    }
}