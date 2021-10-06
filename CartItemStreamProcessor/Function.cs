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
                var quatity = record.Dynamodb.NewImage["Count"].N;

                var batchKey = (CartId: cartId, AlbumId: albumId);

                if (!batchIncrements.ContainsKey(batchKey))
                {
                    batchIncrements[batchKey] = 0;
                }

                if (record.EventName == OperationType.INSERT 
                    || record.EventName == OperationType.MODIFY)
                {
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

            // Fetch multiple cart items from SQL Server based on CartId and AlbumId
            var cartItems = storeDB.Carts.Where(c => batchIncrements.Keys.Any(k => k.CartId == c.CartId && k.AlbumId == c.AlbumId)).ToList();

            foreach (var kvp in batchIncrements)
            {
                try
                {
                    // check if cart items exist in SQL Server database.
                    var cartItem = cartItems.FirstOrDefault(c => c.CartId == kvp.Key.CartId && c.AlbumId == kvp.Key.AlbumId);

                    // It would be 0 if there was only a modification to a gallery item
                    if (cartItem != null)
                    {
                        if (kvp.Value == 0)
                            storeDB.Carts.Remove(cartItem);
                        else
                            cartItem.Count = kvp.Value;
                    }
                    else
                    {
                        if (kvp.Value > 0)
                        {
                            // Create a new cart item if no cart item exists
                            cartItem = new Cart
                            {
                                AlbumId = kvp.Key.AlbumId,
                                CartId = kvp.Key.CartId,
                                Count = kvp.Value,
                                DateCreated = DateTime.Now
                            };
                            storeDB.Carts.Add(cartItem);
                        }
                    }

                    await storeDB.SaveChangesAsync();

                }
                catch (Exception e)
                {
                    context.Logger.LogLine($"Failed to update {kvp.Key.CartId} from user {kvp.Key.AlbumId} by increment {kvp.Value}: {e.Message}. Exception {e}");
                }
            }

            context.Logger.LogLine("Stream processing complete.");

        }

        //private async Task AddOrUpdateCart(string cartId, Guid albumId, int quantity)
        //{
        //    // Get the matching cart and album instances
        //    var cartItem = storeDB.Carts.SingleOrDefault(
        //        c => c.CartId == cartId
        //        && c.AlbumId == albumId);

        //    if (cartItem == null)
        //    {
        //        // Create a new cart item if no cart item exists
        //        cartItem = new Cart
        //        {
        //            AlbumId = albumId,
        //            CartId = cartId,
        //            Count = quantity,
        //            DateCreated = DateTime.Now
        //        };

        //        storeDB.Carts.Add(cartItem);
        //    }
        //    else
        //    {
        //        // If the item does exist in the cart, then add quantity.
        //        cartItem.Count = cartItem.Count + quantity;
        //    }

        //    // Save changes
        //    await storeDB.SaveChangesAsync();
        //}

        //private async Task RemoveFromCart(string cartId, Guid albumId)
        //{
        //    storeDB.Carts.Where(c => c.CartId == cartId
        //        && c.AlbumId == albumId).Dele //.SqlQuery("delete from Cart where CartId={0} && albumdId={1}", cartId, albumId);

        //    // Save changes
        //    await storeDB.SaveChangesAsync();
        //}

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
