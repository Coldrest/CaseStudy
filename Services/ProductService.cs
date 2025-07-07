using CaseStudy.Models;
using System.Text.Json;

namespace CaseStudy.Services
{
    public class ProductService
    {
        private readonly IConfiguration _config;
        private readonly string _dataPath = "Data/products.json";

        public ProductService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<IEnumerable<object>> GetProductsAsync(
            double? minPrice = null,
            double? maxPrice = null,
            double? minPopularity = null,
            double? maxPopularity = null)
        {
            var json = await File.ReadAllTextAsync(_dataPath);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var products = JsonSerializer.Deserialize<List<Product>>(json, options);

            double goldPrice = await GetGoldPriceAsync();

            var projectedProducts = products.Select(p =>
            {
                double popularityOutOfFive = Math.Round(p.PopularityScore * 5, 1);
                double price = Math.Round((p.PopularityScore + 1) * p.Weight * goldPrice, 2);

                return new
                {
                    p.Name,
                    p.PopularityScore,
                    PopularityOutOfFive = popularityOutOfFive,
                    p.Weight,
                    Price = price,
                    p.Images
                };
            });

            if (minPrice.HasValue)
                projectedProducts = projectedProducts.Where(p => p.Price >= minPrice.Value);
            if (maxPrice.HasValue)
                projectedProducts = projectedProducts.Where(p => p.Price <= maxPrice.Value);
            if (minPopularity.HasValue)
                projectedProducts = projectedProducts.Where(p => p.PopularityScore >= minPopularity.Value);
            if (maxPopularity.HasValue)
                projectedProducts = projectedProducts.Where(p => p.PopularityScore <= maxPopularity.Value);

            return projectedProducts;
        }

        private async Task<double> GetGoldPriceAsync()
        {
            var apiKey = _config["GoldApi:ApiKey"];

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://www.goldapi.io/api/XAU/USD"),
            };

            request.Headers.Add("x-access-token", apiKey);
            request.Headers.Add("Accept", "application/json");

            using var client = new HttpClient();
            var response = await client.SendAsync(request);

            var rawContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to retrieve gold price. Status: {response.StatusCode}, Response: {rawContent}");
            }

            using var doc = JsonDocument.Parse(rawContent);
            var pricePerOunce = doc.RootElement.GetProperty("price").GetDouble();

            return pricePerOunce / 31.1035;
        }
    }
}
