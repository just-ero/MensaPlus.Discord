using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using OpenMensa.Models;

namespace MensaPlus.Discord.Converters;

public sealed partial class DresdenAlteMensaConverter : CanteenConverter
{
    private static readonly Dictionary<string, string> s_categories = new(StringComparer.OrdinalIgnoreCase)
    {
        ["fertig"] = "Fertiggerichte",
        ["GRILL"] = "Grill",
        ["Pastaria/WOK"] = "Pasta"
    };

    public override Canteen ConvertCanteen(Canteen canteen)
    {
        return new(
            canteen.Id,
            "Alte Mensa",
            "Dresden",
            canteen.Address,
            canteen.Coordinates);
    }

    public override Meal ConvertMeal(Meal meal)
    {
        string name = GetMealNameRegex().Replace(meal.Name, "");

        string category = meal.Category.Split(' ').First();
        if (s_categories.TryGetValue(category, out string? newCategory))
        {
            category = newCategory;
        }

        return new(
            meal.Id,
            name,
            category,
            ConvertPrices(meal.Prices),
            meal.Notes.Distinct().ToArray());
    }

    public override Prices ConvertPrices(Prices prices)
    {
        return new(
            prices.Students,
            prices.Employees,
            prices.Pupils,
            prices.Employees.GetValueOrDefault() * 1.20);
    }

    [GeneratedRegex(@",?\s?\([^)]*\)")]
    private static partial Regex GetMealNameRegex();
}
