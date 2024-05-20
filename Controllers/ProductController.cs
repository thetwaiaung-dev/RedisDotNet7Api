using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedisDotNet7API.Models;
using System.Collections;

namespace RedisDotNet7API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly CacheService _cacheService;
        private static object _locker = new object();

        public ProductController(AppDbContext dbContext, CacheService cacheService)
        {
            _dbContext = dbContext;
            _cacheService = cacheService;
        }

        [HttpGet]
        public IActionResult GetList()
        {
            var productsCache = _cacheService.GetData<IEnumerable<ProductModel>>("productList");
            if (productsCache != null)
            {
                return Ok(productsCache);
            }
            lock (_locker)
            {
                var expirationTime = DateTimeOffset.Now.AddMinutes(5);
                productsCache = _dbContext.Products.AsNoTracking().ToList();

                _cacheService.SetData<IEnumerable<ProductModel>>("productList", productsCache, expirationTime);
            }
            return Ok(productsCache);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            ProductModel product = new();

            var productsCache = _cacheService.GetData<IEnumerable<ProductModel>>("productList");
            if (productsCache is not null)
            {
                product = productsCache.FirstOrDefault(x => x.Id == id)!;

                return Ok(product);
            }

            product = _dbContext.Products.FirstOrDefault(x => x.Id == id)!;
            return Ok(product);
        }


        [HttpPost]
        [Route("/create")]
        public IActionResult Create(ProductRequestModel request)
        {
            var model = new ProductModel()
            {
                Id = request.Id,
                Name = request.Name,
                Description = request.Description,
                Stock = request.Stock
            };
            _dbContext.Products.Add(model);
            var result = _dbContext.SaveChanges();
            _cacheService.RemoveData("productList");

            var responseMessage = result > 0 ? "Success" : "Failed";
            return Ok(responseMessage);
        }

        [HttpPost]
        [Route("/update")]
        public IActionResult Update(ProductRequestModel request)
        {
            var model = new ProductModel()
            {
                Id = request.Id,
                Name = request.Name,
                Description = request.Description,
                Stock = request.Stock
            };

            _dbContext.Products.Update(model);
            var result = _dbContext.SaveChanges();
            _cacheService.RemoveData("productList");

            var responseMessage = result > 0 ? "Success" : "Failed";
            return Ok(responseMessage);
        }

    }
}
