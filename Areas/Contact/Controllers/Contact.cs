using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HocAspMVC4.Models;
using Microsoft.AspNetCore.Authorization;

namespace HocAspMVC4.Areas.Contact.Controllers
{
    [Area("Contact")]
    [Authorize(Roles ="Admin")]
    public class Contact : Controller
    {
        private readonly AppDbContext1 _context;
         
        public Contact(AppDbContext1 context)
        {
            _context = context;
        }

        [HttpGet("/admin/contact")]
        public async Task<IActionResult> Index()
        {
              return _context.Contacts != null ? 
                          View(await _context.Contacts.ToListAsync()) :
                          Problem("Entity set 'AppDbContext1.Contacts'  is null.");
        }


        [HttpGet("/admin/contact/detail/{id}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Contacts == null)
            {
                return NotFound();
            }

            var contactModel = await _context.Contacts
                .FirstOrDefaultAsync(m => m.id == id);
            if (contactModel == null)
            {
                return NotFound();
            }

            return View(contactModel);
        }


        [HttpGet("/contact")]
        [AllowAnonymous]
        public IActionResult SendContact()
        {
            return View();
        }

        
        [HttpPost("/contact/")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendContact([Bind("FullName,Email,Message,Phone")] ContactModel contactModel)
        {
            if (ModelState.IsValid) //kiểm tra dữ liệu submit đến có phù hợp với cái ta đã set up hay ko
            {
                contactModel.DateSent = DateTime.Now; //lấy thời gian submit đến
                _context.Add(contactModel);  //thêm model đã submit ấy vào database
                await _context.SaveChangesAsync(); //lưu

                TempData["StatusMessage"] = "liện hệ của bạn đã được gửi";

                return RedirectToAction("Index", "Home"); //sau đó chuyển hướng về index của home
            }
            return View(contactModel); //ko phù hợp trả về chính trang đó và báo lỗi 
        }


        [HttpGet("/admin/contact/delete{id}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Contacts == null)
            {
                return NotFound();
            }

            var contactModel = await _context.Contacts
                .FirstOrDefaultAsync(m => m.id == id);
            if (contactModel == null)
            {
                return NotFound();
            }

            return View(contactModel);
        }

        
        [HttpPost("/admin/contact/delete{id}"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Contacts == null)
            {
                return Problem("Entity set 'AppDbContext1.Contacts'  is null.");
            }
            var contactModel = await _context.Contacts.FindAsync(id);
            if (contactModel != null)
            {
                _context.Contacts.Remove(contactModel);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    }
}
