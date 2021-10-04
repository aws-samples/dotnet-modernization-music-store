using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MvcMusicStore.Models
{
    public partial class ShoppingCart
    {
        private AmazonDynamoDBClient dynamoClient;
        private DynamoDBContext context;

        string ShoppingCartId { get; set; }

        private const string CART_SESSION_KEY = "CartId";
        private const string TABLE_CART = "Cart";
        private const string COUNT_ATTRIBUTE = "Count";
        private const string ALBUM_ATTRIBUTE = "Album";

        MusicStoreEntities storeDB = new MusicStoreEntities();

        public static ShoppingCart GetCart(HttpContextBase context)
        {
            var cart = new ShoppingCart();
            cart.ShoppingCartId = cart.GetCartId(context);

            // set DynamoDB Context.
            cart.dynamoClient = new AmazonDynamoDBClient();

            cart.context = new DynamoDBContext(cart.dynamoClient);
            return cart;
        }

        // Helper method to simplify shopping cart calls
        public static ShoppingCart GetCart(Controller controller)
        {
            return GetCart(controller.HttpContext);
        }

        /// <summary>
        /// use updateItem to implement an atomic counter
        /// </summary>
        /// <param name="album"></param>
        /// <returns></returns>
        public async Task AddToCart(Album album)
        {
            await AddToCart(this.ShoppingCartId, album);
        }

        /// <summary>
        /// Remove Album from cart or update the quanity.
        /// </summary>
        /// <returns></returns>
        public async Task<int> RemoveFromCart(Guid albumId, int quantity)
        {
            // if quantity is greater 0, then update quantity in cart with passed quantity.
            if (quantity > 0)
            {
                await UpdateCartCount(this.ShoppingCartId, albumId.ToString(), quantity);
            }
            else
            {
                // remove item from cart.
                await context.DeleteAsync<Cart>(this.ShoppingCartId, rangeKey: $"album#{albumId}");
            }

            return quantity;
        }

        public void EmptyCart(List<Cart> cartItems)
        {
            // remove multiple items from cart.
            var cartBatch = context.CreateBatchWrite<Cart>();
            
            cartBatch.AddDeleteItems(cartItems);

            cartBatch.Execute();
        }

        public List<Cart> GetCartItems()
        {
            var cartItems = context.Query<Cart>(this.ShoppingCartId, Amazon.DynamoDBv2.DocumentModel.QueryOperator.BeginsWith, "album");

            return cartItems.ToList();
        }

        public int GetCount()
        {
            // Get the count of each item in the cart and sum them up
            int? count = GetCartItems().Sum(i => i.Count);

            // Return 0 if all entries are null
            return count ?? 0;
        }

        public decimal GetTotal()
        {
            // Multiply album price by count of that album to get 
            // the current price for each of those albums in the cart
            // sum all album price totals to get the cart total
            decimal? total = GetCartItems().Sum(cartItem => cartItem.Count * cartItem.Album.Price);
            
            return total ?? decimal.Zero;
        }

        public async Task<Guid> CreateOrder(Order order)
        {
            decimal orderTotal = 0;

            var cartItems = GetCartItems();

            // Iterate over the items in the cart, adding the order details for each
            foreach (var item in cartItems)
            {
                var orderDetail = new OrderDetail
                {
                    OrderDetailId = Guid.NewGuid(),
                    AlbumId = item.Album.AlbumId,
                    OrderId = order.OrderId,
                    UnitPrice = item.Album.Price,
                    Quantity = item.Count
                };

                // Set the order total of the shopping cart
                orderTotal += (item.Count * item.Album.Price);

                storeDB.OrderDetails.Add(orderDetail);

            }

            // Set the order's total to the orderTotal count
            order.Total = orderTotal;

            // Save the order
            storeDB.SaveChanges();

            // Empty the shopping cart
            EmptyCart(cartItems);

            // Return the OrderId as the confirmation number
            return order.OrderId;
        }

        // We're using HttpContextBase to allow access to cookies.
        public string GetCartId(HttpContextBase context)
        {
            if (context.Session[CART_SESSION_KEY] == null)
            {
                if (!string.IsNullOrWhiteSpace(context.User.Identity.Name))
                {
                    SetCartId(context, context.User.Identity.Name);
                }
                else
                {
                    SetUnAuthenticatedCartId(context);
                }
            }

            return context.Session[CART_SESSION_KEY].ToString();
        }

        public void SetCartId(HttpContextBase context, string userName)
        {
            context.Session[CART_SESSION_KEY] = $"user#{userName}";
        }

        public void SetUnAuthenticatedCartId(HttpContextBase context)
        {
            // Generate a new random GUID using System.Guid class
            Guid tempCartId = Guid.NewGuid();

            // Send tempCartId back to client as a cookie
            context.Session[CART_SESSION_KEY] = $"cart#{tempCartId.ToString()}";
        }


        // When user logs in, migrate their shopping cart to their username.
        public async Task MigrateCart(string userName)
        {
            List<Cart> cartItems = GetCartItems();

            if (! cartItems.Any() )
            {
                return;
            }

            foreach (Cart cart in cartItems)
            {
                //if item already exist in Cart, then update the quantity, else add new item.
                await AddToCart( $"user#{userName}", cart.Album, cart.Count);
            }

            // delete the items that were associated with temp id.
            EmptyCart(cartItems);
        }

        private async Task<int> UpdateCartCount(string cartId, string albumId, int count)
        {
            var request = new UpdateItemRequest
            {
                TableName = TABLE_CART,
                Key = new Dictionary<string, AttributeValue>
                        {
                            {"PK", new AttributeValue{S = cartId } },
                            {"SK", new AttributeValue{S = $"album#{albumId}" } }
                        },
                UpdateExpression = "SET #count = :count",

                ExpressionAttributeNames = new Dictionary<string, string>
                        {
                            { "#count", COUNT_ATTRIBUTE}
                        },
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                        {
                            {":count", new AttributeValue{N = count.ToString()}},
                        },
                ReturnValues = ReturnValue.UPDATED_NEW
            };

            var response = await dynamoClient.UpdateItemAsync(request);

            return count;
        }


        /// <summary>
        /// If item exist in cart then update the quantity by provided value,
        /// or add new item to cart.
        /// </summary>
        /// <param name="cartId"></param>
        /// <param name="album"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        private async Task AddToCart(string cartId, Album album, int? quantity = null)
        {
            var albumDocument = new Document();
            albumDocument["AlbumId"] = album.AlbumId;
            albumDocument["Title"] = album.Title;
            albumDocument["Price"] = album.Price;
            albumDocument["AlbumArtUrl"] = album.AlbumArtUrl;

            quantity = quantity ?? 1;

            var request = new UpdateItemRequest
            {
                TableName = TABLE_CART,
                Key = new Dictionary<string, AttributeValue>
                        {
                            {"PK", new AttributeValue{S = cartId } },
                            {"SK", new AttributeValue{S = $"album#{album.AlbumId}" } }
                        },
                UpdateExpression = "ADD #count :increment SET #album = :album",

                ExpressionAttributeNames = new Dictionary<string, string>
                        {
                            { "#count", COUNT_ATTRIBUTE},
                            { "#album", ALBUM_ATTRIBUTE },
                        },
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                        {
                            {":increment", new AttributeValue{N = quantity.ToString()}},
                            {":album", new AttributeValue{M = albumDocument.ToAttributeMap()}}
                        },
                ReturnValues = ReturnValue.UPDATED_NEW
            };

            await dynamoClient.UpdateItemAsync(request);
        }
    }
}