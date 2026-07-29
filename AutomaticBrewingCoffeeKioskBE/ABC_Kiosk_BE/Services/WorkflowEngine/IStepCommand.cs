using Domain.Models;

namespace Services.WorkflowEngine
{
    public interface IStepCommand
    {
        Task ExecuteAsync(Step step);
        public string SerializeParameters<T>(T paramtersObject);
        public T? DeserializeParameters<T>(string paramters);

    }
}
