using System.Collections.Generic;
using System;
using System.Linq;
using System.Text.RegularExpressions;

using OpenMensa.Models;
using Humanizer;

namespace MensaPlus.Discord.Converters;

public sealed partial class DresdenSiedepunktConverter : CanteenConverter
{
    public override Canteen ConvertCanteen(Canteen canteen)
    {
        return new(
            canteen.Id,
            "Siedepunkt",
            "Dresden",
            canteen.Address,
            canteen.Coordinates);
    }

    public override Meal ConvertMeal(Meal meal)
    {
        string name = GetMealNameRegex().Replace(meal.Name, "");
        string category = meal.Category.Titleize();

        if (category.StartsWith("Abendangebot "))
        {
            name += " (Abendangebot)";
            category = category[13..];
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
