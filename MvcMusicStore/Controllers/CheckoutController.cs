using MvcMusicStore.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace MvcMusicStore.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        readonly PostgresMusicStoreContext orderDb = new PostgresMusicStoreContext();

        const string PromoCode = "FREE";

        //
        // GET: /Checkout/AddressAndPayment

        public ActionResult AddressAndPayment()
        {
            return View();
        }

        //
        // POST: /Checkout/AddressAndPayment

        [HttpPost]
        public ActionResult AddressAndPayment(FormCollection values)
        {
            var order = new Order() { OrderId = Guid.NewGuid() };
            TryUpdateModel(order);

            if (string.Equals(values["PromoCode"], PromoCode,
                StringComparison.OrdinalIgnoreCase) == false)
            {
                return View(order);
            }

            order.Username = User.Identity.Name;
            order.OrderDate = DateTime.Now;

            //Save Order
            orderDb.Orders.Add(order);
            orderDb.SaveChanges();

            //Process the order
            var cart = ShoppingCart.GetCart(this.HttpContext);
            cart.CreateOrder(order);

            return RedirectToAction("Complete",
                new { id = order.OrderId });
        }

        //
        // GET: /Checkout/Complete

        public ActionResult Complete(Guid id)
        {
            // Validate customer owns this order
            bool isValid = orderDb.Orders.Any(
                o => o.OrderId == id &&
                o.Username == User.Identity.Name);

            if (isValid)
            {
                return View(id);
            }
            else
            {
                return View("Error");
            }
        }
    }
}
