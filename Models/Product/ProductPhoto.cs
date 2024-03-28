using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HocAspMVC4_Test.Models.Product
{
	[Table("ProductPhoto")]
	public class ProductPhoto
	{
		[Key]
		public int Id { set; get; }


		//truy cập theo path: /contents/Products/abc.jpeg
		public string FileName { get; set; }

        
        public int? ProductID { get; set; }

        [ForeignKey("ProductID")]
		public ProductModel? Product { get; set; }

	}
}

