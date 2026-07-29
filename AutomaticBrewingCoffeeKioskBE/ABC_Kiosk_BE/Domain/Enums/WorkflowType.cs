using System.ComponentModel.DataAnnotations;

namespace Domain.Enums;

public enum WorkflowType
{
    
    /// <summary>
    /// The main workflow to make a drink in kiosk
    /// </summary>
    [Display(Name = "Activity")] Activity = 0,

    /// <summary>
    /// The handle workflow if error occur in kiosk
    /// </summary>
    [Display(Name = "Callback")] Callback = 1,

    [Display(Name = "Clean")] Clean = 2,
}