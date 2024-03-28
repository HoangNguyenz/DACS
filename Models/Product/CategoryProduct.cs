using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HocAspMVC4_Test.Models.Product
{
    [Table("CategoryProduct")]
    public class CategoryProduct
    {
        [Key]
        public int Id { get; set; }

        // Tiêu đề Category
        [StringLength(100, MinimumLength = 3, ErrorMessage = "{0} dài {1} đến {2}")]
        [Display(Name = "Tên danh mục")]
        public string Title { get; set; }

        // Nội dung, thông tin chi tiết về Category
        [DataType(DataType.Text)]
        [Display(Name = "Nội dung danh mục")]
        public string Description { set; get; }

        //chuỗi Url
        [StringLength(100, MinimumLength = 3, ErrorMessage = "{0} dài {1} đến {2}")]
        [RegularExpression(@"^[a-z0-9-]*$", ErrorMessage = "Chỉ dùng các ký tự [a-z0-9-]")]
        [Display(Name = "Url hiện thị")]
        public string Slug { set; get; }

        // Các danh mục con
        public ICollection<CategoryProduct>? CategoryChildren { get; set; }

        // Category cha (FKey)
        [Display(Name = "Danh mục cha")]
        public int? ParentCategoryId { get; set; }

        [ForeignKey("ParentCategoryId")]  //khóa ngoại
        [Display(Name = "Danh mục cha")]
        public CategoryProduct? ParentCategory { set; get; }

        //mỗi category có thể là con của 1 category khác (xây dựng theo dạng cây)
        //1 category có nhiều category con
        //tạo ra khóa ngoại để xác định cha của nó ParentCategoryId


        public void ChildCategoryIDs(ICollection<CategoryProduct> childcates, List<int> lists)
        {
            if (childcates == null)
            {
                childcates = this.CategoryChildren;
            }
            foreach (CategoryProduct category in childcates)
            {
                lists.Add(category.Id);
                ChildCategoryIDs(category.CategoryChildren, lists);
            }
        }

        //breadcrumb
        public List<CategoryProduct> ListParents()
        {
            List<CategoryProduct> li = new List<CategoryProduct>();
            var parent = this.ParentCategory;
            while (parent != null)
            {
                li.Add(parent);
                parent = parent.ParentCategory;
            }
            li.Reverse();
            return li;
        }

    }
}

