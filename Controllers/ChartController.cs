// Controllers/ChartController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

public class ChartController : Controller
{
    private readonly IDummyDataService _dataService;

    public ChartController(IDummyDataService dataService)
    {
        _dataService = dataService;
    }

    public IActionResult Index(string category = "", string region = "", string salesperson = "",
        DateTime? fromDate = null, DateTime? toDate = null, string xAxis = "Month", string yAxis = "Amount")
    {
        var allData = _dataService.GetSalesData();

        // ViewBag for filters
        ViewBag.Categories = GetSelectList(allData.Select(x => x.Category).Distinct().OrderBy(x => x).ToList());
        ViewBag.Regions = GetSelectList(allData.Select(x => x.Region).Distinct().OrderBy(x => x).ToList());
        ViewBag.SalesPersons = GetSelectList(allData.Select(x => x.SalesPersonName).Distinct().OrderBy(x => x).ToList());

        // ViewBag for current filter values
        ViewBag.CurrentCategory = category;
        ViewBag.CurrentRegion = region;
        ViewBag.CurrentSalesperson = salesperson;
        ViewBag.CurrentFromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.CurrentToDate = toDate?.ToString("yyyy-MM-dd");

        // ViewBag for axis options
        ViewBag.XAxisOptions = new SelectList(new[]
        {
            new { Value = "Month", Text = "Month" },
            new { Value = "Category", Text = "Category" },
            new { Value = "Region", Text = "Region" },
            new { Value = "Salesperson", Text = "Salesperson" },
            new { Value = "Product", Text = "Product" }
        }, "Value", "Text", xAxis);

        ViewBag.YAxisOptions = new SelectList(new[]
        {
            new { Value = "Amount", Text = "Sales Amount" },
            new { Value = "Count", Text = "Transaction Count" },
            new { Value = "Average", Text = "Average Sale" }
        }, "Value", "Text", yAxis);

        ViewBag.CurrentXAxis = xAxis;
        ViewBag.CurrentYAxis = yAxis;

        // Chart type options
        ViewBag.ChartTypes = new SelectList(new[]
        {
            new { Value = "line", Text = "Line Chart" },
            new { Value = "bar", Text = "Bar Chart" },
            new { Value = "pie", Text = "Pie Chart" },
            new { Value = "doughnut", Text = "Doughnut Chart" }
        }, "Value", "Text", "line");

        return View();
    }

    [HttpPost]
    public IActionResult GetCustomChartData(string category = "", string region = "", string salesperson = "",
        DateTime? fromDate = null, DateTime? toDate = null, string xAxis = "Month", string yAxis = "Amount")
    {
        var allData = _dataService.GetSalesData();
        var query = allData.AsQueryable();

        // Apply filters
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

        var filteredData = query.ToList();

        // Generate chart data based on X and Y axis selection
        var chartData = GenerateChartData(filteredData, xAxis, yAxis);

        return Json(new
        {
            labels = chartData.Labels,
            values = chartData.Values,
            xAxisLabel = GetAxisLabel(xAxis),
            yAxisLabel = GetAxisLabel(yAxis)
        });
    }

    private (List<string> Labels, List<decimal> Values) GenerateChartData(List<SalesData> data, string xAxis, string yAxis)
    {
        var labels = new List<string>();
        var values = new List<decimal>();

        switch (xAxis.ToLower())
        {
            case "month":
                var monthlyData = data
                    .GroupBy(x => new { x.SaleDate.Year, x.SaleDate.Month })
                    .Select(g => new
                    {
                        Label = $"{g.Key.Year}-{g.Key.Month:00}",
                        Data = g.ToList(),
                        SortKey = new DateTime(g.Key.Year, g.Key.Month, 1)
                    })
                    .OrderBy(x => x.SortKey);

                foreach (var item in monthlyData)
                {
                    labels.Add(item.Label);
                    values.Add(CalculateYValue(item.Data, yAxis));
                }
                break;

            case "category":
                var categoryData = data
                    .GroupBy(x => x.Category)
                    .OrderBy(x => x.Key);

                foreach (var item in categoryData)
                {
                    labels.Add(item.Key);
                    values.Add(CalculateYValue(item.ToList(), yAxis));
                }
                break;

            case "region":
                var regionData = data
                    .GroupBy(x => x.Region)
                    .OrderBy(x => x.Key);

                foreach (var item in regionData)
                {
                    labels.Add(item.Key);
                    values.Add(CalculateYValue(item.ToList(), yAxis));
                }
                break;

            case "salesperson":
                var salespersonData = data
                    .GroupBy(x => x.SalesPersonName)
                    .OrderBy(x => x.Key);

                foreach (var item in salespersonData)
                {
                    labels.Add(item.Key);
                    values.Add(CalculateYValue(item.ToList(), yAxis));
                }
                break;

            case "product":
                var productData = data
                    .GroupBy(x => x.ProductName)
                    .OrderByDescending(x => CalculateYValue(x.ToList(), yAxis))
                    .Take(10); // Limit to top 10 products

                foreach (var item in productData)
                {
                    labels.Add(item.Key);
                    values.Add(CalculateYValue(item.ToList(), yAxis));
                }
                break;
        }

        return (labels, values);
    }

    private decimal CalculateYValue(List<SalesData> data, string yAxis)
    {
        return yAxis.ToLower() switch
        {
            "amount" => data.Sum(x => x.Amount),
            "count" => data.Count,
            "average" => data.Any() ? data.Average(x => x.Amount) : 0,
            _ => data.Sum(x => x.Amount)
        };
    }

    private string GetAxisLabel(string axis)
    {
        return axis.ToLower() switch
        {
            "month" => "Month",
            "category" => "Category",
            "region" => "Region",
            "salesperson" => "Salesperson",
            "product" => "Product",
            "amount" => "Sales Amount ($)",
            "count" => "Transaction Count",
            "average" => "Average Sale ($)",
            _ => axis
        };
    }

    private SelectList GetSelectList(List<string> items)
    {
        var list = new List<string> { "All" };
        list.AddRange(items);
        return new SelectList(list);
    }
}