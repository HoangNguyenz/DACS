using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using App.Models;
using App.Utilities;
using HocAspMVC4.Models;
using HocAspMVC4_Test.Models.Blog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;


namespace AppTest1.Areas.blog.Controllers
{
    [Area("blog")]
    [Route("Post/[action]/{id?}")]
    //[Authorize(Roles = "Admin")]
    [Authorize(Policy = "AllowEditRole")]
    public class PostController : Controller
    {
        private readonly AppDbContext1 _context;

        private readonly UserManager<AppUser> _userManager;


        public PostController(AppDbContext1 context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager; 
        }
        [TempData]
        public string StatusMessage { set; get; }

        //hoang
        // GET: blog/Post
        public async Task<IActionResult> Index(int pagesize, [FromQuery(Name ="p")]int currentPage)
        {

            var posts = _context.Posts
                .Include(p => p.Author)
                .OrderByDescending(p => p.DateUpdated);

            //tổng các bài post
            int totalPost = await posts.CountAsync();

            if (pagesize <= 0)
            {
                pagesize = 10; 
            }

            //phân ra bao nhiêu trang          //vd: 150/10 => 15 trang
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

            //dùng để hiện thị stt của trang
            ViewBag.postIndex =  (currentPage - 1) * pagesize;

            //trang 2 thì lấy (2-1) * 10  ==> skip đi 10 bài
            var postsInPage = await posts.Skip((currentPage - 1) * pagesize)
                        .Take(pagesize)   
                        .Include(p => p.PostCategories) 
                        .ThenInclude(pc => pc.Category).ToListAsync();

            return View(postsInPage);
        }

        // GET: blog/Post/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Posts == null)
            {
                return NotFound();
            }


            var postDetail = await _context.Posts
                .Include(p => p.Author)
                .FirstOrDefaultAsync(m => m.PostId == id);


            if (postDetail == null)
            {
                return NotFound();
            }

            return View(postDetail);
        }



        //Hai
        // GET: blog/Post/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Posts == null)
            {
                return NotFound();
            }

            var postDelete = await _context.Posts
                .Include(p => p.Author)
                .FirstOrDefaultAsync(m => m.PostId == id);
            if (postDelete == null)
            {
                return NotFound();
            }

