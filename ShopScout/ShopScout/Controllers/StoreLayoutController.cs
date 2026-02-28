using Microsoft.AspNetCore.Mvc;
using ShopScout.SharedLib.Models;
using ShopScout.SharedLib.Services;

namespace ShopScout.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StoreLayoutController : ControllerBase
    {
        private readonly IStoreLayoutService _storeLayoutService;
        public StoreLayoutController(IStoreLayoutService storeLayoutService)
        {
            _storeLayoutService = storeLayoutService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetStore(int id)
        {
            var layout = await _storeLayoutService.GetStoreAsync(id);
            if (layout == null)
            {
                return NotFound();
            }
            return Ok(layout);
        }

        [HttpPost("{id}")]
        public async Task<Store> StoreLayout([FromBody] LayoutDto layout, int id)
        {
            return await _storeLayoutService.StoreWalls(layout, id);
        }

        [HttpPut("{id}/shelf")]
        public async Task<ActionResult<Shelf>> UpdateShelf([FromBody] ShelfDto shelf, int id)
        {
            try
            {
                return Ok(await _storeLayoutService.UpdateShelf(shelf, id));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        public record ProductToShelfResponse(Store Store, ShelfDto Shelf);
        public record AddProductData(ProductPerStoreDto Pps, float D);

        [HttpPost("shelf/{shelfId}/product")]
        public async Task<ActionResult<ProductToShelfResponse>> AddProductToShelf([FromBody] AddProductData productData, int shelfId)
        {
            try
            {
                var ppsEntity = productData.Pps.ToEntity();
                var updatedShelf = await _storeLayoutService.AddProductToShelf(ppsEntity, shelfId, productData.D);
                return Ok(new ProductToShelfResponse(updatedShelf.store, updatedShelf.shelf));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("shelf/{shelfId}/product/remove")]
        public async Task<ActionResult<ProductToShelfResponse>> RemoveProductFromShelf([FromBody] ProductPerStoreDto productPerStore, int shelfId)
        {
            try
            {
                var ppsEntity = productPerStore.ToEntity();
                var updatedShelf = await _storeLayoutService.RemoveProductFromShelf(ppsEntity, shelfId);
                return Ok(new ProductToShelfResponse(updatedShelf.store, updatedShelf.shelf));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("shelf/{shelfId}")]
        public async Task<ActionResult<Shelf>> GetShelf(int shelfId)
        {
            try
            {
                var shelf = await _storeLayoutService.GetShelfAsync(shelfId);
                if (shelf == null)
                    return NotFound();
                return Ok(shelf);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}