using BlazorWebPoc.ApiService.Model;
using BlazorWebPoc.ApiService.Model.Request;
using BlazorWebPoc.ApiService.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlazorWebPoc.ApiService.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        public ProductController(IProductService productService)
        {
            _productService = productService;
        }


        [HttpGet("getAll")]
        public async Task<IActionResult> GetAllProducts()
        {
            var products = await _productService.GetAllProductsAsync();
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }


        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] Model.Product product)
        {
            if (product == null)
            {
                return BadRequest("Invalid product data.");
            }
            var createdProduct = await _productService.CreateProductAsync(product);
            return CreatedAtAction(nameof(GetProductById), new { id = createdProduct.Id }, createdProduct);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] Model.Product product)
        {
            if (product == null || id != product.Id)
            {
                return BadRequest("Invalid product data.");
            }
            var updatedProduct = await _productService.UpdateProductAsync(id, product);
            if (updatedProduct == null)
            {
                return NotFound();
            }
            return Ok(updatedProduct);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var result = await _productService.DeleteProductAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }

        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadProduct([FromForm] ProductUploadDto dto)
        {
            if (dto == null) return BadRequest("Invalid product data.");

            Product response = await _productService.ProductUploadAsync(dto);

            return Ok(response);
        }

        [HttpGet("{id}/image")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetProductImage(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null || product.Image == null)
            {
                return NotFound();
            }
            var imageStream = new MemoryStream(product.Image);
            return File(imageStream, "image/jpeg", $"{product.Name}.jpg");

        }

    }
}
