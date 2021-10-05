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
                Console.WriteLine("getting connection string");
                string connectionString = GetSecureDBConnectionString();
                storeDB = new MusicStoreEntities(connectionString);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private string GetSecureDBConnectionString()
        {
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

        /// <summary>
        /// A Lambda function to respond to HTTP Get methods from API Gateway
        /// </summary>
        /// <param name="request"></param>
        /// <returns>The API Gateway response.</returns>
        public async Task FunctionHandler(DynamoDBEvent dynamoEvent, ILambdaContext context)
        {
            context.Logger.LogLine($"Beginning to process {dynamoEvent.Records.Count} records...");

            var batchIncrements = new Dictionary<(string CartId, string AlbumId), int>();
            foreach (var record in dynamoEvent.Records)
            {
                var cartId = record.Dynamodb.Keys["PK"].S;
                var albumId = record.Dynamodb.Keys["SK"].S;

                var batchKey = (CartId: cartId, AlbumId: albumId);

                if (!batchIncrements.ContainsKey(batchKey))
                {
                    batchIncrements[batchKey] = 0;
                }

                if (record.EventName == OperationType.INSERT)
                {
                    batchIncrements[batchKey]++;
                }
                else if (record.EventName == OperationType.REMOVE)
                {
                    batchIncrements[batchKey]--;
                }
                else
                {
                    continue;
                }
            }

            foreach (var kvp in batchIncrements)
            {
                try
                {
                    // It would be 0 if there was only a modification to a gallery item
                    if (kvp.Value == 0)
                        continue;

                    if (kvp.Value > 0)
                    {
                        await AddToCart(kvp.Key.CartId.Replace("user#", "").Replace("cart#",""), Guid.Parse(kvp.Key.AlbumId.Replace("album#", "")), kvp.Value);
                    }
                    else { 
                        await RemoveFromCart(kvp.Key.CartId.Replace("user#", "").Replace("cart#", ""), Guid.Parse(kvp.Key.AlbumId.Replace("album#", "")), kvp.Value);
                    }
                }
                catch (Exception e)
                {
                    context.Logger.LogLine($"Failed to update {kvp.Key.CartId} from user {kvp.Key.AlbumId} by increment {kvp.Value}: {e.Message}. Exception {e}");
                }
            }

            context.Logger.LogLine("Stream processing complete.");

        }

        public async Task AddToCart(string cartId, Guid albumId, int quantity)
        {
            // Get the matching cart and album instances
            var cartItem = storeDB.Carts.SingleOrDefault(
                c => c.CartId == cartId
                && c.AlbumId == albumId);

            if (cartItem == null)
            {
                // Create a new cart item if no cart item exists
                cartItem = new Cart
                {
                    AlbumId = albumId,
                    CartId = cartId,
                    Count = quantity,
                    DateCreated = DateTime.Now
                };

                storeDB.Carts.Add(cartItem);
            }
            else
            {
                // If the item does exist in the cart, then add quantity.
                cartItem.Count = cartItem.Count + quantity;
            }

            // Save changes
            await storeDB.SaveChangesAsync();
        }

        public async Task RemoveFromCart(string cartId, Guid albumId, int quantity)
        {
            // Get the cart
            var cartItem = storeDB.Carts.Single(
                cart => cart.CartId == cartId
                && cart.AlbumId == albumId);

            if (cartItem != null && quantity <= 0)
            {
                int itemCount = cartItem.Count + quantity;

                if (itemCount > 0)
                {
                    cartItem.Count = itemCount;
                }
                else
                {
                    storeDB.Carts.Remove(cartItem);
                }

                // Save changes
                await storeDB.SaveChangesAsync();
            }

        }
    }

    public record SecretCredentials(string userName, string password);
}
