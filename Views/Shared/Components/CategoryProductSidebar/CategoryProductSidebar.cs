using System;
using HocAspMVC4_Test.Models.Product;
using Microsoft.AspNetCore.Mvc;
using Test123.Models;

namespace HocAspMVC4_Test.Views.Shared.Components.CategoryProductSidebar
{
	[ViewComponent]
	public class CategoryProductSidebar : ViewComponent
	{

		public class CategorySibarData
		{
			public List<CategoryProduct> Categories { set; get; }

			public int level { set; get; }


			public string categoryslug { set; get; }
        }
		


		public IViewComponentResult Invoke(CategorySibarData data)
		{
			return View(data);
		}


	}
}

