using Lazy.Presentation.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Lazy.Presentation.Models.Post;
using Lazy.Services;
using Lazy.Services.Post;
using Mapster;

namespace Lazy.Presentation.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IPostService _postService;


    public HomeController(ILogger<HomeController> logger, IPostService postService)
    {
        _logger = logger;
        _postService = postService;
    }

    public async Task<IActionResult> Index(int pageNumber = 0)
    {
        var posts = await _postService.GetPostList(pageNumber);
        var postsModel = posts.Adapt<IList<PostItemModel>>();

        return View(postsModel);
    }

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