using System.Collections.Generic;
using System.Linq;
using Amazon.CDK;
using Amazon.CDK.AWS.CodeBuild;
using Amazon.CDK.AWS.CodeCommit;
using Ecr = Amazon.CDK.AWS.ECR;
using Amazon.CDK.AWS.CodePipeline;
using StageProps = Amazon.CDK.AWS.CodePipeline.StageProps;
using Amazon.CDK.AWS.CodePipeline.Actions;
using Amazon.CDK.AWS.IAM;
using IStageProps = Amazon.CDK.AWS.CodePipeline.IStageProps;

// ReSharper disable UnusedMethodReturnValue.Local
// ReSharper disable ObjectCreationAsStatement

namespace MusicStoreInfra
{
    public class BuildInfraStack : Stack
    {
        internal string EcrRepoName => this.CdmLineArg("ecrRepoName", "music-store");
        internal string ImageTag => this.CdmLineArg("ContainerImageTag", "latest");
        internal string EcsClusterName => this.CdmLineArg("EcsClusterName", "Music-Store");
        internal string EcsServiceName => this.CdmLineArg("EcsServiceName", "music-store");
        internal string GitBranchToBuild => this.CdmLineArg("GitBranchToBuild", "main");

        public BuildInfraStack(Construct scope, string id = "Music-Store-CICD-Infra", IStackProps props = null) : base(scope, id, props)
        {
            _ = this.ProvisionEcrRepo();
            Repository gitRepo = this.ProvisionGitRepo();
            this.CreateBuildPipeline(gitRepo);
        }

        private Ecr.Repository ProvisionEcrRepo()
            => new Ecr.Repository(this, "ECR-repo", new Ecr.RepositoryProps
            {
                RepositoryName = this.EcrRepoName,
                RemovalPolicy = RemovalPolicy.DESTROY
            });

        private Repository ProvisionGitRepo()
            => new Repository(this, "music-store-git-repo", new RepositoryProps
            {
                RepositoryName = "music-store",
                Description = "ASP.NET e-commerce sample app source code"
            });

        private Pipeline CreateBuildPipeline(Repository gitRepo)
        {
            Artifact_ sourceCodeArtifact = new Artifact_("Music-Store-Source");

            return new Pipeline(this, "music-store-build-pipeline", new PipelineProps
            {
                PipelineName = "music-store",
                Stages = new IStageProps[]
                {
                    StageFromActions("Checkout-Source-Code", 
                        new CodeCommitSourceAction(new CodeCommitSourceActionProps
                                    {
                                        ActionName = "Git-checkout-from-CodeCommit-repo",
                                        Repository = gitRepo,
                                        Output = sourceCodeArtifact,
                                        Branch = this.GitBranchToBuild,
                                    })
                    ),
                    StageFromActions("Build-container-image", this.ContainerImageBuildAction(sourceCodeArtifact)),
                    StageFromActions("Recycle-ECS-tasks", this.RestartEcsContainersAction(sourceCodeArtifact))
                }
            });
        }

