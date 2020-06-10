using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Spice.Extensions
{
    public static class IEnumrableExtension
    {
        public static IEnumerable<SelectListItem> ToSelectListIteams<T> (this IEnumerable<T> items , int selectedValue)
        {
            return from item in items
                   select new SelectListItem
                   {
                       Text = item.GetPropertyValue<T>("Name"),
                       Value = item.GetPropertyValue<T>("Id"),
                       Selected = item.GetPropertyValue<T>("Id").Equals(selectedValue.ToString())
                   };
        } 
    }
}