            return View(postDelete);
        }

        // POST: blog/Post/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {

            if (_context.Posts == null)
            {
                return Problem("Entity set 'AppDbContextTest.Posts'  is null.");
            }

            var postDelete = await _context.Posts
                .Include(p => p.Photos)
                .FirstOrDefaultAsync(m => m.PostId == id);

            if (postDelete == null)
            {
                return NotFound();
            }

            //xem bài post có hình ảnh hay không
            if (postDelete.Photos.Count > 0)
            {
                _context.PostPhotos.RemoveRange(postDelete.Photos);
            }

            _context.Posts.Remove(postDelete);
            await _context.SaveChangesAsync();
            StatusMessage = "Bạn vừa xóa bài viết: " + postDelete.Title;
            return RedirectToAction(nameof(Index));

        }



        // GET: blog/Post/Create
        [AllowAnonymous]
        public async Task<IActionResult> CreateAsync()
        {
            var categories = await _context.Categories.ToListAsync();
            //tạo một MultiSelectList từ categories, Id làm giá trị, title làm tiêu đề hiển thị 
            ViewData["categories"] = new MultiSelectList(categories, "Id", "Title");
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,Slug,Content,Published, CategoryIDs")] CreatePostModel postCreate)
        {
            var categories = await _context.Categories.ToListAsync();
            ViewData["categories"] = new MultiSelectList(categories, "Id", "Title");

            //dùng để tự động phát sinh slug từ title của bài viết khi user ko nhập slug
            if (postCreate.Slug == null)
            {
                postCreate.Slug = AppUtilities.GenerateSlug(postCreate.Title);
            }

            //kiểm tra xem slug có trùng với bài post nào ko
            if (await _context.Posts.AnyAsync(p => p.Slug == postCreate.Slug))
            {
                ModelState.AddModelError("Slug", "Nhập chuỗi url khác");
                return View(postCreate);
            }


            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(this.User);  //  lấy user đang thực hiện hành động
                postCreate.DateCreated = postCreate.DateUpdated = DateTime.Now;  //   ngày tạo/ngày update bằng hiện tại
                postCreate.AuthorId = user.Id; //tác giả = user đang đăng nhập
                if (Request.Form["Published"] == "on")  // Kiểm tra xem checkbox đã được chọn hay không
                {
                    postCreate.Published = true;  // Gán giá trị "true" cho trường "Published"
                }
                _context.Add(postCreate);

                //còn phải thêm id của danh mục 
                if (postCreate.CategoryIDs != null)
                {
                    foreach (var CateId in postCreate.CategoryIDs)  //duyệt qua những id của danh mục mà user chọn
                    {
                        _context.Add(new PostCategory()  //thêm những danh mục mà user đã chọn vào csdl
                        {
                            CategoryID = CateId,
                            Post = postCreate
                        });
                    }
                }

                await _context.SaveChangesAsync();
                StatusMessage = "Vừa tạo bài viết mới bài viết: " + postCreate.Title;
                return RedirectToAction("Index", "Home");
            }
            ViewData["AuthorId"] = new SelectList(_context.Users, "Id", "Id", postCreate.AuthorId);
            return View(postCreate);
        }

        // GET: blog/Post/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Posts == null)
            {
                return NotFound();
            }

            var post = await _context.Posts.Include(p => p.PostCategories).FirstOrDefaultAsync(p => p.PostId == id);

            if (post == null)
            {
                return NotFound();
            }

            var postEdit = new CreatePostModel()
            {
                PostId = post.PostId,
                Title = post.Title,
                Content = post.Content,
                Description = post.Description,
                Slug = post.Slug,
                Published = post.Published,
                CategoryIDs = post.PostCategories.Select(pc => pc.CategoryID).ToArray()
            };

            var categories = await _context.Categories.ToListAsync();
            ViewData["categories"] = new MultiSelectList(categories, "Id", "Title");
            return View(postEdit);
        }

        // POST: blog/Post/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PostId,Title,Description,Slug,Content,Published,CategoryIDs")] CreatePostModel post)
        {
            if (id != post.PostId)
            {
                return NotFound();
            }

            var categories = await _context.Categories.ToListAsync();
            ViewData["categories"] = new MultiSelectList(categories, "Id", "Title");

            //dùng để tự động phát sinh slug từ title của bài viết khi user ko nhập slug
            if (post.Slug == null)
            {
                post.Slug = AppUtilities.GenerateSlug(post.Title);
            }

            if (await _context.Posts.AnyAsync(p => p.Slug == post.Slug && p.PostId != id))
            {
                ModelState.AddModelError("Slug", "Nhập chuỗi url khác");
                return View(post);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var postUpdate = await _context.Posts.Include(p => p.PostCategories).FirstOrDefaultAsync(p => p.PostId == id);

                    if (postUpdate == null)
                    {
                        return NotFound();
                    }

                    postUpdate.Title = post.Title;
                    postUpdate.Description = post.Description;
                    postUpdate.Content = post.Content;
                    postUpdate.Published = post.Published;
                    postUpdate.Slug = post.Slug;
                    postUpdate.DateUpdated = DateTime.Now;

                    //cập nhật danh mục
                    if (post.CategoryIDs == null)
                    {
                        post.CategoryIDs = new int[] { };
                    }

                    var oldCateIds = postUpdate.PostCategories.Select(c => c.CategoryID).ToArray();  //CateId cũ
                    var newCateIds = post.CategoryIDs;  //CateId binding đến

                    //có trong oldCateIds nhưng ko có trong newCateids
                    //kiểm tra xem các Cate cũ có trong Cate mới không, nếu không có thì xóa đi
                    var removeCatePosts = from postCate in postUpdate.PostCategories
                                          where (!newCateIds.Contains(postCate.CategoryID))
                                          select postCate;

                    _context.postCategories.RemoveRange(removeCatePosts);

                    //thêm những cate mới (không bao gồm cate cũ)
                    var addCateIds = from CateId in newCateIds
                                     where !oldCateIds.Contains(CateId)
                                     select CateId;

                    foreach (var CateId in addCateIds)
                    {
                        _context.postCategories.Add(new PostCategory()
                        {
                            PostID = id,
                            CategoryID = CateId
                        });
                    }

                    _context.Update(postUpdate); //update 1 bài post
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(post.PostId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                StatusMessage = "Vừa cập nhật bài viết: " + post.Title;
                return RedirectToAction(nameof(Index));
            }
            ViewData["AuthorId"] = new SelectList(_context.Users, "Id", "Id", post.AuthorId);
            return View(post);
        }

       

        private bool PostExists(int id)
        {
          return (_context.Posts?.Any(e => e.PostId == id)).GetValueOrDefault();
        }


        public class UploadOneFile
        {
            [Required(ErrorMessage = "Phải chọn một file")]
            [DataType(DataType.Upload)]
            [FileExtensions(Extensions = "png,jpg,jpeg,gif")]
            [Display(Name = "Chọn file upload")]
            public IFormFile FileUpload { set; get; }
        }

        //lấy danh sách ảnh
        [HttpPost]
        public IActionResult ListPhotos(int id)
        {
            //lấy bài viết theo id
            var post = _context.Posts.Where(e => e.PostId == id)
                                            .Include(p => p.Photos)
                                            .FirstOrDefault();
            if (post == null)
            {
                return Json(
                    new
                    {
                        success = 0,
                        message = "Posts not found",
                    }
               );
            }


            var listphotos = post.Photos.Select(photo => new
            {
                id = photo.Id,   //lấy id của ảnh
                path = "/contents/BlogPhoto/" + photo.FileName //lấy đường dẫn
            });

            return Json(
                new
                {
                    success = 1, 
                    photos = listphotos, //trả về danh sách các ảnh
                }
            );

        }

        [HttpPost]
        public IActionResult DeletePhoto(int id)
        {
            //tìm ảnh theo id
            var photo = _context.PostPhotos.Where(p => p.Id == id).FirstOrDefault();

            if (photo != null)
            {
                _context.Remove(photo);
                _context.SaveChanges();

                var filename = "/Uploads/BlogPhoto/" + photo.FileName;
                System.IO.File.Delete(filename); //xóa hình ảnh trong file Uploads/BlogPhoto
            }
            return Ok();
        }


        [HttpPost]
        public async Task<IActionResult> UpLoadPhotoApi(int id, [Bind("FileUpload")] UploadOneFile f)
        {
            //lấy ra bài viết
            var post = _context.Posts.Where(e => e.PostId == id)
                                            .Include(p => p.Photos)
                                            .FirstOrDefault();
            if (post == null)
            {
                return NotFound("Không có bài post");
            }


            if (f != null)
            {
                //Tạo tên ngẫu nhiên cho tệp tin hình ảnh mới bằng Tên nn + phần mở rộng
                                   //bỏ đi phần mở rộng      //tạo tên file ngẫu  nhiên           //lấy phần mở rộng của file tải lên
                var file1 = Path.GetFileNameWithoutExtension(Path.GetRandomFileName())   + Path.GetExtension(f.FileUpload.FileName);

                //tạo đường dẫn đầy đủ đến thư mục và tên tệp tin hình ảnh mới.
                var file = Path.Combine("Uploads", "BlogPhoto", file1); // ==> "Uploads/BlogPhoto/file1"

                //ghi dữ liệu từ tệp tin hình ảnh được tải lên vào đường dẫn file.
                //mở tệp "file", FileMode.Create: tạo or ghi đè
                using (var filestream = new FileStream(file, FileMode.Create))
                {
                    await f.FileUpload.CopyToAsync(filestream); //copy dữ liệu từ f.FileUpload vào filestream
                }

                _context.Add(new PostPhoto()
                {
                    PostID = post.PostId,
                    FileName = file1
                });

                await _context.SaveChangesAsync();
            }

            return Ok();
        }





    }
}
