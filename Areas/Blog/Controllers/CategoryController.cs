using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HocAspMVC4.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Test123.Models;

namespace Test123.Areas.Blog.Controllers
{
    [Area("Blog")]
    [Route("admin/blog/category/[action]/{id?}")]
    //[Authorize(Roles = "Admin")]
    [Authorize(Policy = "AllowEditRole")]
    public class CategoryController : Controller
    {
        private readonly AppDbContext1 _context;

        public CategoryController(AppDbContext1 context)
        {
            _context = context;
        }


        //Hoang
        // GET: Blog/Category
        public async Task<IActionResult> Index()
        {
            var qr = (from c in _context.Categories select c)
                        .Include(c => c.ParentCategory)   //lấy ra tham chiếu ParentCategory (danh mục cha của nó cũng dc lấy ra theo)
                        .Include(c => c.CategoryChildren); //lấy ra cả những danh mục con 

            //chỉ lấy ra các cate ko có danh mục cha
            var categoriesNoParent = (await qr.ToListAsync())
                            .Where(c => c.ParentCategory == null)  
                            .ToList();

            //nhưng thực tế cái categories còn chứa các category con nữa
            //mục đích là để hiển thị các danh mục gốc
            return View(categoriesNoParent);
        }


        // GET: Blog/Category/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var categoryDetail = await _context.Categories
                .Include(c => c.ParentCategory) 
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (categoryDetail == null)
            {
                return NotFound();
            }

            return View(categoryDetail);
        }

        // GET: Blog/Category/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var categoryDelete = await _context.Categories
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (categoryDelete == null)
            {
                return NotFound();
            }

