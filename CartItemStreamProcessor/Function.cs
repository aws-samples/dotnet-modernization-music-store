using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.DynamoDBv2;
using Amazon.Lambda.DynamoDBEvents;
using Microsoft.Extensions.Configuration;
using MvcMusicStore.Models;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace CartItemStreamProcessor
{
    public class Function
    {
        private readonly MusicStoreEntities storeDB;
        private const string RDS_ENDPOINT = "RDS_ENDPOINT";
        private const string RDS_CRED_SECRET_ARN = "RDS_CRED_SECRET_ARN";

        /// <summary>
        /// Default constructor that Lambda will invoke.
        /// </summary>
        public Function()
        {
            try
            {
                string connectionString = GetSecureDBConnectionString();
                Console.WriteLine("RDS DB connection string successfully created");

                storeDB = new MusicStoreEntities(connectionString);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        /// <summary>
        /// A Lambda function to respond to HTTP Get methods from API Gateway
        /// </summary>
        /// <param name="request"></param>
        /// <returns>The API Gateway response.</returns>
        public async Task FunctionHandler(DynamoDBEvent dynamoEvent, ILambdaContext context)
        {
            context.Logger.LogLine($"Beginning to process {dynamoEvent.Records.Count} records...");

            var batchIncrements = new Dictionary<(string CartId, Guid AlbumId), int>();

            // aggregate dynamodb events by cartId and albumId.
            foreach (var record in dynamoEvent.Records)
            {
                var cartId = RDSCartId(record.Dynamodb.Keys["PK"].S);
                var albumId = RDSAlbumId(record.Dynamodb.Keys["SK"].S);

                var batchKey = (CartId: cartId, AlbumId: albumId);

                if (!batchIncrements.ContainsKey(batchKey))
                {
                    batchIncrements[batchKey] = 0;
                }

                if (record.EventName == OperationType.INSERT
                    || record.EventName == OperationType.MODIFY)
                {
                    var quatity = record.Dynamodb.NewImage["Count"]?.N;
                    batchIncrements[batchKey] = int.Parse(quatity);
                }
                else if (record.EventName == OperationType.REMOVE)
                {
                    batchIncrements[batchKey] = 0;
                }
                else
                {
                    continue;
                }
            }

            List<Cart> cartItems = null;

            context.Logger.LogLine($"Aggregated dynamodb events to {batchIncrements.Count} records...");

            List<string> cartIds = batchIncrements.Keys.Select(k => k.CartId).ToList();
            List<Guid> albumdIds = batchIncrements.Keys.Select(k => k.AlbumId).ToList();

            try
            {
                // Fetch multiple cart items from SQL Server based on CartId and AlbumId
                cartItems = storeDB.Carts.Where(c => cartIds.Contains(c.CartId) && albumdIds.Contains(c.AlbumId)).ToList();

                context.Logger.LogLine($"Fetched CartItems from Sql Server");
            }
            catch (Exception e)
            {
                context.Logger.LogLine($"Failed to fetch cart items from Sql Server for cartIds: {string.Join(',', cartIds)}. Exception {e}");
            }

            foreach (var kvp in batchIncrements)
            {
                // check if cart items exist in SQL Server database.
                var cartItem = cartItems.FirstOrDefault(c => c.CartId == kvp.Key.CartId && c.AlbumId == kvp.Key.AlbumId);

                if (cartItem == null)
                {
                    // no modification to cart item.
                    if (kvp.Value == 0)
                        continue;

                    // Create a new cart item if no cart item exists
                    cartItem = new Cart
                    {
                        RecordId = Guid.NewGuid(),
                        AlbumId = kvp.Key.AlbumId,
                        CartId = kvp.Key.CartId,
                        Count = kvp.Value,
                        DateCreated = DateTime.Now
                    };
                    storeDB.Carts.Add(cartItem);

                    context.Logger.LogLine($"Added Cart Item {kvp.Key.CartId} from user {kvp.Key.AlbumId} with quantity {kvp.Value}");
                }
                else
                {
                    if (kvp.Value == 0)
                    {
                        storeDB.Carts.Remove(cartItem);
                        context.Logger.LogLine($"Removed Cart item {kvp.Key.CartId} from user {kvp.Key.AlbumId}");
                    }
                    else
                    {
                        cartItem.Count = kvp.Value;
                        context.Logger.LogLine($"Updated Cart Item {kvp.Key.CartId} from user {kvp.Key.AlbumId} by quantity {kvp.Value}");
                    }
                }
            }

            try
            {
                await storeDB.SaveChangesAsync();
            }
            catch (Exception e)
            {
                context.Logger.LogLine($"Failed to save changes in Sql Server. DynamoDb stream event count {dynamoEvent.Records.Count}. Exception {e}");
            }


            context.Logger.LogLine("Stream processing complete.");

        }
        private static string GetSecureDBConnectionString()
        {
            Console.WriteLine("Fetching RDS credentials from Secret Manager");
            var secretRequest = new GetSecretValueRequest { SecretId = Environment.GetEnvironmentVariable(RDS_CRED_SECRET_ARN) };
            var secretsManagerClient = new AmazonSecretsManagerClient();
            GetSecretValueResponse response = null;

            try
            {
                response = Task.Run(async () => await secretsManagerClient.GetSecretValueAsync(secretRequest)).Result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            var secretCredetials = JsonConvert.DeserializeObject<SecretCredentials>(response.SecretString);
            var serverAddress = Environment.GetEnvironmentVariable(RDS_ENDPOINT);

            return $"Server={serverAddress}; Database=MusicStore; User Id={secretCredetials.userName}; Password={secretCredetials.password}";
        }

        private static string RDSCartId(string cartId) => cartId.Replace("user#", "").Replace("cart#", "");

        private Guid RDSAlbumId(string albumId) => Guid.Parse(albumId.Replace("album#", ""));
    }

    public record SecretCredentials(string userName, string password);
}
