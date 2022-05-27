using MvcMusicStore.Models;
using MvcMusicStore.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MvcMusicStore.Controllers
{
    public class SearchController : Controller
    {
        // GET: Search
        public ActionResult Index() => View((SearchResultsViewModel)null);

        public async Task<ActionResult> Lookup(SearchCriteriaModel searchCriteria)
        {
            this.TempData["SearchCriteria"] = searchCriteria;

            SearchResultsViewModel searchResults;
            if (this.ModelState.IsValid)
                searchResults = await searchCriteria.Search();
            else
            {   // Error in the input
                IEnumerable<string> errorMessages = ModelState.Values.SelectMany(x => x.Errors).Select(e => e.ErrorMessage);
                searchCriteria.ErrorMessage = string.Join("<br/>", errorMessages);

                if (SameReferrerControllerAndAction(this.Request, "Search", "Lookup"))
                    // Bad search term entered while staying in this route, show empty results
                    searchResults = new SearchResultsViewModel { Artists = new List<Artist>(), Albums = new List<Album>(), Genres = new List<Genre>() };
                else
                    // Bad search term entered from another controller/action - stay there
                    return Redirect(this.Request.UrlReferrer.ToString()); // Stay with the original controller
            }

            return View("Index", searchResults);
        }

        public bool SameReferrerControllerAndAction(HttpRequestBase request, string currentController, string currentAction)
        {
            var controller = (Request.UrlReferrer.Segments.Skip(1).Take(1).SingleOrDefault() ?? "Home").Trim('/');
            var action = (Request.UrlReferrer.Segments.Skip(2).Take(1).SingleOrDefault() ?? "Index").Trim('/');

            return controller == currentController && action == currentAction;
        }
    }
}