using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Identity;

namespace App.ExtendMethods
{
    //hỗ trợ cho việc xử lý lỗi trong ModelState.
    public static class ModelStateExtend
    {
        public static void AddModelError(this ModelStateDictionary ModelState, string mgs)
        {
            ModelState.AddModelError(string.Empty, mgs); //thêm lỗi, msg là thông báo
        }
        //lỗi liên quan đến sác thực người dùng
        public static void AddModelError(this ModelStateDictionary ModelState, IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.Description);
            }
        }
    }
}

