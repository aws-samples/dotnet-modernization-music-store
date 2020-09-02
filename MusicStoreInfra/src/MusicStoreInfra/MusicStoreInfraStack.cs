using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.SecretsManager;
using Amazon.CDK.AWS.RDS;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.ECS.Patterns;
using Amazon.CDK.AWS.ElasticLoadBalancingV2;
using Amazon.CDK.AWS.ECR;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace MusicStoreInfra
{
    public class MusicStoreInfraStack : Stack
    {
        internal string ContextVar(string contextVar, string defaultVal)
        {
            object val = this.Node.TryGetContext(contextVar);
            string strVal = val?.ToString();
            return string.IsNullOrEmpty(strVal) ? defaultVal : strVal;
        }

        internal string DbUsername => ContextVar("dbUsername", "sa");

        internal MusicStoreInfraStack(Construct scope, string id = "Music-Store-Hosting-Env-Stack", IStackProps props = null) : base(scope, id, props)
        {
            var dbPasswordSecret = new Amazon.CDK.AWS.SecretsManager.Secret(this, "DbPasswordSecret", new SecretProps
            {
                SecretName = "music-store-database-password",
                GenerateSecretString = new SecretStringGenerator
                {
                    PasswordLength=10,
                    ExcludeCharacters="/@\";{}`<>() "
                }
            });

            var vpc = new Vpc(this, "New-VPC", new VpcProps { MaxAzs = 3 });

            var dbServer = new DatabaseInstance(this, "RDS-SQL-Server", new DatabaseInstanceProps
            {
                Vpc = vpc,
                InstanceType = InstanceType.Of(InstanceClass.BURSTABLE3, InstanceSize.SMALL),
                VpcPlacement = new SubnetSelection
                {
                    SubnetType = SubnetType.PRIVATE
                },

                DeletionProtection = false,
                InstanceIdentifier = "Music-Store-SQL-Server",
                Engine = DatabaseInstanceEngine.SqlServerWeb(new SqlServerWebInstanceEngineProps
                {
                    Version = SqlServerEngineVersion.VER_14
                }),

                MasterUsername = this.DbUsername,
                MasterUserPassword = dbPasswordSecret.SecretValue,
                RemovalPolicy = RemovalPolicy.DESTROY
            });

            // --------------------- ECS ------------------------

            var ecsCluster = new Cluster(this, "ECS_Cluster", new ClusterProps
            {
                Vpc = vpc,
                ClusterName = "Music-Store"
            });

            string ecrName = Fn.ImportValue(BuildEnvStack.ecrRepoNameExport);
            var ecrRepo = Repository.FromRepositoryName(this, "Existing-ECR-Repo", ecrName);

            var lbFargateSvc = new ApplicationLoadBalancedFargateService(this, "ECS_Fargate-Service", 
                new ApplicationLoadBalancedFargateServiceProps
                {
                    Cluster = ecsCluster,
                    Cpu = 256,
                    MemoryLimitMiB = 1024,
                    PublicLoadBalancer = true,
                    LoadBalancer = new ApplicationLoadBalancer(this, "ALB", new Amazon.CDK.AWS.ElasticLoadBalancingV2.ApplicationLoadBalancerProps
                    {
                        LoadBalancerName = "Music-Store",
                        Vpc = vpc,
                        InternetFacing = true,
                        DeletionProtection = false
                    }),

                    TaskImageOptions = new ApplicationLoadBalancedTaskImageOptions
                    {
                        Image = ContainerImage.FromEcrRepository(ecrRepo, "latest"),
                        Environment = new Dictionary<string, string>()
                        {
                            { 
                                "ConnectionStrings__MusicStoreEntities", 
                                FormatConnectionString(dbServer.DbInstanceEndpointAddress, "MusicStore", dbPasswordSecret.SecretValue) 
                            },
                            { 
                                "ConnectionStrings__IdentityConnection",
                                FormatConnectionString(dbServer.DbInstanceEndpointAddress, "identity", dbPasswordSecret.SecretValue)
                            }
                        }
                    }
                });

            dbServer.Connections.AllowDefaultPortFrom(lbFargateSvc.Service.Connections.SecurityGroups[0]);
        }

        private string FormatConnectionString(string serverAddress, string dbName, object dbPassword) =>
            $"Server={serverAddress}; Database={dbName}; User Id={this.DbUsername}; Password={dbPassword}";
    }
}
