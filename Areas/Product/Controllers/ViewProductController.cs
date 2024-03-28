using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Threading.Tasks;
using App.Models;
using HocAspMVC4.Models;
using HocAspMVC4_Test.Areas.Product.Models;
using HocAspMVC4_Test.Areas.Product.Service;
using HocAspMVC4_Test.Models.Product;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace AppTest1.Areas.Product.Controllers
{
    [Area("Product")]
    public class ViewProductController : Controller
    {
        private readonly AppDbContext1 _context;
        private readonly ILogger<ViewProductController> _logger;
        private readonly CartService _cartService;

        public ViewProductController(AppDbContext1 context, ILogger<ViewProductController> logger, CartService cartService)
        {
            _context = context;
            _logger = logger;
            _cartService = cartService;
        }


        // /post
        // post/{categorySlug}
        [Route("/product/{categoryslug?}")]
        public IActionResult Index(string categoryslug, [FromQuery(Name ="p")]int currentPage, int pagesize)
        {
            var categories = GetCategories();
            ViewBag.categories = categories;
            ViewBag.categoryslug = categoryslug;

            CategoryProduct category = null;

            if (!string.IsNullOrEmpty(categoryslug)) //nếu slug ko null
            {
                category = _context.CategoryProduct.Where(c => c.Slug == categoryslug)
                                               .Include(c => c.CategoryChildren)
                                               .FirstOrDefault();
                if (category == null)
                {
                    return NotFound("Không tìm thấy danh mục này");
                }
            }

            var products = _context.Products
                .Include(p => p.Author)
                .Include(p => p.Photos)
                .Include(p => p.ProductCategoryProduct)
                .ThenInclude(p => p.Category)
                .OrderByDescending(p => p.DateUpdated)
                .AsQueryable();


            if (category != null)
            {
                var ids = new List<int>();
                category.ChildCategoryIDs(null, ids); //chứa ds các id con
                ids.Add(category.Id);

                products = products.Where(p => p.ProductCategoryProduct.Where(pc => ids.Contains(pc.CategoryID)).Any());

            }

            //phân trang
            int totalProducts = products.Count();
            if (pagesize <= 0)
            {
                pagesize = 6;
            }
            int countPages = (int)Math.Ceiling((double)totalProducts / pagesize);


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

            //lấy ra số lượng bài post trong 1 trang
            var productsInPage = products.Skip((currentPage - 1) * pagesize)
                       .Take(pagesize);

            ViewBag.pagingModel = pagingModel;
            ViewBag.totalPosts = totalProducts;

            ViewBag.category = category;
            return View(productsInPage.ToList());
        }

        [Route("/product/{productslug}.html")]
        public IActionResult Details(string productslug)
        {
            var categories = GetCategories();
            ViewBag.categories = categories;

            var product = _context.Products.Where(p => p.Slug == productslug)
                                        .Include(p => p.Author)
                                        .Include(p => p.Photos)
                                        .Include(p => p.ProductCategoryProduct)
                                        .ThenInclude(pc => pc.Category)
                                        .FirstOrDefault();

            if (product == null)
            {
                return NotFound();
            }

            CategoryProduct category = product.ProductCategoryProduct.FirstOrDefault()?.Category;
            ViewBag.category = category;

            //lấy ra những bài post có cùng danh mục với cả bài post hiện tại
            var otherProducts = _context.Products.Where(p => p.ProductCategoryProduct.Any(c => c.CategoryID == category.Id))
                                            .Where(p => p.ProductId != product.ProductId)
                                            .OrderByDescending(p => p.DateUpdated)
                                            .Take(5);
            ViewBag.otherProducts = otherProducts;

            return View(product);
        }


        //method lấy ra tất cả danh mục
        private List<CategoryProduct> GetCategories()
        {
            var categories = _context.CategoryProduct
                .Include(c => c.CategoryChildren) //đã lấy luôn những dm con
                .AsEnumerable()
                .Where(c => c.ParentCategory == null).ToList(); //lấy những dm cha
            return categories;
        }


        /// Thêm sản phẩm vào cart
        [Route("addcart/{productid:int}", Name = "addcart")]
        public IActionResult AddToCart([FromRoute] int productid)
        {

            var product = _context.Products
                .Where(p => p.ProductId == productid)
                .FirstOrDefault();
            if (product == null)
                return NotFound("Không có sản phẩm");

            // Xử lý đưa vào Cart ...
            var cart = _cartService.GetCartItems();
            var cartitem = cart.Find(p => p.product.ProductId == productid);
            if (cartitem != null)
            {
                // Đã tồn tại, tăng thêm 1
                cartitem.quantity++;
            }
            else
            {
                //  Thêm mới
                cart.Add(new CartItem() { quantity = 1, product = product });
            }

            // Lưu cart vào Session
            _cartService.SaveCartSession(cart);
            // Chuyển đến trang hiện thị Cart
            return RedirectToAction(nameof(Cart));
        }

        // Hiện thị giỏ hàng
        [Route("/cart", Name = "cart")]
        public IActionResult Cart()
        {
            return View(_cartService.GetCartItems());
        }

        /// xóa item trong cart
        [Route("/removecart/{productid:int}", Name = "removecart")]
        public IActionResult RemoveCart([FromRoute] int productid)
        {
            var cart = _cartService.GetCartItems();
            var cartitem = cart.Find(p => p.product.ProductId == productid);
            if (cartitem != null)
            {
                // Đã tồn tại, tăng thêm 1
                cart.Remove(cartitem);
            }

            _cartService.SaveCartSession(cart);
            return RedirectToAction(nameof(Cart));
        }

        /// Cập nhật
        [Route("/updatecart", Name = "updatecart")]
        [HttpPost]
        public IActionResult UpdateCart([FromForm] int productid, [FromForm] int quantity)
        {
            // Cập nhật Cart thay đổi số lượng quantity ...
            var cart = _cartService.GetCartItems();
            var cartitem = cart.Find(p => p.product.ProductId == productid);
            if (cartitem != null)
            {
                // Đã tồn tại, tăng thêm 1
                cartitem.quantity = quantity;
            }
            _cartService.SaveCartSession(cart);
            // Trả về mã thành công (không có nội dung gì - chỉ để Ajax gọi)
            return Ok();
        }

        [Route("/checkout")]
        public IActionResult Checkout()
        {
            var cart = _cartService.GetCartItems();


            //sau khi đặt hàng xong thì xóa toàn bộ giỏ hàng
            _cartService.ClearCart();

            return Content("Đã gửi đơn hàng");
        }


    }
}