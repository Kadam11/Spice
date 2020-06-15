using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using Spice.Data;
using Spice.Data.Migrations;
using Spice.Models;
using Spice.Models.ViewModels;
using Spice.Utility;

namespace Spice.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MenuItemController : Controller
    {
        private readonly ApplicationDbContext _db;
        #region Why I have Used IWebHostEnvironment 
        //Whenever we are saving anything on the server or we need the route path of the application we will
        //Also have to use the HOSTING Environment and we fetc that using Dependancy Injection.
        #endregion
        private readonly IWebHostEnvironment _hostingEnvironment;
        #region Use of BindProperty Attribute 
        //This Controller has MenuIteamVM property of Type MenuItemViewModel attached with it .
        //So Next time we can directly use that property insted of passing it as an argument.
        //We need to initialize it in the constructor
        #endregion 
        [BindProperty]
        public MenuItemViewModel MenuItemVM { get; set; }


        public MenuItemController(ApplicationDbContext Db, IWebHostEnvironment hostingEnvironment)
        {
            _db = Db;
            _hostingEnvironment = hostingEnvironment;
            MenuItemVM = new MenuItemViewModel()
            {
                Category = _db.Categories,
                MenuItem = new Models.MenuItem()
            };
        }

        #region Index Method 
        public async Task<IActionResult> Index()
        {
            var menuItems = await _db.MenuItems.Include(m => m.Category).Include(m => m.SubCategory).ToListAsync();
            return View(menuItems);
        }
        #endregion

        #region Create 
        [HttpGet]
        public IActionResult Create()
        {
            return View(MenuItemVM);
        }

        [HttpPost]
        [ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePOST()
        {
            MenuItemVM.MenuItem.SubCategoryId = Convert.ToInt32(Request.Form["SubCategoryId"].ToString());
            if (!ModelState.IsValid)
            {
                return View(MenuItemVM);
            }
            _db.MenuItems.Add(MenuItemVM.MenuItem);
            await _db.SaveChangesAsync();

            //Work on the Image Saving section
            //Name of the Image must be unique.we can not have two images with Same name.
            //Even if User uploads the same image we have to rename it .

            string webRootPath = _hostingEnvironment.WebRootPath;
            var files = HttpContext.Request.Form.Files;
            var menuItemFromDb = await _db.MenuItems.FindAsync(MenuItemVM.MenuItem.Id);

            if (files.Count > 0)
            {
                //File has been uploaded...
                var uploads = Path.Combine(webRootPath, "images");
                var extensions = Path.GetExtension(files[0].FileName);

                using (var fileStrem = new FileStream(Path.Combine(uploads, MenuItemVM.MenuItem.Id + extensions), FileMode.Create))
                {
                    files[0].CopyTo(fileStrem);
                }
                menuItemFromDb.Image = @"\images\" + MenuItemVM.MenuItem.Id + extensions;
            }
            else
            {
                // no file was uploaded so use defaults...
                var uploads = Path.Combine(webRootPath, @"images\" + SD.DefaultFoodImage);
                System.IO.File.Copy(uploads, webRootPath + @"\images\" + MenuItemVM.MenuItem.Id + ".png");
                menuItemFromDb.Image = @"\images\" + MenuItemVM.MenuItem.Id + ".png";
                var kadam = webRootPath + @"\images\" + MenuItemVM.MenuItem.Id + ".png";
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }
        #endregion

        #region Edit

        [HttpGet]
        public async Task<IActionResult> Edit(int? Id)
        {
            if (Id == null)
            {
                return NotFound();
            }
            MenuItemVM.MenuItem = await _db.MenuItems.Include(m => m.Category).Include(m => m.SubCategory).SingleOrDefaultAsync(m => m.Id == Id);
            MenuItemVM.SubCategory = await _db.SubCategories.Where(s => s.CategoryId == MenuItemVM.MenuItem.CategoryId).ToListAsync();
            if (MenuItemVM.MenuItem == null)
            {
                return NotFound();
            }
            return View(MenuItemVM);
        }

        [HttpPost]
        [ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPOST()
        {
            MenuItemVM.MenuItem.SubCategoryId = Convert.ToInt32(Request.Form["SubCategoryId"].ToString());
            if (!ModelState.IsValid)
            {
                MenuItemVM.SubCategory = await _db.SubCategories.Where(s => s.CategoryId == MenuItemVM.MenuItem.CategoryId).ToListAsync();
                return View(MenuItemVM);
            }


            //Work on the Image Saving section
            //Name of the Image must be unique.we can not have two images with Same name.
            //Even if User uploads the same image we have to rename it .

            string webRootPath = _hostingEnvironment.WebRootPath;
            var files = HttpContext.Request.Form.Files;
            var menuItemFromDb = await _db.MenuItems.FindAsync(MenuItemVM.MenuItem.Id);

            if (files.Count > 0)
            {
                //New File has been uploaded...
                var uploads = Path.Combine(webRootPath, "images");
                var extensions_new = Path.GetExtension(files[0].FileName);

                //Delete the original File
                var imagepath = Path.Combine(webRootPath, menuItemFromDb.Image.TrimStart('\\'));
                if (System.IO.File.Exists(imagepath))
                {
                    System.IO.File.Delete(imagepath);
                }

                //We will upload the new file 
                using (var fileStrem = new FileStream(Path.Combine(uploads, MenuItemVM.MenuItem.Id + extensions_new), FileMode.Create))
                {
                    files[0].CopyTo(fileStrem);
                }
                menuItemFromDb.Image = @"\images\" + MenuItemVM.MenuItem.Id + extensions_new;
            }
            menuItemFromDb.Name = MenuItemVM.MenuItem.Name;
            menuItemFromDb.Description = MenuItemVM.MenuItem.Description;
            menuItemFromDb.Price = MenuItemVM.MenuItem.Price;
            menuItemFromDb.Spicyness = MenuItemVM.MenuItem.Spicyness;
            menuItemFromDb.CategoryId = MenuItemVM.MenuItem.CategoryId;
            menuItemFromDb.SubCategoryId = MenuItemVM.MenuItem.SubCategoryId;

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }

        #endregion

        #region Details Method 
        public async Task<IActionResult> Details(int? Id)
        {
            if (Id == null)
            {
                return NotFound();
            }
            MenuItemVM.MenuItem = await _db.MenuItems.Where(x => x.Id == Id).Include(x => x.SubCategory).Include(x => x.Category).SingleOrDefaultAsync(x => x.Id == Id);
            return View(MenuItemVM);
        }
        #endregion

        #region Delete Method
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            MenuItemVM.MenuItem = await _db.MenuItems.Include(m => m.Category).Include(m => m.SubCategory).SingleOrDefaultAsync(m => m.Id == id);

            if (MenuItemVM.MenuItem == null)
            {
                return NotFound();
            }

            return View(MenuItemVM);
        }

        //POST Delete MenuItem
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            string webRootPath = _hostingEnvironment.WebRootPath;
            MenuItem menuItem = await _db.MenuItems.FindAsync(id);

            if (menuItem != null)
            {
                var imagePath = Path.Combine(webRootPath, menuItem.Image.TrimStart('\\'));

                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
                _db.MenuItems.Remove(menuItem);
                await _db.SaveChangesAsync();

            }

            return RedirectToAction(nameof(Index));
        }

        #endregion

    }
}
