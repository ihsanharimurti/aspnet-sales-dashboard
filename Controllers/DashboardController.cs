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

    public IActionResult Index(string category = "", string region = "", string salesperson = "", 
        DateTime? fromDate = null, DateTime? toDate = null, int page = 1, int pageSize = 10)
    {
        // Get and filter data
        var allData = _dataService.GetSalesData();
        var query = allData.AsQueryable();

        if (!string.IsNullOrEmpty(category) && category != "All")
            query = query.Where(x => x.Category == category);

        if (!string.IsNullOrEmpty(region) && region != "All")
            query = query.Where(x => x.Region == region);

        if (!string.IsNullOrEmpty(salesperson) && salesperson != "All")
            query = query.Where(x => x.SalesPersonName == salesperson);

        if (fromDate.HasValue)
            query = query.Where(x => x.SaleDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(x => x.SaleDate <= toDate.Value);

        var filteredData = query.OrderByDescending(x => x.SaleDate).ToList();

        // Pagination
        var totalItems = filteredData.Count;
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        var items = filteredData.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        // ViewBag data
        ViewBag.Categories = GetSelectList(allData.Select(x => x.Category).Distinct().OrderBy(x => x).ToList());
        ViewBag.Regions = GetSelectList(allData.Select(x => x.Region).Distinct().OrderBy(x => x).ToList());
        ViewBag.SalesPersons = GetSelectList(allData.Select(x => x.SalesPersonName).Distinct().OrderBy(x => x).ToList());
        ViewBag.PageSizes = new SelectList(new[] { 10, 25, 50, 100 }, pageSize);

        ViewBag.CurrentCategory = category;
        ViewBag.CurrentRegion = region;
        ViewBag.CurrentSalesperson = salesperson;
        ViewBag.CurrentFromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.CurrentToDate = toDate?.ToString("yyyy-MM-dd");

        ViewBag.TotalSales = filteredData.Sum(x => x.Amount);
        ViewBag.AverageSale = filteredData.Any() ? filteredData.Average(x => x.Amount) : 0;
        ViewBag.TotalTransactions = filteredData.Count;

        ViewBag.CurrentPage = page;
        ViewBag.PageSize = pageSize;
        ViewBag.TotalPages = totalPages;
        ViewBag.TotalItems = totalItems;
        ViewBag.HasPrevious = page > 1;
        ViewBag.HasNext = page < totalPages;

        return View(items);
    }

    [HttpPost]
    public IActionResult GetChartData(string category = "", string region = "", string salesperson = "", 
        DateTime? fromDate = null, DateTime? toDate = null)
    {
        var allData = _dataService.GetSalesData();
        var query = allData.AsQueryable();

        if (!string.IsNullOrEmpty(category) && category != "All")
            query = query.Where(x => x.Category == category);

        if (!string.IsNullOrEmpty(region) && region != "All")
            query = query.Where(x => x.Region == region);

        if (!string.IsNullOrEmpty(salesperson) && salesperson != "All")
            query = query.Where(x => x.SalesPersonName == salesperson);

        if (fromDate.HasValue)
            query = query.Where(x => x.SaleDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(x => x.SaleDate <= toDate.Value);

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

    private SelectList GetSelectList(List<string> items)
    {
        var list = new List<string> { "All" };
        list.AddRange(items);
        return new SelectList(list);
    }
}