using System;
using System.ComponentModel.DataAnnotations;
using HocAspMVC4_Test.Models.Product;

namespace AppTest1.Areas.Product.Models
{
	public class CreateProductModel : ProductModel
	{
        //cho thêm 1 mảng số nguyên chứa các id của các danh mục, để cho biết bài post này thuộc category nào
        [Display(Name = "Chuyên mục")]
        public int[]? CategoryIDs { get; set; } 
	}
}

