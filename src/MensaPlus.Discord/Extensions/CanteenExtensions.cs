using MensaPlus.Discord.Converters;

using OpenMensa.Models;

namespace MensaPlus.Discord.Extensions;

internal static class CanteenExtensions
{
    public static Canteen Convert(this Canteen canteen)
    {
        if (CanteenConverter.Converters.TryGetValue(canteen.Id, out CanteenConverter? converter))
        {
            return converter.ConvertCanteen(canteen);
        }
        else
        {
            return canteen;
        }
    }

    public static Meal Convert(this Meal meal, int canteenId)
    {
        if (CanteenConverter.Converters.TryGetValue(canteenId, out CanteenConverter? converter))
        {
            return converter.ConvertMeal(meal);
        }
        else
        {
            return meal;
        }
    }
}
