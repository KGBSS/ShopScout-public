using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ShopScout.SharedLib.Services
{
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            if (field == null)
            {
                throw new ArgumentException("Enum value not found", nameof(value));
            }
            var attribute = field.GetCustomAttribute<DisplayAttribute>();
            if (attribute == null)
            {
                return value.ToString();
            }
            return attribute.Name ?? value.ToString();
        }
    }
}
