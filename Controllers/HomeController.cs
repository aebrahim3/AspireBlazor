using System.Diagnostics;
using CmsBackend.Models;
using Microsoft.AspNetCore.Mvc;

namespace CmsBackend.Controllers;

// Handles home page and error views.
public class HomeController : Controller
{
    // Display the home page.
    public IActionResult Index()
    {
        return View();
    }

    // Display 404 page for unmatched routes.
    public IActionResult NotFound404()
    {
        Response.StatusCode = 404;
        return View("NotFound");
    }

    // Display error page with request tracking information.
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(
            new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier }
        );
    }
}
