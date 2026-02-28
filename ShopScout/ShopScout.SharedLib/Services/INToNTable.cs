using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopScout.SharedLib.Services;

public interface INToNTable
{
    public string Name { get; set; }
}

public interface IVerifiable
{
    public bool Verified { get; set; }
}
