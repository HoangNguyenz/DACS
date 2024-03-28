



using System;
using HocAspMVC4_Test.Models.Product;

namespace HocAspMVC4_Test.Areas.Product.Models
{
	public class CartItem
	{
        public int quantity { set; get; }
        public ProductModel product { set; get; }
    }
}

