﻿using Pinterest.Data;
using Pinterest.Models;
using Ganss.Xss;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using static Pinterest.Models.ArticleBookmarks;

namespace Pinterest.Controllers
{
    
    [Authorize]
    public class ArticlesController : Controller
    {
        // PASUL 10: useri si roluri 

        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public ArticlesController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager
        )
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        

        // Se afiseaza lista tuturor articolelor impreuna cu categoria 
        // din care fac parte
        // Pentru fiecare articol se afiseaza si userul care a postat articolul respectiv
        // [HttpGet] care se executa implicit
        //[Authorize(Roles = "User,Editor,Admin")]
        //am comentat linia de cod de mai sus si am adaugat o pe cea de mai jos pentru a avea acces si ca utilizator nelogat sa vedem articolele
        [AllowAnonymous]
        public IActionResult Index(string sortOrder)
        {
            var articles = db.Articles.Include("Category")
                                      .Include("User");

            // Sortam articolele dupa ce criteriu vrem noi
            switch (sortOrder)
            {
                case "likes":
                    articles = articles.OrderByDescending(a => a.Likes.Count);
                    break;
                case "date":
                default:
                    articles = articles.OrderByDescending(a => a.Date);
                    break;
            }

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            var search = "";

            if (Convert.ToString(HttpContext.Request.Query["search"]) != null)
            {
                search = Convert.ToString(HttpContext.Request.Query["search"]).Trim();

                List<int> articleIds = db.Articles.Where
                                        (
                                         at => at.Title.Contains(search)
                                         || at.Content.Contains(search)
                                        ).Select(a => a.Id).ToList();

                List<int> articleIdsOfCommentsWithSearchString = db.Comments
                                        .Where
                                        (
                                         c => c.Content.Contains(search)
                                        ).Select(c => (int)c.ArticleId).ToList();

                List<int> mergedIds = articleIds.Union(articleIdsOfCommentsWithSearchString).ToList();

                articles = db.Articles.Where(article => mergedIds.Contains(article.Id))
                                      .Include("Category")
                                      .Include("User");

                switch (sortOrder)
                {
                    case "likes":
                        articles = articles.OrderByDescending(a => a.Likes.Count);
                        break;
                    case "date":
                    default:
                        articles = articles.OrderByDescending(a => a.Date);
                        break;
                }
            }

            ViewBag.SearchString = search;
            //aici s-a horat cate articole avem pe pagina
            int _perPage = 3;
            int totalItems = articles.Count();
            var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);
            var offset = 0;

            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * _perPage;
            }

            var paginatedArticles = articles.Skip(offset).Take(_perPage);

            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)_perPage);
            ViewBag.Articles = paginatedArticles;

            if (search != "")
            {
                ViewBag.PaginationBaseUrl = "/Articles/Index/?search=" + search + "&page";
            }
            else
            {
                ViewBag.PaginationBaseUrl = "/Articles/Index/?page";
            }

            ViewBag.SortOrder = sortOrder;

            return View();
        }

        // Se afiseaza un singur articol in functie de id-ul sau 
        // impreuna cu categoria din care face parte
        // In plus sunt preluate si toate comentariile asociate unui articol
        // Se afiseaza si userul care a postat articolul respectiv
        // [HttpGet] se executa implicit implicit
        //[Authorize(Roles = "User,Editor,Admin")]
        [AllowAnonymous]
        public IActionResult Show(int id)
        {
            Article article = db.Articles.Include("Category")
                                         .Include("Comments")
                                         .Include("User")
                                         .Include("Comments.User")
                                         .Include("Likes")
                              .Where(art => art.Id == id)
                              .First();

            // Adaugam bookmark-urile utilizatorului pentru dropdown
            ViewBag.UserBookmarks = db.Bookmarks
                                      .Where(b => b.UserId == _userManager.GetUserId(User))
                                      .ToList();

            SetAccessRights();

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            return View(article);
        }

        // Se afiseaza formularul in care se vor completa datele unui articol
        // impreuna cu selectarea categoriei din care face parte
        // HttpGet implicit


        // Adaugarea unui comentariu asociat unui articol in baza de date
        // Toate rolurile pot adauga comentarii in baza de date
        [HttpPost]
        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Show([FromForm] Comment comment)
        {
            comment.Date = DateTime.Now;

            // preluam Id-ul utilizatorului care posteaza comentariul
            comment.UserId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                db.Comments.Add(comment);
                db.SaveChanges();
                return Redirect("/Articles/Show/" + comment.ArticleId);
            }
            else 
            {
                Article art = db.Articles.Include("Category")
                                         .Include("User")
                                         .Include("Comments")
                                         .Include("Comments.User")
                                         .Where(art => art.Id == comment.ArticleId)
                                         .First();

                //return Redirect("/Articles/Show/" + comm.ArticleId);

                // Adaugam bookmark-urile utilizatorului pentru dropdown
                ViewBag.UserBookmarks = db.Bookmarks
                                          .Where(b => b.UserId == _userManager.GetUserId(User))
                                          .ToList();

                SetAccessRights();

                return View(art);
            }
        }

        [HttpPost]
        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult AddBookmark([FromForm] ArticleBookmark articleBookmark)
        {
            // Daca modelul este valid
            if (ModelState.IsValid)
            {
                // Verificam daca avem deja articolul in colectie
                if (db.ArticleBookmarks
                    .Where(ab => ab.ArticleId == articleBookmark.ArticleId)
                    .Where(ab => ab.BookmarkId == articleBookmark.BookmarkId)
                    .Count() > 0)
                {
                    TempData["message"] = "Acest articol este deja adaugat in colectie";
                    TempData["messageType"] = "alert-danger";
                }
                else
                {
                    // Adaugam asocierea intre articol si bookmark 
                    db.ArticleBookmarks.Add(articleBookmark);
                    // Salvam modificarile
                    db.SaveChanges();

                    // Adaugam un mesaj de succes
                    TempData["message"] = "Articolul a fost adaugat in colectia selectata";
                    TempData["messageType"] = "alert-success";
                }

            }
            else
            {
                TempData["message"] = "Nu s-a putut adauga articolul in colectie";
                TempData["messageType"] = "alert-danger";
            }

            // Ne intoarcem la pagina articolului
            return Redirect("/Articles/Show/" + articleBookmark.ArticleId);
        }


        // Se afiseaza formularul in care se vor completa datele unui articol
        // impreuna cu selectarea categoriei din care face parte
        // Doar utilizatorii cu rolul de Editor si Admin pot adauga articole in platforma
        // [HttpGet] - care se executa implicit


        //am modificat pentru ca orice utilizator inregistrat sa poata adauga articole
        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult New()
        {
            Article article = new Article();

            article.Categ = GetAllCategories();

            return View(article);
        }

        // Se adauga articolul in baza de date
        // Doar utilizatorii cu rolul Editor si Admin pot adauga articole in platforma
        [HttpPost]
        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult New(Article article)
        {
            var sanitizer = new HtmlSanitizer();

            article.Date = DateTime.Now;

            // preluam Id-ul utilizatorului care posteaza articolul
            article.UserId = _userManager.GetUserId(User);

            if(ModelState.IsValid)
            {
                article.Content = sanitizer.Sanitize(article.Content);

                db.Articles.Add(article);
                db.SaveChanges();
                TempData["message"] = "Articolul a fost adaugat";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index");
            }
            else
            {
                article.Categ = GetAllCategories();
                return View(article);
            }
        }

        // Se editeaza un articol existent in baza de date impreuna cu categoria din care face parte
        // Categoria se selecteaza dintr-un dropdown
        // Se afiseaza formularul impreuna cu datele aferente articolului din baza de date
        // Doar utilizatorii cu rolul de Editor si Admin pot edita articole
        // Adminii pot edita orice articol din baza de date
        // Editorii pot edita doar articolele proprii (cele pe care ei le-au postat)
        // [HttpGet] - se executa implicit

        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Edit(int id)
        {
            Article article = db.Articles.Include("Category")
                                         .Where(art => art.Id == id)
                                         .First();

            article.Categ = GetAllCategories();

            if ((article.UserId == _userManager.GetUserId(User)) ||
                User.IsInRole("Admin"))
            {
                return View(article);
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui articol care nu va apartine";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }
        }

        // Se adauga articolul modificat in baza de date
        // Se verifica rolul utilizatorilor care au dreptul sa editeze (Editor si Admin)
        [HttpPost]
        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Edit(int id, Article requestArticle)
        {
            var sanitizer = new HtmlSanitizer();

            Article article = db.Articles.Find(id);

            if (ModelState.IsValid)
            {
                if ((article.UserId == _userManager.GetUserId(User))
                    || User.IsInRole("Admin"))
                {
                    article.Title = requestArticle.Title;

                    requestArticle.Content = sanitizer.Sanitize(requestArticle.Content);

                    article.Content = requestArticle.Content;

                    article.Date = DateTime.Now;
                    article.CategoryId = requestArticle.CategoryId;
                    article.UserId = requestArticle.UserId; // Update the author of the article
                    TempData["message"] = "Articolul a fost modificat";
                    TempData["messageType"] = "alert-success";
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui articol care nu va apartine";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("Index");
                }
            }
            else
            {
                requestArticle.Categ = GetAllCategories();
                return View(requestArticle);
            }
        }

        // Se sterge un articol din baza de date 
        // Utilizatorii cu rolul de Editor sau Admin pot sterge articole
        // Editorii pot sterge doar articolele publicate de ei
        // Adminii pot sterge orice articol de baza de date

        [HttpPost]
        [Authorize(Roles = "Editor,Admin")]
        public ActionResult Delete(int id)
        {
            // Article article = db.Articles.Find(id);

            Article article = db.Articles.Include("Comments")
                                         .Where(art => art.Id == id)
                                         .First();

            if ((article.UserId == _userManager.GetUserId(User))
                    || User.IsInRole("Admin"))
            {
                db.Articles.Remove(article);
                db.SaveChanges();
                TempData["message"] = "Articolul a fost sters";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["message"] = "Nu aveti dreptul sa stergeti un articol care nu va apartine";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }    
        }

        // Conditiile de afisare pentru butoanele de editare si stergere
        // butoanele aflate in view-uri
        private void SetAccessRights()
        {
            ViewBag.AfisareButoane = false;

            if (User.IsInRole("Editor"))
            {
                ViewBag.AfisareButoane = true;
            }

            ViewBag.UserCurent = _userManager.GetUserId(User);

            ViewBag.EsteAdmin = User.IsInRole("Admin");
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllCategories()
        {
            // generam o lista de tipul SelectListItem fara elemente
            var selectList = new List<SelectListItem>();

            // extragem toate categoriile din baza de date
            var categories = from cat in db.Categories
                             select cat;

            // iteram prin categorii
            foreach (var category in categories)
            {
                // adaugam in lista elementele necesare pentru dropdown
                // id-ul categoriei si denumirea acesteia
                selectList.Add(new SelectListItem
                {
                    Value = category.Id.ToString(),
                    Text = category.CategoryName
                });
            }
            /* Sau se poate implementa astfel: 
             * 
            foreach (var category in categories)
            {
                var listItem = new SelectListItem();
                listItem.Value = category.Id.ToString();
                listItem.Text = category.CategoryName;

                selectList.Add(listItem);
             }*/


            // returnam lista de categorii
            return selectList;
        }

        [HttpPost]
        [Authorize(Roles = "User,Editor,Admin")]
        public IActionResult Like(int articleId)
        {
            var userId = _userManager.GetUserId(User);

            // Check if the user has already liked the article
            var existingLike = db.Likes.FirstOrDefault(l => l.ArticleId == articleId && l.UserId == userId);

            if (existingLike == null)
            {
                var like = new Like
                {
                    ArticleId = articleId,
                    UserId = userId
                };

                db.Likes.Add(like);
                db.SaveChanges();

                TempData["message"] = "You liked the article!";
                TempData["messageType"] = "alert-success";
            }
            else
            {
                TempData["message"] = "You have already liked this article.";
                TempData["messageType"] = "alert-warning";
            }

            return RedirectToAction("Show", new { id = articleId });
        }


        // Metoda utilizata pentru exemplificarea Layout-ului
        // Am adaugat un nou Layout in Views -> Shared -> numit _LayoutNou.cshtml
        // Aceasta metoda are un View asociat care utilizeaza noul layout creat
        // in locul celui default generat de framework numit _Layout.cshtml
        public IActionResult IndexNou()
        {
            return View();
        }
    }
}