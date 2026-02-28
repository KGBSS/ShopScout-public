using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopScout.SharedLib.Models
{
    public enum EditorMode
    {
        ReadOnly,
        WallEditor,
        EntranceEditor,
        ShelfEditor,
        ToolEditor,
        ProductEditor
    }

    public enum Tool
    {
        None,
        Wall,
        Entrance,
        Shelf,
        Move,
        Eraser,
        Draw,
        Product
    }
}
