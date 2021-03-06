﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Spice.Data;
using Spice.Models;
using Spice.Models.ViewModels;

namespace Spice.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SubCategoryController : Controller
    {
        #region variables Declaration   
        [TempData]
        public string StatusMessage { get; set; }
        private readonly ApplicationDbContext _db;
        #endregion 

        #region ApplicationDbContext variable Initialized using Constructor
        public SubCategoryController(ApplicationDbContext Db)
        {
            _db = Db;
        }
        #endregion

        #region Index Action Method

        [Area("Admin")]
        public async Task<IActionResult> Index()
        {
            var subCatagories = await _db.SubCategories.Include(s => s.Category).ToListAsync();
            return View(subCatagories);
        }
        #endregion

        #region Create Method 
        //GET - CREATE 
        public async Task<IActionResult> Create()
        {

            // ViewBag.CategoryList = new SelectList(_db.Categories, "Id", "Name");
            SubCategoryAndCategoryViewModel model = new SubCategoryAndCategoryViewModel()
            {
                CategoryList = await _db.Categories.ToListAsync(),
                SubCategory = new Models.SubCategory(),
                SubCategoryList = await _db.SubCategories.OrderBy(p => p.Name).Select(p => p.Name).Distinct().ToListAsync()
               

            };
            return View(model);
        }

        // POST - CREATE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubCategoryAndCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var doesSubCategoryExists = _db.SubCategories.Include(s => s.Category).Where(s => s.Name == model.SubCategory.Name && s.Category.Id == model.SubCategory.CategoryId);
                if (doesSubCategoryExists.Count() > 0)
                {
                    //Error
                    StatusMessage = "Error : Sub Category exists under " + doesSubCategoryExists.First().Category.Name + " category. Please use another Name ";
                }
                else
                {
                    _db.SubCategories.Add(model.SubCategory);
                    await _db.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            SubCategoryAndCategoryViewModel vm = new SubCategoryAndCategoryViewModel()
            {
                CategoryList = await _db.Categories.ToListAsync(),
                SubCategory = new Models.SubCategory(),
                SubCategoryList = await _db.SubCategories.OrderBy(p => p.Name).Select(p => p.Name).Distinct().ToListAsync(),
                StatusMessage = this.StatusMessage
            };
            vm.SubCategory = model.SubCategory;
            return View(vm);
        }

        #endregion

        #region method that sends Data in JSON to update SubCategory
        [ActionName("GetSubCategory")]
        public async Task<IActionResult> GetSubCategory (int id)
        {
            List<SubCategory> subCategories = new List<SubCategory>();
            subCategories = (from subCategory in _db.SubCategories
                                                 where subCategory.CategoryId == id
                                                 select subCategory).ToList();
            return Json(new SelectList(subCategories,"Id","Name"));
        }
        #endregion

        #region Edit Method

        //GET - EDIT 
        public async Task<IActionResult> Edit(int? Id)
        {
            if(Id == null)
            {
                return NotFound();
            }
            var subCategory = await _db.SubCategories.SingleOrDefaultAsync(m => m.Id == Id);
            if(subCategory == null)
            {
                return NotFound();

            }
            // ViewBag.CategoryList = new SelectList(_db.Categories, "Id", "Name");
            SubCategoryAndCategoryViewModel model = new SubCategoryAndCategoryViewModel()
            {
                CategoryList = await _db.Categories.ToListAsync(),
                SubCategory = subCategory,
                SubCategoryList = await _db.SubCategories.OrderBy(p => p.Name).Select(p => p.Name).Distinct().ToListAsync()


            };
           
            return View(model);
        }

        
        // POST - Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? Id, SubCategoryAndCategoryViewModel model)
        {
            if (ModelState.IsValid)
             {
                var doesSubCategoryExists = _db.SubCategories.Include(s => s.Category).Where(s => s.Name == model.SubCategory.Name && s.Category.Id == model.SubCategory.CategoryId);
                if (doesSubCategoryExists.Count() > 0)
                {
                    //Error
                    StatusMessage = "Error : Sub Category exists under " + doesSubCategoryExists.First().Category.Name + " category. Please use another Name ";
                }
                else
                {
                    var subCatFromDb = await _db.SubCategories.FindAsync(Id);
                    subCatFromDb.Name = model.SubCategory.Name;
                    await _db.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            SubCategoryAndCategoryViewModel vm = new SubCategoryAndCategoryViewModel()
            {
                CategoryList = await _db.Categories.ToListAsync(),
                SubCategory = model.SubCategory,
                SubCategoryList = await _db.SubCategories.OrderBy(p => p.Name).Select(p => p.Name).Distinct().ToListAsync(),
                StatusMessage = this.StatusMessage
            };
            //Solution No 1 for Null Refrence 
            //Solution 2 is inside View But we need to change Action Method to =>public async Task<IActionResult> Edit(SubCategoryAndCategoryViewModel model) )
            vm.SubCategory.Id = (int)Id;
            return View(vm);
        }
        #endregion

        #region Detail Method
        public async Task<IActionResult> Detail(int? Id)
        {
            if (Id == null)
            {
                return NotFound();
            }
            var subCategory = await _db.SubCategories.Include(x => x.Category).SingleOrDefaultAsync(m => m.Id == Id);
            if (subCategory == null)
            {
                return NotFound();
            }

            return View(subCategory);

        }


        #endregion

        #region Delete Method 
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var subCategory = await _db.SubCategories.Include(s => s.Category).SingleOrDefaultAsync(m => m.Id == id);
            if (subCategory == null)
            {
                return NotFound();
            }

            return View(subCategory);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var subCategory = await _db.SubCategories.SingleOrDefaultAsync(m => m.Id == id);
            _db.SubCategories.Remove(subCategory);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }
        #endregion
    }
}
