using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MvcMusicStore.Models
{
    public partial class ShoppingCart
    {
        readonly MusicStoreEntities storeDB = new MusicStoreEntities();
        readonly PostgresMusicStoreContext orderDb = new PostgresMusicStoreContext();

        string ShoppingCartId { get; set; }

        public const string CartSessionKey = "CartId";

        public static ShoppingCart GetCart(HttpContextBase context)
        {
            var cart = new ShoppingCart();
            cart.ShoppingCartId = cart.GetCartId(context);
            return cart;
        }

        // Helper method to simplify shopping cart calls
        public static ShoppingCart GetCart(Controller controller)
        {
            return GetCart(controller.HttpContext);
        }

        public void AddToCart(Album album)
        {
            // Get the matching cart and album instances
            var cartItem = storeDB.Carts.SingleOrDefault(
c => c.CartId == ShoppingCartId
&& c.AlbumId == album.AlbumId);

            if (cartItem == null)
            {
                // Create a new cart item if no cart item exists
                cartItem = new Cart
                {
                    RecordId = Guid.NewGuid(),
                    AlbumId = album.AlbumId,
                    CartId = ShoppingCartId,
                    Count = 1,
                    DateCreated = DateTime.Now
                };

                storeDB.Carts.Add(cartItem);
            }
            else
            {
                // If the item does exist in the cart, then add one to the quantity
                cartItem.Count++;
            }

            // Save changes
            storeDB.SaveChanges();
        }

        public int RemoveFromCart(Guid id)
        {
            // Get the cart
            var cartItem = storeDB.Carts.Single(
cart => cart.CartId == ShoppingCartId
&& cart.RecordId == id);

            int itemCount = 0;

            if (cartItem != null)
            {
                if (cartItem.Count > 1)
                {
                    cartItem.Count--;
                    itemCount = cartItem.Count;
                }
                else
                {
                    storeDB.Carts.Remove(cartItem);
                }

                // Save changes
                storeDB.SaveChanges();
            }

            return itemCount;
        }

        public void EmptyCart()
        {
            var cartItems = storeDB.Carts.Where(cart => cart.CartId == ShoppingCartId);

            foreach (var cartItem in cartItems)
            {
                storeDB.Carts.Remove(cartItem);
            }

            // Save changes
            storeDB.SaveChanges();
        }

        public List<Cart> GetCartItems()
        {
            return storeDB.Carts.Where(cart => cart.CartId == ShoppingCartId).ToList();
        }

        public int GetCount()
        {
            var lineItemsQ = from cartItems in storeDB.Carts
                             where cartItems.CartId == this.ShoppingCartId
                             select new { Key = cartItems.CartId, ItemCount = (int?)cartItems.Count };

            var q = from li in lineItemsQ
                    group li by li.Key into g
                    select g.Sum(i => (decimal)i.ItemCount); // Casting count to decimal is an undesireable but ultimately safe workaround.

            Debug.WriteLine($"Getting cart item count query:\n{q}");

            // Return 0 if all entries are null
            return (int)q.SingleOrDefault();
        }

        /// <summary>
        /// Removes trailing zeros from a decimal
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static decimal Normalize(decimal value)
        {
            return value / 1.000000000000000000000000000000000m;
        }

        public decimal GetTotal()
        {
            var lineItemsQ = from cartItems in storeDB.Carts
                             where cartItems.CartId == ShoppingCartId
                             select new { Key = cartItems.CartId, ItemTotal = cartItems.Count * cartItems.Album.Price };

            var q = from li in lineItemsQ
                    group li by li.Key into g
                    select g.Sum(i => i.ItemTotal);

            Debug.WriteLine($"Getting cart total amount query:\n{q}");
            return Normalize(q.SingleOrDefault()); // Trailing zeros could be an indication of something undesirable going on in decimal math, but hopefully not.
        }

        public Guid CreateOrder(Order order)
        {
            decimal orderTotal = 0;

            var cartItems = GetCartItems();

            // Iterate over the items in the cart, adding the order details for each
            foreach (var item in cartItems)
            {
                var orderDetail = new OrderDetail
                {
                    OrderDetailId = Guid.NewGuid(),
                    AlbumId = item.AlbumId,
                    OrderId = order.OrderId,
                    UnitPrice = item.Album.Price,
                    Quantity = item.Count
                };

                // Set the order total of the shopping cart
                orderTotal += (item.Count * item.Album.Price);

                orderDb.OrderDetails.Add(orderDetail);

            }

            // Set the order's total to the orderTotal count
            order.Total = orderTotal;

            // Save the order
            storeDB.SaveChanges();

            // Empty the shopping cart
            EmptyCart();

            // Return the OrderId as the confirmation number
            return order.OrderId;
        }

        // We're using HttpContextBase to allow access to cookies.
        public string GetCartId(HttpContextBase context)
        {
            if (context.Session[CartSessionKey] == null)
            {
                if (!string.IsNullOrWhiteSpace(context.User.Identity.Name))
                {
                    context.Session[CartSessionKey] = context.User.Identity.Name;
                }
                else
                {
                    // Generate a new random GUID using System.Guid class
                    Guid tempCartId = Guid.NewGuid();

                    // Send tempCartId back to client as a cookie
                    context.Session[CartSessionKey] = tempCartId.ToString();
                }
            }

            return context.Session[CartSessionKey].ToString();
        }

        // When a user has logged in, migrate their shopping cart to
        // be associated with their username
        public void MigrateCart(string userName)
        {
            var shoppingCart = storeDB.Carts.Where(c => c.CartId == ShoppingCartId);

            foreach (Cart item in shoppingCart)
            {
                item.CartId = userName;
            }
            storeDB.SaveChanges();
        }
    }
}