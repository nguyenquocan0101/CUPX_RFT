using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Domain.Enums
{
    /// <summary>
    /// Defines standard units of measurement for ingredients, especially in coffee making.
    /// </summary>
    public enum IngredientUnit
    {
        /// <summary>
        /// No unit specified or unit is not applicable.
        /// </summary>
        [Display(Name = "None")] 
        None = 0,

        /// <summary>
        /// Grams (g) - Commonly used for weight (coffee beans/grounds, sugar).
        /// </summary>
        [Display(Name = "Gram (g)")]
        Gram = 1,

        /// <summary>
        /// Milliliters (ml) - Commonly used for volume of liquids (water, milk, syrups).
        /// </summary>
        [Display(Name = "Milliliter (ml)")]
        Milliliter = 2,

        /// <summary>
        /// Countable units (pods, packets, ice cubes).
        /// </summary>
        [Display(Name = "Đơn vị / Cái")]
        Unit = 3,

    }
}