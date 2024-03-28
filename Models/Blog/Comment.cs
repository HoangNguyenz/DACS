using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HocAspMVC4.Models;

namespace HocAspMVC4_Test.Models.Blog
{
    [Table("Comment")]
    public class Comment
    {
        [Key]
        public int CommentId { get; set; }

        [Display(Name = "Tác giả")]
        public string? AuthorId { set; get; }

        [ForeignKey("AuthorId")]
        [Display(Name = "Tác giả")]
        public AppUser? Author { set; get; }

        [Display(Name = "Nội dung")]
        [Required(ErrorMessage = "Phải có nội dung bình luận")]
        public string Content { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime DateCreated { get; set; }

        [Display(Name = "Bài viết")]
        public int PostId { get; set; }

        [ForeignKey("PostId")]
        [Display(Name = "Bài viết")]
        public Post Post { get; set; }
    }
}
