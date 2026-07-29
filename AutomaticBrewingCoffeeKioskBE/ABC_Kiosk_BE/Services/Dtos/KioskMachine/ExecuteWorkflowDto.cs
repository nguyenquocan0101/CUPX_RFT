using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Services.Dtos.KioskMachine
{
    public class ExecuteWorkflowDto
    {
        private int leftSide = 1;
        private int rightSide = 2;

        [Required]
        public string OrderId { get; set; }

        [Required]
        public int Side { get; set; }

        [Required]
        [JsonPropertyName("products")]
        public List<WorkflowInput> WorkflowIds { get; set; } = new();

        public bool IsValidSide() => Side == leftSide || Side == rightSide;
    }

    public class WorkflowInput
    {
        [Required]
        [JsonPropertyName("productId")]
        public string WorkflowId { get; set; }

        public List<StepOption>? Options { get; set; } 
    }

    public class StepOption
    {
        [JsonPropertyName("deviceModelId")]
        public string DeviceModelId { get; set; }
        [JsonPropertyName("target")]
        public string Target { get; set; }
        [JsonPropertyName("value")]
        public double Value { get; set; } //%
    }
}
