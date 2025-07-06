using Microsoft.AspNetCore.Mvc;

namespace FIndex.Api.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}