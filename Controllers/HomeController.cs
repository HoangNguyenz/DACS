using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using HocAspMVC4.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;

namespace HocAspMVC4.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    private readonly AppDbContext1 _context;


    public HomeController(ILogger<HomeController> logger, AppDbContext1 context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index(string SearchText = "")
    {
        
        if (SearchText != "" && SearchText != null)
        {
            var postSearch = _context.Posts
                .Include(p => p.Author)
                .Include(p => p.Photos)
                .Include(p => p.PostCategories)
                .ThenInclude(p => p.Category)
                .OrderByDescending(p => p.DateUpdated)
                .Where(p => p.Title.Contains(SearchText))
                .AsQueryable();
            ViewBag.postSearch = postSearch;
        }
        else
        {
            //post mới nhất
            var poststake1 = _context.Posts
                   .Include(p => p.Author)
                   .Include(p => p.Photos)
                   .Include(p => p.PostCategories)
                   .ThenInclude(p => p.Category)
                   .Where(p => p.Published)
                   .OrderByDescending(p => p.DateUpdated)
                   .AsQueryable();
            var posttake1first = poststake1.Take(1);
            ViewBag.posttake1first = posttake1first;

            //post trừ post mới nhất
            var posts = poststake1.Skip(1).Take(8).AsQueryable();
            ViewBag.posts = posts;

            //post co luot xem nhieu 
            var postLuotXemNhieu = _context.Posts
                   .Include(p => p.Author)
                   .Include(p => p.Photos)
                   .Include(p => p.PostCategories)
                   .ThenInclude(p => p.Category)
                   .OrderByDescending(p => p.DateUpdated)
                   .Skip(9)
                   .OrderByDescending(p => p.ViewCount)
                   .Take(4)
                   .AsQueryable();
            ViewBag.postLuotXemNhieu = postLuotXemNhieu;

            //post phim
            var postPhim = _context.Posts
                  .Include(p => p.Author)
                  .Include(p => p.Photos)
                  .Include(p => p.PostCategories)
                  .ThenInclude(p => p.Category)
                  .OrderByDescending(p => p.DateUpdated)
                  .Where(p => p.PostCategories.Any(pc => pc.Category.Title == "Điện Ảnh"))
                  .AsQueryable();
            var postPhimTake = postPhim.Take(4);
            ViewBag.postPhimTake = postPhimTake;

            //post sách
            var postSach = _context.Posts
                  .Include(p => p.Author)
                  .Include(p => p.Photos)
                  .Include(p => p.PostCategories)
                  .ThenInclude(p => p.Category)
                  .OrderByDescending(p => p.DateUpdated)
                  .Where(p => p.PostCategories.Any(pc => pc.Category.Title == "Sách"))
                  .AsQueryable();
            var postSachTake = postSach.Take(4);
            ViewBag.postSachTake = postSachTake;
        }
        return View();
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

