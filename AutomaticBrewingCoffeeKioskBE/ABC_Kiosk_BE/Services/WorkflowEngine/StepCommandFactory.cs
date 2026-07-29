using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Services.WorkflowEngine
{
    public class StepCommandFactory
    {
        private readonly Dictionary<string, Type> _commands;
        private readonly IServiceProvider _serviceProvider;

        public StepCommandFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _commands = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => typeof(IStepCommand).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .Select(t => new
                {
                    Type = t,
                    Attribute = t.GetCustomAttribute<StepTypeAttribute>()
                })
                .Where(x => x.Attribute != null)
                .ToDictionary(x => x.Attribute.TypeName, x => x.Type);
        }

        public IStepCommand Create(string typeName)
        {
            if (_commands.TryGetValue(typeName, out var type))
            {
                // Use IServiceProvider to initialize dependency
                return (IStepCommand)_serviceProvider.GetRequiredService(type);
            }

            throw new NotSupportedException($"Step '{typeName}' is not supported.");
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class StepTypeAttribute : Attribute
    {
        public string TypeName { get; }
        public StepTypeAttribute(string typeName)
        {
            TypeName = typeName;
        }
    }

}
