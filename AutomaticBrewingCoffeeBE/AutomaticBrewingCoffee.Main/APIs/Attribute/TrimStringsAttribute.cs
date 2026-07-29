using System.Collections;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AutomaticBrewingCoffee.API.Attribute;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class TrimStringsAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument == null) continue;

            var type = argument.GetType();

            // Chỉ xử lý class thường (không xử lý primitive types)
            if (!type.IsClass || type == typeof(string)) continue;

            TrimAllStringsRecursive(argument);
        }

        base.OnActionExecuting(context);
    }

    private void TrimStringProperties(object obj)
    {
        var props = obj.GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => p.CanRead && p.CanWrite && p.PropertyType == typeof(string));

        foreach (var prop in props)
        {
            var val = (string?)prop.GetValue(obj);
            if (val != null)
            {
                var trimmed = val.Trim();
                prop.SetValue(obj, trimmed);
            }
        }
    }

    private void TrimAllStringsRecursive(object? obj)
    {
        if (obj == null) return;

        var props = obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => p.CanRead && p.CanWrite && p.GetIndexParameters().Length == 0);

        foreach (var prop in props)
        {
            var type = prop.PropertyType;
            var value = prop.GetValue(obj);
            if (value == null) continue;

            if (type == typeof(string))
            {
                var trimmed = ((string)value).Trim();
                prop.SetValue(obj, trimmed);
            }
            else if (typeof(IEnumerable<object>).IsAssignableFrom(type) && type != typeof(string))
            {
                // Xử lý danh sách
                var enumerable = value as IEnumerable;
                if (enumerable != null)
                {
                    foreach (var item in enumerable)
                    {
                        TrimAllStringsRecursive(item); // đệ quy cho từng phần tử trong danh sách
                    }
                }
            }
            else if (type.IsClass && type != typeof(string))
            {
                TrimAllStringsRecursive(value); // đệ quy cho object con
            }
        }
    }

}