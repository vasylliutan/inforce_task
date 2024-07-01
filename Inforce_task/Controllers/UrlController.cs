using Inforce_task.Models;
using Inforce_task.ViewModels.Inforce_task.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Inforce_task.Controllers
{


    [Authorize]
    [Route("api/url")]
    public class UrlController : Controller
    {
        private readonly DB_Context _context;
        private readonly UserManager<User> _userManager;

        public UrlController(DB_Context context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet("list")]
        public IActionResult Index()
        {
            var urls = _context.Urls.ToList();
            return View(urls);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(UrlViewModel model)
        {
            var baseUrl = "https://liutanvasyl.com/";
            var new_shortUrl = GenerateShortUrl();

            // Check if the short URL is already in use
            while (await _context.Urls.AnyAsync(u => u.shortUrl == new_shortUrl))
            {
                new_shortUrl = GenerateShortUrl();
            }

                if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                var url = new Url
                {
                    originalUrl = model.OriginalUrl,
                    shortUrl = baseUrl + new_shortUrl,
                    createdBy = user.Id,
                    createdDate = DateTime.Now
                };
                _context.Urls.Add(url);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        private string GenerateShortUrl()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        [HttpGet("item")]
        public IActionResult Details(int id)
        {
            var url = _context.Urls.FirstOrDefault(u => u.id == id);
            if (url == null)
            {
                return NotFound();
            }
            return View(url);
        }

        [HttpPost("delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var url = _context.Urls.FirstOrDefault(u => u.id == id);
            if (url == null)
            {
                return NotFound();
            }
            if (User.IsInRole("Admin") || url.createdBy == _userManager.GetUserId(User))
            {
                _context.Urls.Remove(url);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return Forbid();
        }
    }

}
