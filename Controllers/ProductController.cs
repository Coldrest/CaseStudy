using CaseStudy.Services;
using Microsoft.AspNetCore.Mvc;

namespace CaseStudy.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly ProductService _productService;

        public ProductController(ProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] double? minPrice,
            [FromQuery] double? maxPrice,
            [FromQuery] double? minPopularity,
            [FromQuery] double? maxPopularity)
        {
            var products = await _productService.GetProductsAsync(
                minPrice, maxPrice, minPopularity, maxPopularity);

            return Ok(products);
        }
    }
}
