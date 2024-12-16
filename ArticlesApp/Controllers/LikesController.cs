using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pinterest.Data;
using Pinterest.Models;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Pinterest.Controllers
{
    public class LikesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LikesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Like(int articleId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            var article = await _context.Articles.Include(a => a.Likes).FirstOrDefaultAsync(a => a.Id == articleId);
            if (article == null)
            {
                return NotFound();
            }

            var existingLike = article.Likes.FirstOrDefault(l => l.UserId == userId);
            if (existingLike != null)
            {
                _context.Likes.Remove(existingLike);
            }
            else
            {
                var like = new Like
                {
                    ArticleId = articleId,
                    UserId = userId
                };
                _context.Likes.Add(like);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Show", "Articles", new { id = articleId });
        }
    }
}
