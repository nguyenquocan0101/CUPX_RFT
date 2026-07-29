using System.ComponentModel.DataAnnotations;

namespace AutomaticBrewingCoffee.Domain.Enums;

public enum EWebhookType
{
    /// <summary>
    /// To sync data to kiosk
    /// </summary>
    [Display(Name = "SynchronizedData")] SynchronizedData = 0,

    /// <summary>
    /// To order kiosk make a product
    /// </summary>
    [Display(Name = "ExecuteProduct")] ExecuteProduct = 1,

    /// <summary>
    /// To order kiosk make a product
    /// </summary>
    [Display(Name = "RetrieveDevice")] RetrieveDevice = 2,

    /// <summary>
    /// To sync the whole data of kiosk from beginning
    /// </summary>
    [Display(Name = "OverriddenData")] OverriddenData = 3,

    /// <summary>
    /// To clean the kiosk after working
    /// </summary>
    [Display(Name = "ExecuteClean")] ExecuteClean = 4,

    /// <summary>
    /// To ping the kiosk for healthcheck
    /// </summary>
    [Display(Name = "HealthCheck")] HealthCheck = 5,
}