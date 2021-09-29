using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.RDS;
using Amazon.CDK.AWS.SecretsManager;
using System;

namespace MusicStoreInfra
{
    public class MusicStoreRDSStack : Stack
    {
        private static string FormatConnectionString(string serverAddress, string dbName, object password) =>
            $"Server={serverAddress}; Database={dbName}; User Id=sa; Password={password}";

        internal MusicStoreRDSStack(Construct scope, string id= "Music-Store-Windows-Hosting-Env-Stack", IStackProps props = null) : base(scope, id, props)
        {
            var dbPasswordSecret = new Amazon.CDK.AWS.SecretsManager.Secret(this, "DbPasswordSecret", new SecretProps
            {
                SecretName = "music-store-database-password",
                GenerateSecretString = new SecretStringGenerator
                {
                    ExcludeCharacters = "/@\" ",
                    PasswordLength = 10,
                }
            });

            var vpc = Vpc.FromLookup(this, "WorkshopFrameworkToCore", new VpcLookupOptions {VpcName= "WorkshopFrameworkToCore" });

            var database = new DatabaseInstance(this, $"RDS-SQL-Server",
                new DatabaseInstanceProps
                {
                    Vpc = vpc,
                    InstanceType = InstanceType.Of(InstanceClass.BURSTABLE3, InstanceSize.SMALL),
                    VpcSubnets = new SubnetSelection
                    {
                        SubnetType = SubnetType.PRIVATE
                    },

                    DeletionProtection = false,
                    InstanceIdentifier = "Music-Store-SQL-Server",
                    Engine = DatabaseInstanceEngine.SqlServerWeb(new SqlServerWebInstanceEngineProps { Version = SqlServerEngineVersion.VER_14 }),
                    
                    Credentials = Credentials.FromPassword(
                        username: "sa",
                        password: dbPasswordSecret.SecretValue
                        ),
                    RemovalPolicy = RemovalPolicy.DESTROY
                }
            );

            string mainDbConnectionString = FormatConnectionString(database.DbInstanceEndpointAddress, "MusicStore", dbPasswordSecret.SecretValue);
            string identityDbConnectionString = FormatConnectionString(database.DbInstanceEndpointAddress, "Identity", dbPasswordSecret.SecretValue);

            Console.WriteLine(mainDbConnectionString);
            Console.WriteLine(identityDbConnectionString);

            var sg = new SecurityGroup(this, "EC2-SG", new SecurityGroupProps
            {
                SecurityGroupName = "EC2-RDS",
                Vpc = vpc
            });

            sg.AddIngressRule(Peer.AnyIpv4(), Port.Tcp(80));
            sg.AddIngressRule(Peer.AnyIpv4(), Port.Tcp(8080));

            database.Connections.AllowDefaultPortFrom(sg);

        }
    }
}
