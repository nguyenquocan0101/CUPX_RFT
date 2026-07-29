
using Newtonsoft.Json;

namespace WorkflowExecutorTest
{
    public partial class Workflow
    {
        [JsonProperty("workflowId")]
        public string WorkflowId { get; set; } 

        [JsonProperty("productId")]
        public string? ProductId { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("steps")]
        public virtual ICollection<Step> Steps { get; set; } = new List<Step>();
    }


    public partial class Step
    {
        [JsonProperty("stepId")]
        public string StepId { get; set; } = null!;

        [JsonProperty("workflowId")]
        public string? WorkflowId { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("function")]
        public string Function { get; set; }

        [JsonProperty("deviceModelId")]
        public string DeviceModelId { get; set; }

        [JsonProperty("sequence")]
        public int Sequence { get; set; }

        [JsonProperty("maxRetries")]
        public int? MaxRetries { get; set; }

        [JsonProperty("callbackWorkflowId")]
        public string? CallbackWorkflowId { get; set; }

        [JsonProperty("parameters")]
        public string? Parameters { get; set; }
    }


    public partial class Device
    {
        [JsonProperty("deviceId")]
        public string DeviceId { get; set; } 

        [JsonProperty("deviceModelId")]
        public string? DeviceModelId { get; set; }

        [JsonProperty("serialNumber")]
        public string SerialNumber { get; set; } = null!;

        [JsonProperty("name")]
        public string Name { get; set; } = null!;

        [JsonProperty("description")]
        public string Description { get; set; } = null!;

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("x")]
        public decimal X { get; set; }

        [JsonProperty("y")]
        public decimal Y { get; set; }

        [JsonProperty("z")]
        public decimal Z { get; set; }

        [JsonProperty("rx")]
        public decimal RX { get; set; }

        [JsonProperty("ry")]
        public decimal RY { get; set; }

        [JsonProperty("rz")]
        public decimal RZ { get; set; }

        [JsonProperty("j1")]
        public decimal J1 { get; set; }

        [JsonProperty("j2")]
        public decimal J2 { get; set; }

        [JsonProperty("j3")]
        public decimal J3 { get; set; }

        [JsonProperty("j4")]
        public decimal J4 { get; set; }

        [JsonProperty("j5")]
        public decimal J5 { get; set; }

        [JsonProperty("j6")]
        public decimal J6 { get; set; }
    }

}
