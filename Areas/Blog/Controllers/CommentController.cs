using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using HocAspMVC4.Models;
using HocAspMVC4_Test.Models.Blog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace AppTest1.Areas.blog.Controllers
{
    
    [Area("blog")]
    [Route("Comment/[action]/{id?}")]
    [Authorize]
    public class CommentController : Controller
    {
        private readonly AppDbContext1 _context;

        private readonly UserManager<AppUser> _userManager;

        public CommentController(AppDbContext1 context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        // GET: blog/Comment/Create
        public IActionResult Create(int postId)
        {
            ViewData["PostId"] = postId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int postId, Comment comment)
        {
            if (User.Identity.IsAuthenticated) // Kiểm tra xem user đã đăng nhập chưa
            {
                // Lấy thông tin user đang đăng nhập
                var user = await _userManager.GetUserAsync(this.User);  //  lấy user đang thực hiện hành động

                if (user != null)
                {
                    comment.AuthorId = user.Id;
                    comment.DateCreated = DateTime.Now;
                    comment.PostId = postId;

                    // Thêm comment mới vào cơ sở dữ liệu
                    _context.Comments.Add(comment);
                    await _context.SaveChangesAsync();

                    // Truy vấn lại bài viết
                    var post = await _context.Posts.FirstOrDefaultAsync(p => p.PostId == postId);

                    if (post != null)
                    {
                        // Cập nhật danh sách Comments của bài viết
                        post.Comments.Add(comment);
                        await _context.SaveChangesAsync();

                        // Chuyển hướng đến trang Details.html của ViewPostController
                        return RedirectToAction("Details", "ViewPost", new { postslug = post.Slug });
                    }
                }
            }

            // Trường hợp xử lý lỗi
            return RedirectToAction("Index", "Home");
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var comment = await _context.Comments.FindAsync(id);

            if (comment == null)
            {
                return NotFound();
            }

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            // Truy vấn lại bài viết
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.PostId == comment.PostId);

            if (post != null)
            {
                // Chuyển hướng đến trang Details.html của ViewPostController
                return RedirectToAction("Details", "ViewPost", new { postslug = post.Slug });
            }

            // Trường hợp xử lý lỗi
            return RedirectToAction("Index", "Home");
        }
    }
}