        private CodeBuildAction ContainerImageBuildAction(Artifact_ sourceCodeArtifact)
            => new CodeBuildAction(new CodeBuildActionProps
            {
                Input = sourceCodeArtifact,
                ActionName = "Build-app-Docker-image",
                Type = CodeBuildActionType.BUILD,
                Project = new PipelineProject(this, "CodeBuildProject", new PipelineProjectProps
                {
                    ProjectName = "Music-Store-container-image-build",
                    BuildSpec = BuildSpec.FromObjectToYaml(
                        /*
                         * There two approaches to storing buildspec:
                         * 1. Use shared buildspec that is bundled together with your CI/CD infra-as-code, as shown here;
                         * 2. As a YAML file committed together with your main app code base. 
                         * Both have pros and cons.
                         * Advantages of the shared is that it's the only one to maintain, which is better if different branches can be built with the same buildspec.
                         * Advantages of the buildspec committed with the app source is that they could be changed w/o changing the rest of the build pipeline.
                         * Here we show shared buildspec building the branch that has no CI/CD code in it at all.
                         */
                        new Dictionary<string, object>
                        {
                            ["version"] = "0.2",
                            ["phases"] = new Dictionary<string, object> {
                                ["pre_build"] = new Dictionary<string, object> {
                                    ["commands"] = new [] {
                                        "echo ------------ Authorizing docker to push container images to ECR: ------------",
                                        "AWS_ACCOUNT_ID=$(aws sts get-caller-identity --query Account --output text)",
                                        "aws ecr get-login-password --region $AWS_DEFAULT_REGION | docker login --username AWS --password-stdin $AWS_ACCOUNT_ID.dkr.ecr.$AWS_DEFAULT_REGION.amazonaws.com"
                                    }
                                },
                                ["build"] = new Dictionary<string, object> {
                                    ["commands"] = new [] {
                                        "echo ------------ Building Music Store container image: ------------",
                                        "ECR_REPO_URI=\"${AWS_ACCOUNT_ID}.dkr.ecr.${AWS_DEFAULT_REGION}.amazonaws.com/${ECR_REPO_NAME}\"",
                                        "cd ./MvcMusicStore",
                                        "docker build -t ${ECR_REPO_URI}:${IMAGE_TAG} ."
                                    }
                                },
                                ["post_build"] = new Dictionary<string, object> {
                                    ["commands"] = new[] {
                                        "echo ------------ Pushing Music Store container image to ECR repo: -----------",
                                        "docker push ${ECR_REPO_URI}:${IMAGE_TAG}"
                                    }
                                }
                            }
                        }
                    ),
                    Environment = new BuildEnvironment
                    {
                        Privileged = true,
                        BuildImage = LinuxBuildImage.STANDARD_5_0,
                        EnvironmentVariables = new Dictionary<string, IBuildEnvironmentVariable>
                        {
                            ["ECR_REPO_NAME"] = new BuildEnvironmentVariable { Value = this.EcrRepoName },
                            ["IMAGE_TAG"] = new BuildEnvironmentVariable { Value = this.ImageTag },
                        },
                    },
                    Role = this.CreateCodeBuildRoleFromManagedPolicies("Music-store-code-build-role", "AmazonEC2ContainerRegistryPowerUser"),
                    Cache = Cache.Local(LocalCacheMode.SOURCE, LocalCacheMode.DOCKER_LAYER)
                })
            });

        /// <summary>
        /// Deployment stage recycles ECS containers w/o needing to update ECS Task Definition
        /// because it relies on mutable tag image, where container image to run has the same
        /// tag, like "latest" even when underlying image changes.
        /// </summary>
        private CodeBuildAction RestartEcsContainersAction(Artifact_ sourceCodeArtifact)
            => new CodeBuildAction(new CodeBuildActionProps
            {
                Input = sourceCodeArtifact,
                ActionName = "Restart-ECS-Containers-with-new-image",
                Project = new PipelineProject(this, "DeployStage", new PipelineProjectProps
                {
                    ProjectName = "Music-Store-recycle-ECS-containers",
                    BuildSpec = BuildSpec.FromObjectToYaml(
                        new Dictionary<string, object>
                        {
                            ["version"] = "0.2",
                            ["phases"] = new Dictionary<string, object> {
                                ["pre_build"] = new Dictionary<string, object> {
                                    ["commands"] = new[] {
                                        "aws ecs update-service --cluster ${ECS_CLUSTER_NAME} --service ${ECS_SERVICE_NAME} --force-new-deployment",
                                    }
                                }
                            }
                        }
                    ),
                    Environment = new BuildEnvironment
                    {
                        Privileged = true,
                        BuildImage = LinuxBuildImage.STANDARD_5_0,
                        EnvironmentVariables = new Dictionary<string, IBuildEnvironmentVariable>
                        {
                            ["ECS_CLUSTER_NAME"] = new BuildEnvironmentVariable { Value = this.EcsClusterName },
                            ["ECS_SERVICE_NAME"] = new BuildEnvironmentVariable { Value = this.EcsServiceName },
                        }
                    },
                    Role = this.CreateCodeBuildRoleFromManagedPolicies("Music-Store-ECS-container-recycle-role", "AmazonECS_FullAccess"),
                })
            });

        public Role CreateCodeBuildRoleFromManagedPolicies(string roleName, params string[] managedPolicyNames)
            => new Role(this, roleName, new RoleProps
            {
                RoleName = roleName,
                AssumedBy = new ServicePrincipal("codebuild.amazonaws.com"),
                ManagedPolicies = managedPolicyNames.Select(ManagedPolicy.FromAwsManagedPolicyName).ToArray()
            });

        public static StageProps StageFromActions(string stageName, params IAction[] actions) =>
            new StageProps { StageName = stageName, Actions = actions };

        internal string CdmLineArg(string contextVar, string defaultVal)
        {
            object val = this.Node.TryGetContext(contextVar);
            string strVal = val?.ToString();
            return string.IsNullOrEmpty(strVal) ? defaultVal : strVal;
        }
    }
}