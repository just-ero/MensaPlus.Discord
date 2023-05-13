using System.Collections.Generic;

using OpenMensa.Models;

namespace MensaPlus.Discord.Converters;

public abstract class CanteenConverter
{
    public virtual Canteen ConvertCanteen(Canteen canteen)
    {
        return canteen;
    }

    public virtual Meal ConvertMeal(Meal meal)
    {
        return meal;
    }

    public virtual Prices ConvertPrices(Prices prices)
    {
        return prices;
    }

    public static Dictionary<int, CanteenConverter> Converters { get; } = new()
    {
        [78] = new DresdenZeltschloesschenConverter(),
        [79] = new DresdenAlteMensaConverter(),
        [80] = new DresdenMensaMatrixConverter(),
        [82] = new DresdenSiedepunktConverter(),
    };
}
