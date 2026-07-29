
using CouchDb.Domain.Enums;
using CouchDB.Driver.Types;
using Newtonsoft.Json;
using Domain.Models;
using CouchDb.Domain;

namespace Domain.CouchDbModels
{
    /// <summary>
    /// Quản lý trạng thái data của step trong quá trình thực hiện workflow.
    /// </summary>
    public class StepData
    {
        [JsonProperty("step")]
        public Step Step { get; set; }

        [JsonProperty("state")]
        public EStepDataStatus State { get; set; }  // Pending/Done/Failed

        [JsonProperty("isRunCallBack")]
        public bool IsRunCallBack { get; set; } 

        [JsonProperty("observed")]
        public bool Observed { get; set; } = false;
        [JsonProperty("callbackObserved")]
        public bool CallbackObserved { get; set; } = false;
        [JsonProperty("executor")]
        public string Executor { get; set; } = string.Empty; //deviceId

    }
}
