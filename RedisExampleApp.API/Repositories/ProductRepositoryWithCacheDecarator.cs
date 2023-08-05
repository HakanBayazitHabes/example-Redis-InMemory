using RedisExampleApp.API.Models;
using RedisExampleApp.Cache;
using StackExchange.Redis;
using System.Text.Json;

namespace RedisExampleApp.API.Repositories
{
    public class ProductRepositoryWithCacheDecarator : IProductRepository
    {
        private const string productKey = "productCaches";
        private readonly IProductRepository _repo;
        private readonly RedisService _redisService;
        private readonly IDatabase _redisDb;

        public ProductRepositoryWithCacheDecarator(IProductRepository repo, RedisService redisService)
        {
            _repo = repo;
            _redisService = redisService;
            _redisDb = _redisService.GetDb(2);
        }

        public async Task<Product> AddAsync(Product product)
        {
            var newProduct = await _repo.AddAsync(product);

            if (await _redisDb.KeyExistsAsync(productKey))
            {
                await _redisDb.HashSetAsync(productKey, newProduct.Id, JsonSerializer.Serialize(newProduct));
            }

            return newProduct;

        }



        public async Task<List<Product>> GetAsync()
        {
            if (!await _redisDb.KeyExistsAsync(productKey))
                return await LoadToCacheFromDbAsync();

            var products = new List<Product>();

            var cacheProducts = await _redisDb.HashGetAllAsync(productKey);
            foreach (var item in cacheProducts.ToList())
            {
                var product = JsonSerializer.Deserialize<Product>(item.Value);
                products.Add(product);
            }

            return products;

        }

        public async Task<Product> GetByIdAsync(int id)
        {
            if (_redisDb.KeyExists(productKey))
            {
                var product = await _redisDb.HashGetAsync(productKey, id);
                return product.HasValue ? JsonSerializer.Deserialize<Product>(product) : null;
            }

            var products = await LoadToCacheFromDbAsync();

            return products.FirstOrDefault(x => x.Id == id);

        }

        private async Task<List<Product>> LoadToCacheFromDbAsync()
        {
            var product = await _repo.GetAsync();

            product.ForEach(x =>
            {
                _redisDb.HashSetAsync(productKey, x.Id, JsonSerializer.Serialize(x));
            });

            return product;

        }
    }
}
