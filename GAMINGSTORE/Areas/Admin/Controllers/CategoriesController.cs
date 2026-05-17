using GAMINGSTORE.Models;
using GAMINGSTORE.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GAMINGSTORE.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class CategoriesController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public CategoriesController(IProductRepository productRepository,
            ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _categoryRepository.GetAllAsync();
            // Order by hierarchy for better display in Index if needed, but for now just pass all
            return View(categories);
        }

        public async Task<IActionResult> Display(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        public async Task<IActionResult> Add()
        {
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.ParentCategories = new SelectList(categories, "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(Category category)
        {
            if (ModelState.IsValid)
            {
                await _categoryRepository.AddAsync(category);
                return RedirectToAction(nameof(Index));
            }
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.ParentCategories = new SelectList(categories, "Id", "Name", category.ParentId);
            return View(category);
        }

        public async Task<IActionResult> Update(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            var categories = (await _categoryRepository.GetAllAsync()).Where(c => c.Id != id).ToList();
            ViewBag.ParentCategories = new SelectList(categories, "Id", "Name", category.ParentId);
            return View(category);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int id, Category category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                await _categoryRepository.UpdateAsync(category);
                return RedirectToAction(nameof(Index));
            }
            var categories = (await _categoryRepository.GetAllAsync()).Where(c => c.Id != id).ToList();
            ViewBag.ParentCategories = new SelectList(categories, "Id", "Name", category.ParentId);
            return View(category);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category != null)
            {
                await _categoryRepository.DeleteAsync(id);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}