using System.Diagnostics;
using Firmeza.Application.Interfaces;
using Firmeza.Domain.Constants;
using Firmeza.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Firmeza.Web.Controllers;

public class HomeController(IDashboardService dashboardService) : Controller
{
    [AllowAnonymous]
    public IActionResult Index()
    {
        return View();
    }

    [Authorize(Roles = Roles.Administrador)]
    public async Task<IActionResult> Dashboard()
    {
        var metrics = await dashboardService.GetDashboardMetricsAsync();
        return View(metrics);
    }

    [AllowAnonymous]
    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}