// Services/DummyDataService.cs
public class DummyDataService : IDummyDataService
{
    private readonly List<SalesData> _salesData;

    public DummyDataService()
    {
        _salesData = GenerateDummyData();
    }

    public List<SalesData> GetSalesData()
    {
        return _salesData;
    }

    private List<SalesData> GenerateDummyData()
    {
        var random = new Random();
        var categories = new[] { "Electronics", "Furniture", "Clothing", "Books", "Home & Garden" };
        var regions = new[] { "Jakarta", "Surabaya", "Bandung", "Medan", "Makassar", "Semarang" };
        var salesPersons = new[] { "John Doe", "Jane Smith", "Bob Wilson", "Alice Johnson", "Mike Brown", "Sarah Davis" };
        var products = new Dictionary<string, string[]>
        {
            ["Electronics"] = new[] { "Laptop Dell", "iPhone 15", "Samsung TV", "Sony Headphones", "iPad Pro", "Gaming Mouse", "Mechanical Keyboard", "Monitor 27 inch", "Tablet Android", "Smartwatch" },
            ["Furniture"] = new[] { "Office Chair", "Wooden Desk", "Sofa 3 Seater", "Dining Table", "Bookshelf", "Storage Cabinet", "Bed Frame", "Wardrobe", "Coffee Table", "Study Chair" },
            ["Clothing"] = new[] { "Dress Shirt", "Casual Jeans", "Sports Jacket", "Running Shoes", "Winter Coat", "Summer Dress", "Formal Suit", "Polo Shirt", "Sneakers", "Leather Jacket" },
            ["Books"] = new[] { "Programming Guide", "Business Strategy", "Self Help Book", "History Novel", "Science Fiction", "Cookbook", "Art Book", "Travel Guide", "Biography", "Educational Textbook" },
            ["Home & Garden"] = new[] { "Garden Tools", "Kitchen Set", "Dining Set", "Plant Pot", "Lawn Mower", "Garden Hose", "Outdoor Light", "BBQ Grill", "Patio Furniture", "Flower Seeds" }
        };

        var salesData = new List<SalesData>();

        // Generate data for the last 12 months
        var startDate = DateTime.Now.AddMonths(-12);
        
        for (int i = 0; i < 500; i++) // Generate 500 dummy records
        {
            var category = categories[random.Next(categories.Length)];
            var productList = products[category];
            var product = productList[random.Next(productList.Length)];
            
            var saleDate = startDate.AddDays(random.Next(365));
            
            // Generate amount based on category
            decimal baseAmount = category switch
            {
                "Electronics" => random.Next(500000, 20000000), // 500k - 20M
                "Furniture" => random.Next(300000, 8000000),    // 300k - 8M
                "Clothing" => random.Next(100000, 2000000),     // 100k - 2M
                "Books" => random.Next(50000, 500000),          // 50k - 500k
                "Home & Garden" => random.Next(200000, 3000000), // 200k - 3M
                _ => random.Next(100000, 1000000)
            };

            salesData.Add(new SalesData
            {
                Id = i + 1,
                ProductName = product,
                Category = category,
                Region = regions[random.Next(regions.Length)],
                Amount = baseAmount,
                SaleDate = saleDate,
                SalesPersonName = salesPersons[random.Next(salesPersons.Length)]
            });
        }

        return salesData.OrderByDescending(x => x.SaleDate).ToList();
    }
}