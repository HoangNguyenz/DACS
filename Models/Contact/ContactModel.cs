using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HocAspMVC4.Models
{
	public class ContactModel
	{
		[Key]
		public int id { set; get; }


		[Column(TypeName ="nvarchar")]
		[StringLength(50)]
		[Required(ErrorMessage ="Phải nhập {0}")]
		[Display(Name ="Họ và tên")]
		public string FullName { set; get; }


		[Required(ErrorMessage ="Phải nhập {0}")]
        [StringLength(100)]
        [Display(Name = "Địa chỉ Email")]
		[EmailAddress(ErrorMessage ="Phải là địa chỉ Email")]
        public string Email { set; get; }


		public DateTime? DateSent { set; get; }

        [Display(Name = "Nội Dung")]
        public string Message { set; get; }


        [StringLength(50)]
		[Phone(ErrorMessage ="Phải là số điện thoại")]
        [Display(Name = "Số điện thoại")]
        public string Phone { set; get; }



    }
}

