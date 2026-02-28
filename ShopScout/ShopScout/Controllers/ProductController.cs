using Microsoft.AspNetCore.Mvc;
using ShopScout.SharedLib.Models;
using ShopScout.SharedLib.Services;

namespace ShopScout.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet("{barcode}")]
        public async Task<ActionResult<Product>> GetProductByBarcode(string barcode)
        {
            var product = await _productService.GetProductAsync(barcode);
            return Ok(product);
        }

        [HttpGet("{page:int}")]
        public async Task<ActionResult<List<Product>>> GetAllProducts(int page = 1)
        {
            var product = await _productService.GetAllProductsAsync(page);
            return Ok(product);
        }

        [HttpGet("search/{search_term}/page/{page:int}")]
        public async Task<ActionResult<List<Product>>> Search(string search_term, int page = 1)
        {
            var product = await _productService.GetProductsSearchAsync(search_term, page);
            return Ok(product);
        }

        [HttpPost("filter/page/{page:int}")]
        public async Task<ActionResult<List<Product>>> FilteredSearch(int page, [FromBody] ProductFilterParams filters)
        {
            var result = await _productService.GetProductsFilteredAsync(null, filters, page);
            return Ok(result);
        }

        [HttpPost("filter/{search_term}/page/{page:int}")]
        public async Task<ActionResult<List<Product>>> FilteredSearchWithTerm(string search_term, int page, [FromBody] ProductFilterParams filters)
        {
            var result = await _productService.GetProductsFilteredAsync(search_term, filters, page);
            return Ok(result);
        }
    }
}