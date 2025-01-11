using Pinterest.Data;
using Pinterest.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Pinterest.Controllers
{
    [Authorize]
    public class BookmarksController : Controller
    {
        private readonly ApplicationDbContext db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        public BookmarksController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager
            )
        {
            db = context;

            _userManager = userManager;

            _roleManager = roleManager;
        }


        // Toti utilizatorii pot vedea Bookmark-urile existente in platforma
        // Fiecare utilizator vede bookmark-urile pe care le-a creat
        // Userii cu rolul de Admin pot sa vizualizeze toate bookmark-urile existente
        // HttpGet - implicit
        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Index()
        {
            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            SetAccessRights();

            var currentUserId = _userManager.GetUserId(User);

            List<Bookmark> bookmarks;

            if (User.IsInRole("Admin"))
            {
                bookmarks = db.Bookmarks
                              .Include("User")
                              .Where(b => b.UserId == currentUserId || b.IsPublic)
                              .ToList();
            }
            else
            {
                bookmarks = db.Bookmarks
                              .Include("User")
                              .Where(b => b.UserId == currentUserId)
                              .ToList();
            }

            ViewBag.Bookmarks = bookmarks;

            return View();
        }


        // Afisarea tuturor articolelor pe care utilizatorul le-a salvat in 
        // bookmark-ul sau 
        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Show(int id)
        {
            SetAccessRights();

            var currentUserId = _userManager.GetUserId(User);
            

            var bookmark = db.Bookmarks
                             .Include("ArticleBookmarks.Article.Category")
                             .Include("ArticleBookmarks.Article.User")
                             .Include("User")
                             .FirstOrDefault(b => b.Id == id);

            if (bookmark == null)
            {
                TempData["message"] = "Resursa cautata nu poate fi gasita";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Articles");
            }

            if (bookmark.UserId == currentUserId || User.IsInRole("Admin"))
            {
                // User is viewing their own bookmark or is an Admin
                return View(bookmark);
            }
            else if (bookmark.IsPublic)
            {
                // User is viewing someone else's public bookmark
                return View(bookmark);
            }
            else
            {
                TempData["message"] = "Nu aveti drepturi";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Articles");
            }
        }


        // Randarea formularului in care se completeaza datele unui bookmark
        // [HttpGet] - se executa implicit
        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult New()
        {
            return View();
        }

        // Adaugarea bookmark-ului in baza de date
        [HttpPost]
        [Authorize(Roles = "User,Editor,Admin")]
        public ActionResult New(Bookmark bm)
        {
            bm.UserId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                db.Bookmarks.Add(bm);
                db.SaveChanges();
                TempData["message"] = "Colectia a fost adaugata";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index");
            }

            else
            {
                return View(bm);
            }
        }


        // Conditiile de afisare a butoanelor de editare si stergere
        private void SetAccessRights()
        {
            ViewBag.AfisareButoane = false;

            var currentUserId = _userManager.GetUserId(User);
            var bookmarkUserId = db.Bookmarks.FirstOrDefault()?.UserId;

            if (User.IsInRole("Admin") || currentUserId == bookmarkUserId)
            {
                ViewBag.AfisareButoane = true;
            }

            ViewBag.EsteAdmin = User.IsInRole("Admin");
            ViewBag.UserCurent = currentUserId;
        }

        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Edit(int id)
        {
            var bookmark = db.Bookmarks.FirstOrDefault(b => b.Id == id);

            if (bookmark == null)
            {
                TempData["message"] = "Resursa cautata nu poate fi gasita";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

            var currentUserId = _userManager.GetUserId(User);

            if (bookmark.UserId == currentUserId || User.IsInRole("Admin"))
            {
                return View(bookmark);
            }
            else
            {
                TempData["message"] = "Nu aveti drepturi";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Edit(int id, Bookmark updatedBookmark)
        {
            var bookmark = db.Bookmarks.FirstOrDefault(b => b.Id == id);

            if (bookmark == null)
            {
                TempData["message"] = "Resursa cautata nu poate fi gasita";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

            var currentUserId = _userManager.GetUserId(User);

            if (bookmark.UserId == currentUserId || User.IsInRole("Admin"))
            {
                if (ModelState.IsValid)
                {
                    bookmark.Name = updatedBookmark.Name;
                    bookmark.IsPublic = updatedBookmark.IsPublic;
                    db.SaveChanges();

                    TempData["message"] = "Colectia a fost actualizata";
                    TempData["messageType"] = "alert-success";
                    return RedirectToAction("Index");
                }
                else
                {
                    return View(updatedBookmark);
                }
            }
            else
            {
                TempData["message"] = "Nu aveti drepturi";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }
        }

        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Delete(int id)
        {
            var bookmark = db.Bookmarks.FirstOrDefault(b => b.Id == id);

            if (bookmark == null)
            {
                TempData["message"] = "Resursa cautata nu poate fi gasita";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

            var currentUserId = _userManager.GetUserId(User);

            if (bookmark.UserId == currentUserId || User.IsInRole("Admin"))
            {
                return View(bookmark);
            }
            else
            {
                TempData["message"] = "Nu aveti drepturi";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult DeleteConfirmed(int id)
        {
            var bookmark = db.Bookmarks.FirstOrDefault(b => b.Id == id);

            if (bookmark == null)
            {
                TempData["message"] = "Resursa cautata nu poate fi gasita";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

            var currentUserId = _userManager.GetUserId(User);

            if (bookmark.UserId == currentUserId || User.IsInRole("Admin"))
            {
                db.Bookmarks.Remove(bookmark);
                db.SaveChanges();

                TempData["message"] = "Colectia a fost stearsa";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["message"] = "Nu aveti drepturi";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }
        }


    }
}