            return View(categoryDelete);
        }

        // POST: Blog/Category/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            //lấy ra danh mục cần xóa
            var categoryDelete = _context.Categories
                .Include(c => c.CategoryChildren)
                .FirstOrDefault(c => c.Id == id);


            if (categoryDelete == null)
            {
                return NotFound();
            }

            //duyệt qua các dm con của dm cần xóa và gán danh mục cha của các dm con đó = dm cha của danh mục cần xóa
            foreach (var cCategory in categoryDelete.CategoryChildren)
            {
                cCategory.ParentCategoryId = categoryDelete.ParentCategoryId;
            }

            _context.Categories.Remove(categoryDelete);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }




        //Hai
        //source là danh mục nguồn => xử lý => và đưa vào danh mục đích: des
        //Chuyển đổi danh sách các đối tượng Category từ nguồn sang đích,
        //với mỗi đối tượng Category được thêm vào danh sách đích với một tiền tố chuỗi "----" lặp lại level lần,
        private void CreateSelectItems(List<Category> source, List<Category> des, int level)
        {
            string prefix = string.Concat(Enumerable.Repeat("----", level));
            foreach (var category in source) //duyệt qua từng p/tử ở nguồn
            {
                des.Add(new Category()
                {
                    Id = category.Id,
                    Title = prefix + " " + category.Title //chèn prefix vào title category
                }); //thêm vào danh sách đích

                if(category.CategoryChildren?.Count > 0) //nếu có danh mục nguồn đó có các danh mục con
                {
                    CreateSelectItems(category.CategoryChildren.ToList(), des, level+1); //gọi đệ quy
                }
            }
        }


        // GET: tạo mới 1 category (tạo mới 1 cate gốc hay 1 cate con)
        public async Task<IActionResult> CreateAsync()
        {
            var qr = (from c in _context.Categories select c)
                        .Include(c => c.ParentCategory)   //lấy ra tham chiếu parentCategory (danh mục cha của nó cũng dc lấy ra theo)
                        .Include(c => c.CategoryChildren); //lấy ra các danh mục con 

            var categoriesNoParent = (await qr.ToListAsync())
                            .Where(c => c.ParentCategory == null)  //chỉ lấy ra các cate ko có danh mục cha
                            .ToList();
            //==> lấy ra các danh mục ko có dm cha (danh mục gốc)

            //0 == vị trí ở đầu 
            categoriesNoParent.Insert(0, new Category() //trong danh sách các dm gốc lấy dc ta chèn thêm vào 1 Category có id = -1 và title
            {
                //đại diện cho tùy chọn "không có danh mục cha" trong form nhập liệu.
                Id = -1,
                Title = "Không có danh mục cha"
            });

            //dùng để đưa qua select list để chọn
            var items = new List<Category>(); //tạo 1 ds mới
            CreateSelectItems(categoriesNoParent, items, 0); //chuyển và đưa vào selectlist
            var selectList = new SelectList(items, "Id", "Title"); 


            ViewData["ParentCategoryId"] = selectList; //đổ sang view
            return View();
        }

        // POST: tiến hành tạo mới 1 category
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,Slug,ParentCategoryId")] Category categoryCreate)
        {
            if (ModelState.IsValid) 
            {
                //nếu user chọn ko có danh mục cha tức là ParentCategoryId = -1 thì gán danh mục cha của nó bằng null
                //để biến danh mục đó là danh mục gốc
                if (categoryCreate.ParentCategoryId == -1) categoryCreate.ParentCategoryId = null;
                
                _context.Add(categoryCreate);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }


            //Phần chọn danh mục START
            var qr = (from c in _context.Categories select c)
                      .Include(c => c.ParentCategory)   //lấy ra tham chiếu parentCategory (danh mục cha của nó cũng dc lấy ra theo)
                      .Include(c => c.CategoryChildren); //lấy ra các danh mục con 

            var categories = (await qr.ToListAsync())
                            .Where(c => c.ParentCategory == null)  //chỉ lấy ra các cate ko có danh mục cha
                            .ToList();

            categories.Insert(0, new Category()
            {
                Id = -1,
                Title = "Không có danh mục cha"
            });

            var items = new List<Category>();
            CreateSelectItems(categories, items, 0);
            var selectList = new SelectList(items, "Id", "Title");
            //Phần chọn danh END


            ViewData["ParentCategoryId"] = selectList;
            return View(categoryCreate);
        }

        // GET: Blog/Category/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var categoryEdit = await _context.Categories.FindAsync(id);

            if (categoryEdit == null)
            {
                return NotFound();
            }


            //Phần chọn danh mục START
            var qr = (from c in _context.Categories select c)
                    .Include(c => c.ParentCategory)   //lấy ra tham chiếu parentCategory (danh mục cha của nó cũng dc lấy ra theo)
                    .Include(c => c.CategoryChildren); //lấy ra các danh mục con 

            var categories = (await qr.ToListAsync())
                            .Where(c => c.ParentCategory == null)  //chỉ lấy ra các cate ko có danh mục cha
                            .ToList();

            categories.Insert(0, new Category()
            {
                Id = -1,
                Title = "Không có danh mục cha"
            });

            var items = new List<Category>();
            CreateSelectItems(categories, items, 0);
            var selectList = new SelectList(items, "Id", "Title");
            //Phần chọn danh mục END


            ViewData["ParentCategoryId"] = selectList;
            return View(categoryEdit);
        }

        // POST: Blog/Category/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Slug,ParentCategoryId")] Category categoryEdit)
        {
            if (id != categoryEdit.Id)
            {
                return NotFound();
            }

            bool canUpdate = true;

            if (categoryEdit.ParentCategoryId == categoryEdit.Id) //nếu user chọn chính nó làm dm cha của nó
            {
                ModelState.AddModelError(string.Empty, "Phải chọn danh mục cha khác");
                canUpdate = false;
            }

           
            if (ModelState.IsValid && canUpdate)
            {
                try
                {
                    //là danh mục gốc
                    if (categoryEdit.ParentCategoryId == -1)
                    {
                        categoryEdit.ParentCategoryId = null;
                    }

                    var dtc = _context.Categories.FirstOrDefault(c => c.Id == id);
                    //đặt trạng thái của đối tượng danh mục đã lấy thành Detached,
                    //để tránh sự xung đột với đối tượng đã tồn tại trong DbContext.
                    _context.Entry(dtc).State = EntityState.Detached;


                    _context.Update(categoryEdit);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(categoryEdit.Id)) //ko tồn tại trong database
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            //Phần chọn danh mục START
            var qr = (from c in _context.Categories select c)
                    .Include(c => c.ParentCategory)   //lấy ra tham chiếu parentCategory (danh mục cha của nó cũng dc lấy ra theo)
                    .Include(c => c.CategoryChildren); //lấy ra các danh mục con 

            var categories = (await qr.ToListAsync())
                            .Where(c => c.ParentCategory == null)  //chỉ lấy ra các cate ko có danh mục cha
                            .ToList();

            categories.Insert(0, new Category()
            {
                Id = -1,
                Title = "Không có danh mục cha"
            });

            var items = new List<Category>();
            CreateSelectItems(categories, items, 0);
            var selectList = new SelectList(items, "Id", "Title");
            //Phần chọn danh mục END


            ViewData["ParentCategoryId"] = selectList;
            return View(categoryEdit);
        }

        //kiểm tra xem một đối tượng Category với Id tương ứng đã tồn tại trong cơ sở dữ liệu chưa.
        private bool CategoryExists(int id)
        {
          return (_context.Categories?.Any(e => e.Id == id)).GetValueOrDefault(); //trả về true nếu tồn tại
        }
    }
}
