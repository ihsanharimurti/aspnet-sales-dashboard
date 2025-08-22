// Controllers/DashboardController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

public class DashboardController : Controller
{
    private readonly IDummyDataService _dataService;

    public DashboardController(IDummyDataService dataService)
    {
        _dataService = dataService;
    }

    public IActionResult Index(string category = "", string region = "", string salesperson = "", DateTime? fromDate = null, DateTime? toDate = null)
    {
        // Get all data from dummy service
        var allData = _dataService.GetSalesData();

        // Build query with filters
        var query = allData.AsQueryable();

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
        var salesData = query.OrderByDescending(x => x.SaleDate).ToList();

        // Prepare dropdown data
        ViewBag.Categories = GetCategoriesSelectList(allData);
        ViewBag.Regions = GetRegionsSelectList(allData);
        ViewBag.SalesPersons = GetSalesPersonsSelectList(allData);

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
    public IActionResult GetChartData(string category = "", string region = "", string salesperson = "", DateTime? fromDate = null, DateTime? toDate = null)
    {
        var allData = _dataService.GetSalesData();
        var query = allData.AsQueryable();

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
        var chartData = query
            .GroupBy(x => new { x.SaleDate.Year, x.SaleDate.Month })
            .Select(g => new {
                Label = $"{g.Key.Year}-{g.Key.Month:00}",
                Value = g.Sum(x => x.Amount)
            })
            .OrderBy(x => x.Label)
            .ToList();

        return Json(chartData);
    }

    private SelectList GetCategoriesSelectList(List<SalesData> data)
    {
        var categories = data
            .Select(x => x.Category)
            .Distinct()
            .OrderBy(x => x)
            .ToList();

        categories.Insert(0, "All");
        return new SelectList(categories);
    }

    private SelectList GetRegionsSelectList(List<SalesData> data)
    {
        var regions = data
            .Select(x => x.Region)
            .Distinct()
            .OrderBy(x => x)
            .ToList();

        regions.Insert(0, "All");
        return new SelectList(regions);
    }

    private SelectList GetSalesPersonsSelectList(List<SalesData> data)
    {
        var salesPersons = data
            .Select(x => x.SalesPersonName)
            .Distinct()
            .OrderBy(x => x)
            .ToList();

        salesPersons.Insert(0, "All");
        return new SelectList(salesPersons);
    }
}