using Services.Base;
using Services.Dtos.KioskMachine;

namespace Services.Interfaces
{
    public interface IWorkflowService2
    {
        /// <summary>
        ///  Execute workflow by ids including orderId, side. it will push all workflow to rabbitmq queue for execute-product-consumer   
        /// </summary>
        /// <param name="workflowIds"></param>
        /// <param name="orderId"></param>
        /// <returns></returns>
        Task<BaseResult> ExecuteWorkflowAsync(ExecuteWorkflowDto dto);

        /// <summary>
        /// Execute clean workflow, it will push all workflow to rabbitmq queue for execute-product-consumer
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        Task<BaseResult> ExecuteCleanWorkflowAsync(ExecuteCleanWorkflowDto dto);

        /// <summary>
        /// Get all clean workflows from file system, it will return a list of clean workflow
        /// </summary>
        /// <returns></returns>
        Task<BaseResult> GetAllCleanWorkflowsAsync();


        /// <summary>
        /// Get all workflows from file system, it will return a list of workflow
        /// </summary>
        /// <returns></returns>
        Task<BaseResult> GetAllWorkflowsAsync();


    }
}
