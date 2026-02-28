using ShopScout.SharedLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopScout.SharedLib.Services
{
    public static class LayoutExtensions
    {
        public static LayoutDto ToDto(this LayoutObject obj)
        {
            return new LayoutDto
            {
                Entrance = new EntranceDto
                {
                    X1 = obj.EntranceX1,
                    Y1 = obj.EntranceY1,
                    X2 = obj.EntranceX2,
                    Y2 = obj.EntranceY2
                },
                Wall = obj.Walls.Select(w => new WallDto
                {
                    Id = w.Id.ToString(),
                    X1 = w.X1,
                    Y1 = w.Y1,
                    X2 = w.X2,
                    Y2 = w.Y2
                }).ToList(),
                Shelf = obj.Shelves.Select(s => s.ToDto()).ToList(),
            };
        }

        public static LayoutObject ToEntity(this LayoutDto dto, int storeId)
        {
            return new LayoutObject
            {
                StoreId = storeId,
                EntranceX1 = dto.Entrance.X1,
                EntranceY1 = dto.Entrance.Y1,
                EntranceX2 = dto.Entrance.X2,
                EntranceY2 = dto.Entrance.Y2,
                Walls = dto.Wall.Select(w => new Wall
                {
                    Id = int.TryParse(w.Id?.ToString(), out var id) ? id : 0,
                    X1 = w.X1,
                    Y1 = w.Y1,
                    X2 = w.X2,
                    Y2 = w.Y2
                }).ToList(),
                Shelves = dto.Shelf.Select(s => s.ToEntity()).ToList(),
            };
        }
    }
}
