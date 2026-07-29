using CouchDB.Driver.Types;
using CouchDb.Domain.Enums;
using Newtonsoft.Json;

namespace Domain.CouchDbModels
{
    /// <summary>
    /// Quản lý trạng thái của workflow 
    /// </summary>
    public class WorkflowData : CouchDocument
    {
        [JsonProperty("workflowId")]
        public string WorkflowId { get; set; }

        [JsonProperty("deliveryTag")] //for rabbitmq
        public ulong DeliveryTag { get; set; }
        [JsonProperty("currentStepId")]
        public List<string> CurrentStepId { get; set; }
        [JsonProperty("workflowName")]
        public string WorkflowName { get; set; }

        [JsonProperty("workflowState")]
        public EWorkflowDataStatus WorkflowState { get; set; }  // Pending/Running/Done/Failed/Reseting/Reseted

        [JsonProperty("isCompleted")]
        public bool IsCompleted { get; set; }

        [JsonProperty("side")]
        public int Side { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
        [JsonProperty("orderId")]
        public string OrderId { get; set; }
        [JsonProperty("productId")]
        public string ProductId { get; set; }

        [JsonProperty("steps")]
        public List<StepData> Steps { get; set; } = new List<StepData>();
    }
}
