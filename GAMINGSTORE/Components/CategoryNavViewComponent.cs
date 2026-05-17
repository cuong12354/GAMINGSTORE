using GAMINGSTORE.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GAMINGSTORE.Components
{
    public class CategoryNavViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public CategoryNavViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Fetch all categories with their children hierarchy
            var categories = await _context.Categories
                .Include(c => c.SubCategories)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            // We only pass top-level categories to the view
            var topLevelCategories = categories.Where(c => c.ParentId == null).ToList();

            return View(topLevelCategories);
        }
    }
}
