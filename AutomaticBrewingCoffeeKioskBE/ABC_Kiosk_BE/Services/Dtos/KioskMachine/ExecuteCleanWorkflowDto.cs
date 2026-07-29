
using System.ComponentModel.DataAnnotations;

namespace Services.Dtos.KioskMachine
{
    public class ExecuteCleanWorkflowDto
    {
        [Required]
        public Guid WorkflowId { get; set; }

    }
}
