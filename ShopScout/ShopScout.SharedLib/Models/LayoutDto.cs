using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopScout.SharedLib.Models
{
    public class LayoutDto
    {
        public EntranceDto Entrance { get; set; }
        public List<WallDto> Wall { get; set; }
        public List<ShelfDto> Shelf { get; set; }
    }

    public class EntranceDto
    {
        public int X1 { get; set; }
        public int Y1 { get; set; }
        public int X2 { get; set; }
        public int Y2 { get; set; }
        public string? WallId { get; set; }
    }

    public class WallDto
    {
        public string Id { get; set; }
        public int X1 { get; set; }
        public int Y1 { get; set; }
        public int X2 { get; set; }
        public int Y2 { get; set; }
    }

    public class ShelfDto
    {
        public string Id { get; set; }
        public int X1 { get; set; }
        public int Y1 { get; set; }
        public int X2 { get; set; }
        public int Y2 { get; set; }
        public ShelfType Type { get; set; }
        public ShelfSide Side { get; set; }
        public List<ProductPerStoreDto> Products { get; set; } = new List<ProductPerStoreDto>();
    }

    public class ProductPerStoreDto
    {
        public int ProductId { get; set; }
        public int StoreId { get; set; }
        public int? Price { get; set; }
        public int? DiscountedPrice { get; set; }
        public bool Verified { get; set; }
        public int? ShelfId { get; set; }
        public float? DistanceFromP1 { get; set; }
    }

    public static class ShelfExtensions
    {
        public static ProductPerStoreDto ToDto(this ProductPerStore pps)
        {
            return new ProductPerStoreDto
            {
                ProductId = pps.ProductId,
                StoreId = pps.StoreId,
                Price = pps.Price,
                DiscountedPrice = pps.DiscountedPrice,
                Verified = pps.Verified,
                ShelfId = pps.ShelfId,
                DistanceFromP1 = pps.DistanceFromP1
            };
        }

        public static ProductPerStore ToEntity(this ProductPerStoreDto ppsDto)
        {
            return new ProductPerStore
            {
                ProductId = ppsDto.ProductId,
                StoreId = ppsDto.StoreId,
                Price = ppsDto.Price,
                DiscountedPrice = ppsDto.DiscountedPrice,
                Verified = ppsDto.Verified,
                ShelfId = ppsDto.ShelfId,
                DistanceFromP1 = ppsDto.DistanceFromP1
            };
        }

        public static ShelfDto ToDto(this Shelf s)
        {
            return new ShelfDto
            {
                Id = s.Id.ToString(),
                X1 = s.X1,
                Y1 = s.Y1,
                X2 = s.X2,
                Y2 = s.Y2,
                Type = s.Type,
                Side = s.Side,
                Products = s.Products?.Select(x => x.ToDto()).ToList() ?? new List<ProductPerStoreDto>()
            };
        }

        public static Shelf ToEntity(this ShelfDto s, Shelf? existing = null)
        {
            var entity = existing ?? new Shelf();
            entity.Id = int.TryParse(s.Id?.ToString(), out var id) ? id : 0;
            entity.X1 = s.X1;
            entity.Y1 = s.Y1;
            entity.X2 = s.X2;
            entity.Y2 = s.Y2;
            entity.Type = s.Type;
            entity.Side = s.Side;
            return entity;
        }

        public static Shelf UpdateFromDto(this Shelf shelf, ShelfDto dto)
        {
            shelf.X1 = dto.X1;
            shelf.Y1 = dto.Y1;
            shelf.X2 = dto.X2;
            shelf.Y2 = dto.Y2;
            shelf.Type = dto.Type;
            shelf.Side = dto.Side;
            return shelf;
        }
    }
}
