using System.Collections.Generic;
using System.IO;
using System.Linq;
using Amazon.CDK;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.Ecr.Assets;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.ECS.Patterns;
using Constructs;

namespace MvcMusicStoreInfra
{
    public class MusicStoreInfraResourcesStack : Stack
    {
        internal MusicStoreInfraResourcesStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            //VPC
            var vpc = new Vpc(this, "cluster-vpc", new VpcProps
            {
                Cidr = "172.51.0.0/16",
                MaxAzs = 3
            });

            // Import DynamoDb Tables
            var catalogTable = Table.FromTableArn(this, "imported-album-table", $"arn:aws:dynamodb:{this.Region}:{this.Account}:table/Catalog");

            //ECR
            //Build docker image and publish on ECR Repository
            var asset = new DockerImageAsset(this, "web-api-docker-image", new DockerImageAssetProps
            {
                Directory = Path.Combine(Directory.GetCurrentDirectory(), "../MvcMusicStore.CatalogApi"),
                File = "Dockerfile"
            });

            // Create ECS Cluster 
            var cluster = new Cluster(this, "demo-cluster", new ClusterProps
            {
                Vpc = vpc,
            });

            //L3 Construct for ALB + ECS + Fargate
            var loadBalancedFargateService = new ApplicationLoadBalancedFargateService(this, "demo-ecs-fargate-service", new ApplicationLoadBalancedFargateServiceProps
            {
                Cluster = cluster,
                MemoryLimitMiB = 1024,
                Cpu = 512,
                TaskImageOptions = new ApplicationLoadBalancedTaskImageOptions
                {
                    Image = ContainerImage.FromDockerImageAsset(asset),
                    Environment = new Dictionary<string, string>()
                        {
                            {"AWS_REGION", this.Region},
                            {"ASPNETCORE_ENVIRONMENT","Development"},
                            {"ASPNETCORE_URLS","http://+:80"}
                        }
                }
            });

            //Grant Read Permission
            catalogTable.GrantReadData(loadBalancedFargateService.Service.TaskDefinition.TaskRole);
        }
    }
}
