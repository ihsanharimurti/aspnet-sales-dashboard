// Controllers/DashboardController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;

    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string category = "", string region = "", string salesperson = "", DateTime? fromDate = null, DateTime? toDate = null)
    {
        // Build query with filters
        var query = _context.SalesData.AsQueryable();

        if (!string.IsNullOrEmpty(category) && category != "All")
        {
            query = query.Where(x => x.Category == category);
        }

        if (!string.IsNullOrEmpty(region) && region != "All")
        {
            query = query.Where(x => x.Region == region);
        }

        if (!string.IsNullOrEmpty(salesperson) && salesperson != "All")
        {
            query = query.Where(x => x.SalesPersonName == salesperson);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(x => x.SaleDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(x => x.SaleDate <= toDate.Value);
        }

        // Get filtered data
        var salesData = await query.OrderByDescending(x => x.SaleDate).ToListAsync();

        // Prepare dropdown data
        ViewBag.Categories = await GetCategoriesSelectList();
        ViewBag.Regions = await GetRegionsSelectList();
        ViewBag.SalesPersons = await GetSalesPersonsSelectList();

        // Prepare current filter values
        ViewBag.CurrentCategory = category;
        ViewBag.CurrentRegion = region;
        ViewBag.CurrentSalesperson = salesperson;
        ViewBag.CurrentFromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.CurrentToDate = toDate?.ToString("yyyy-MM-dd");

        // Calculate summary statistics
        var totalSales = salesData.Sum(x => x.Amount);
        var averageSale = salesData.Any() ? salesData.Average(x => x.Amount) : 0;
        var totalTransactions = salesData.Count;

        ViewBag.TotalSales = totalSales;
        ViewBag.AverageSale = averageSale;
        ViewBag.TotalTransactions = totalTransactions;

        return View(salesData);
    }

    [HttpPost]
    public async Task<IActionResult> GetChartData(string category = "", string region = "", string salesperson = "", DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.SalesData.AsQueryable();

        // Apply same filters as Index method
        if (!string.IsNullOrEmpty(category) && category != "All")
        {
            query = query.Where(x => x.Category == category);
        }

        if (!string.IsNullOrEmpty(region) && region != "All")
        {
            query = query.Where(x => x.Region == region);
        }

        if (!string.IsNullOrEmpty(salesperson) && salesperson != "All")
        {
            query = query.Where(x => x.SalesPersonName == salesperson);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(x => x.SaleDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(x => x.SaleDate <= toDate.Value);
        }

        // Group by month for chart
        var chartData = await query
            .GroupBy(x => new { x.SaleDate.Year, x.SaleDate.Month })
            .Select(g => new {
                Label = $"{g.Key.Year}-{g.Key.Month:00}",
                Value = g.Sum(x => x.Amount)
            })
            .OrderBy(x => x.Label)
            .ToListAsync();

        return Json(chartData);
    }

    private async Task<SelectList> GetCategoriesSelectList()
    {
        var categories = await _context.SalesData
            .Select(x => x.Category)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync();

        categories.Insert(0, "All");
        return new SelectList(categories);
    }

    private async Task<SelectList> GetRegionsSelectList()
    {
        var regions = await _context.SalesData
            .Select(x => x.Region)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync();

        regions.Insert(0, "All");
        return new SelectList(regions);
    }

    private async Task<SelectList> GetSalesPersonsSelectList()
    {
        var salesPersons = await _context.SalesData
            .Select(x => x.SalesPersonName)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync();

        salesPersons.Insert(0, "All");
        return new SelectList(salesPersons);
    }
}