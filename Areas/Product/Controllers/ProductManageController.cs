using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using App.Models;
using App.Utilities;
using AppTest1.Areas.Product.Models;
using HocAspMVC4.Models;

using HocAspMVC4_Test.Models.Product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace AppTest1.Areas.Product.Controllers
{
    [Area("Product")]
    [Route("admin/productmanage/[action]/{id?}")]
    [Authorize(Roles ="Admin")]
    public class ProductManageController : Controller
    {
        private readonly AppDbContext1 _context;

        private readonly UserManager<AppUser> _userManager;


        public ProductManageController(AppDbContext1 context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        [TempData]
        public string StatusMessage { set; get; }

        // GET: blog/Post
        public async Task<IActionResult> Index(int pagesize, [FromQuery(Name ="p")]int currentPage)
        {
            var posts = _context.Products
                .Include(p => p.Author)
                .OrderByDescending(p => p.DateUpdated);

            int totalPost = await posts.CountAsync();
            if (pagesize <= 0)
            {
                pagesize = 10;
            }
            int countPages = (int)Math.Ceiling((double)totalPost / pagesize);

            
            if (currentPage > countPages)
                currentPage = countPages; //đưa về index cuối cùng 
            if (currentPage < 1)  //nếu index của paging < 1 ==> đưa về trang 1
                currentPage = 1;


            var pagingModel = new PagingModel()
            {
                countpages = countPages,
                currentpage = currentPage,
                generateUrl = (pageNumber) => Url.Action("Index", new
                {
                     p = pageNumber,
                     pagesize = pagesize
                })
            };

            ViewBag.pagingModel = pagingModel;
            ViewBag.totalPosts = totalPost;

            //dùng để hiện thị stt
            ViewBag.postIndex =  (currentPage - 1) * pagesize;

            var postsInPage = await posts.Skip((currentPage - 1) * pagesize)
                        .Take(pagesize)  
                        .Include(p => p.ProductCategoryProduct) 
                        .ThenInclude(pc => pc.Category).ToListAsync();

            return View(postsInPage);
        }

        // GET: blog/Post/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var post = await _context.Products
                .Include(p => p.Author)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // GET: blog/Post/Create
        public async Task<IActionResult> CreateAsync()
        {
            var categories = await _context.CategoryProduct.ToListAsync();
            ViewData["categories"] = new MultiSelectList(categories, "Id", "Title");
            return View();
        }

        // POST: blog/Post/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,Slug,Content,Published, CategoryIDs, Price")] CreateProductModel product)
        {
            var categories = await _context.CategoryProduct.ToListAsync();
            ViewData["categories"] = new MultiSelectList(categories, "Id", "Title");

            //dùng để tự động phát sinh slug từ title của bài viết khi user ko nhập slug
            if (product.Slug == null)
            {
                product.Slug = AppUtilities.GenerateSlug(product.Title);
            }

            if (await _context.Products.AnyAsync(p => p.Slug == product.Slug))
            {
                ModelState.AddModelError("Slug", "Nhập chuỗi url khác");
                return View(product);
            }


            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(this.User);  //  lấy user đang thực hiện hành động
                product.DateCreated = product.DateUpdated = DateTime.Now;  //   ngày tạo/ngày update bằng hiện tại
                product.AuthorId = user.Id; //tác giả = user đang đăng nhập

                _context.Add(product);

                //còn phải thêm id của danh mục nữa
                if (product.CategoryIDs != null)
                {
                    // CategoryIDs này chịu ảnh hưởng của thư viện multiple select rồi
                    foreach (var CateId in product.CategoryIDs)  //duyệt qua những id của danh mục mà user chọn
                    {
                        _context.Add(new ProductCategoryProduct()  //thêm những danh mục mà user đã chọn vào csdl
                        {
                            CategoryID = CateId,
                            Product = product
                        });
                    }
                }

                await _context.SaveChangesAsync();
                StatusMessage = "Vừa tạo bài viết mới bài viết: " + product.Title;
                return RedirectToAction(nameof(Index));
            }
            ViewData["AuthorId"] = new SelectList(_context.Users, "Id", "Id", product.AuthorId);
            return View(product);
        }

        // GET: blog/Post/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products.Include(p => p.ProductCategoryProduct).FirstOrDefaultAsync(p => p.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            var postEdit = new CreateProductModel()
            {
                ProductId = product.ProductId,
                Title = product.Title,
                Content = product.Content,
                Description = product.Description,
                Slug = product.Slug,
                Published = product.Published,
                CategoryIDs = product.ProductCategoryProduct.Select(pc => pc.CategoryID).ToArray(),
                Price = product.Price
            };

            var categories = await _context.CategoryProduct.ToListAsync();
            ViewData["categories"] = new MultiSelectList(categories, "Id", "Title");
            return View(postEdit);
        }

        // POST: blog/Post/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,Title,Description,Slug,Content,Published,CategoryIDs, Price")] CreateProductModel product)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            var categories = await _context.CategoryProduct.ToListAsync();
            ViewData["categories"] = new MultiSelectList(categories, "Id", "Title");

            //dùng để tự động phát sinh slug từ title của bài viết khi user ko nhập slug
            if (product.Slug == null)
            {
                product.Slug = AppUtilities.GenerateSlug(product.Title);
            }

            if (await _context.Products.AnyAsync(p => p.Slug == product.Slug && p.ProductId != id))
            {
                ModelState.AddModelError("Slug", "Nhập chuỗi url khác");
                return View(product);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var productUpdate = await _context.Products.Include(p => p.ProductCategoryProduct).FirstOrDefaultAsync(p => p.ProductId == id);

                    if (productUpdate == null)
                    {
                        return NotFound();
                    }

                    productUpdate.Title = product.Title;
                    productUpdate.Description = product.Description;
                    productUpdate.Content = product.Content;
                    productUpdate.Published = product.Published;
                    productUpdate.Slug = product.Slug;
                    productUpdate.DateUpdated = DateTime.Now;
                    productUpdate.Price = product.Price;
                    //cập nhật danh mục
                    if (product.CategoryIDs == null)
                    {
                        product.CategoryIDs = new int[] { };
                    }

                    var oldCateIds = productUpdate.ProductCategoryProduct.Select(c => c.CategoryID).ToArray();  //CateId cũa
                    var newCateIds = product.CategoryIDs;  //CateId binding đến

                    //có trong oldCateIds nhưng ko có trong newCateids
                    var removeCatePosts = from productCate in productUpdate.ProductCategoryProduct
                                          where (!newCateIds.Contains(productCate.CategoryID))
                                          select productCate;

                    _context.ProductCategoryProducts.RemoveRange(removeCatePosts);


                    var addCateIds = from CateId in newCateIds
                                     where !oldCateIds.Contains(CateId)
                                     select CateId;

                    foreach (var CateId in addCateIds)
                    {
                        _context.ProductCategoryProducts.Add(new ProductCategoryProduct()
                        {
                            ProductID = id,
                            CategoryID = CateId
                        });
                    }

                    _context.Update(productUpdate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(product.ProductId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                StatusMessage = "Vừa cập nhật bài viết: " + product.Title;
                return RedirectToAction(nameof(Index));
            }
            ViewData["AuthorId"] = new SelectList(_context.Users, "Id", "Id", product.AuthorId);
            return View(product);
        }

        // GET: blog/Post/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var post = await _context.Products
                .Include(p => p.Author)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // POST: blog/Post/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Products == null)
            {
                return Problem("Entity set 'AppDbContextTest.Posts'  is null.");
            }
            var post = await _context.Products.FindAsync(id);
            if (post != null)
            {
                _context.Products.Remove(post);
            }
            
            await _context.SaveChangesAsync();
            StatusMessage = "Bạn vừa xóa bài viết: " + post.Title;
            return RedirectToAction(nameof(Index));
        }

        private bool PostExists(int id)
        {
          return (_context.Products?.Any(e => e.ProductId == id)).GetValueOrDefault();
        }

        public class UploadOneFile
        {
            [Required(ErrorMessage ="Phải chọn một file")]
            [DataType(DataType.Upload)]
            [FileExtensions( Extensions = "png,jpg,jpeg,gif")]
            [Display(Name ="Chọn file upload")]
            public IFormFile FileUpload { set; get; }
        }

        [HttpGet]
        public IActionResult UpLoadPhoto(int id)
        {
            var product = _context.Products.Where(e => e.ProductId == id)
                                            .Include(p => p.Photos)
                                            .FirstOrDefault();
            if (product == null)
            {
                return NotFound("Không có sản phẩm");
            }

            ViewData["product"] = product;


            return View(new UploadOneFile());
        }

        [HttpPost, ActionName("UpLoadPhoto")]
        public async Task<IActionResult> UpLoadPhotoAsync(int id, [Bind("FileUpload")]UploadOneFile f)
        {
            var product = _context.Products.Where(e => e.ProductId == id)
                                            .Include(p => p.Photos)
                                            .FirstOrDefault();
            if (product == null)
            {
                return NotFound("Không có sản phẩm");
            }

            ViewData["product"] = product;

            if (f != null)
            {
                var file1 = Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) 
                   +Path.GetExtension(f.FileUpload.FileName);

                var file = Path.Combine("Uploads", "Products", file1);

                using (var filestream =  new FileStream(file, FileMode.Create))
                {
                    await f.FileUpload.CopyToAsync(filestream);
                }

                _context.Add(new ProductPhoto()
                {
                    ProductID = product.ProductId,
                    FileName = file1
                });

                await _context.SaveChangesAsync();
            }

            return View(f);
        }

        [HttpPost]
        public IActionResult ListPhotos(int id)
        {
            var product = _context.Products.Where(e => e.ProductId == id)
                                            .Include(p => p.Photos)
                                            .FirstOrDefault();
            if (product == null)
            {
                return Json(
                    new
                    {
                        success = 0,
                        message = "Product not found",
                    }
               );
            }


            var listphotos =  product.Photos.Select(photo => new
            {
                id = photo.Id,
                path = "/contents/Products/" + photo.FileName
            });

            return Json(
                new {
                    success = 1,
                    photos = listphotos,
                }
            );

        }

        [HttpPost]
        public IActionResult DeletePhoto(int id)
        {
            var photo = _context.ProductPhotos.Where(p => p.Id == id).FirstOrDefault();

            if (photo != null)
            {
                _context.Remove(photo);
                _context.SaveChanges();

                var filename = "/Uploads/Products/" + photo.FileName;
                System.IO.File.Delete(filename);
            }

            return Ok();
        }


        [HttpPost]
        public async Task<IActionResult> UpLoadPhotoApi(int id, [Bind("FileUpload")] UploadOneFile f)
        {
            var product = _context.Products.Where(e => e.ProductId == id)
                                            .Include(p => p.Photos)
                                            .FirstOrDefault();
            if (product == null)
            {
                return NotFound("Không có sản phẩm");
            }


            if (f != null)
            {
                var file1 = Path.GetFileNameWithoutExtension(Path.GetRandomFileName())
                   + Path.GetExtension(f.FileUpload.FileName);

                var file = Path.Combine("Uploads", "Products", file1);

                using (var filestream = new FileStream(file, FileMode.Create))
                {
                    await f.FileUpload.CopyToAsync(filestream);
                }

                _context.Add(new ProductPhoto()
                {
                    ProductID = product.ProductId,
                    FileName = file1
                });

                await _context.SaveChangesAsync();
            }

            return Ok();
        }

    }
}
