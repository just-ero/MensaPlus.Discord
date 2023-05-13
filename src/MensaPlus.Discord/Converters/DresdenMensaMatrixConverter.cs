using System.Linq;
using System.Text.RegularExpressions;

using OpenMensa.Models;

namespace MensaPlus.Discord.Converters;

public sealed partial class DresdenMensaMatrixConverter : CanteenConverter
{
    public override Canteen ConvertCanteen(Canteen canteen)
    {
        return new(
            canteen.Id,
            "Mensa Matrix",
            "Dresden",
            canteen.Address,
            canteen.Coordinates);
    }

    public override Meal ConvertMeal(Meal meal)
    {
        string name = GetMealNameRegex().Replace(meal.Name, "");

        return new(
            meal.Id,
            name,
            meal.Category,
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
