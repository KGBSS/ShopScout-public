using Microsoft.AspNetCore.Mvc;
using ShopScout.SharedLib.Models;
using ShopScout.SharedLib.Services;

namespace ShopScout.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StoreController : Controller
    {
        private readonly IStoreService _storeService;
        public StoreController(IStoreService storeService)
        {
            _storeService = storeService;
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Store>> GetStoreAsync(int id)
        {
            var store = await _storeService.GetStoreAsync(id);
            return Ok(store);
        }

        [HttpGet("search/{searchText}")]
        public async Task<ActionResult<Store>> Search(string searchText)
        {
            var store = await _storeService.SearchStore(searchText);
            return Ok(store);
        }
    }
}
