using System.ComponentModel.DataAnnotations;

namespace AutomaticBrewingCoffee.Domain.Enums;

public enum EWorkflowType
{
    
    /// <summary>
    /// The main workflow to make a drink in kiosk
    /// </summary>
    [Display(Name = "Activity")] Activity = 0,

    /// <summary>
    /// The handle workflow if error occur in kiosk
    /// </summary>
    [Display(Name = "Callback")] Callback = 1,
    
    /// <summary>
    /// The handle workflow if error occur in kiosk
    /// </summary>
    [Display(Name = "Clean")] Clean = 2,
}