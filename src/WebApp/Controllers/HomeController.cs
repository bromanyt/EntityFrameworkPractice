using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebRetail.ViewModels;


namespace WebRetail.Controllers;

public class HomeController : Controller
{
    private readonly Serilog.ILogger _logger;

    public HomeController(Serilog.ILogger logger) => _logger = logger;

    public IActionResult Index() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View(new ErrorViewModel
    { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

}

