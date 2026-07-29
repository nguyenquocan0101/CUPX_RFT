using Domain.Models;
using Repositories.Interfaces;
using Services.Interfaces;
using Shared.MessageStore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services.WorkflowEngine
{
    public class WorkflowExecutor
    {
        private readonly StepCommandFactory _commandFactory;
        private readonly IDeviceMethodInvoker _deviceMethodInvoker;
        private readonly List<DeviceForWorkFlow> _devices;
        private readonly Dictionary<string, List<DeviceForWorkFlow>> _deviceGroup;

        public WorkflowExecutor(IDeviceMethodInvoker deviceMethodInvoker, IUnitOfWork unitOfWork)
        {
            _deviceMethodInvoker = deviceMethodInvoker;

            _devices = (unitOfWork.GetRepository<Domain.Models.Device>().GetListAsync<DeviceForWorkFlow>(
                predicate: d => d.Status.Equals(Domain.Enums.DeviceStatus.Working),
                selector: d => new DeviceForWorkFlow(d)))
                .Result.ToList();

            _deviceGroup = _devices
                .GroupBy(d => d.Device.DeviceModelId)
                .ToDictionary(g => g.Key, g => g.ToList());
        }

        public async Task Execute(Workflow workflow)
        {
            Console.WriteLine($"[Workflow] Executing workflow {workflow.WorkflowId}...");

            var groupedSteps = workflow.Steps
                .GroupBy(s => s.Sequence)
                .OrderBy(g => g.Key);

            try
            {
                foreach (var stepGroup in groupedSteps)
                {
                    var tasks = stepGroup.Select(async step =>
                    {
                        if (!_deviceGroup.ContainsKey(step.DeviceModelId))
                        {
                            Console.WriteLine($"[Warning] No devices found for model ID {step.DeviceModelId}");
                            return false;
                        }

                        var targetDevices = _deviceGroup[step.DeviceModelId];
                        var targetDevice = targetDevices.FirstOrDefault(x => !x.IsWorking);

                        if (targetDevice == null)
                        {
                            Console.WriteLine($"[Warning] All devices busy for model ID {step.DeviceModelId}");
                            return false;
                        }

                        targetDevice.IsWorking = true;

                        try
                        {
                            var commandId = $"{workflow.WorkflowId}:{step.StepId}:{targetDevice.Device.DeviceId}";
                            var response = await _deviceMethodInvoker.InvokeAsync(new DeviceCommandRequest(
                                commandId,
                                1,
                                commandId,
                                workflow.WorkflowId,
                                step.StepId,
                                targetDevice.Device.DeviceId,
                                step.Function,
                                new Dictionary<string, string> { ["raw"] = step.Parameters ?? string.Empty },
                                DateTimeOffset.UtcNow,
                                30000));

                            Console.WriteLine($"[Device] Invoked {step.Function} on device {targetDevice.Device.DeviceId}, response: {response.Status}");

                            if (response.Status != "Completed")
                            {
                                Console.WriteLine($"[Error] Device {targetDevice.Device.DeviceId} responded with status {response.Status}");
                                return false;
                            }

                            return true;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[Error] Failed to invoke device {targetDevice.Device.DeviceId}: {ex.Message}");
                            return false;
                        }
                        finally
                        {
                            targetDevice.IsWorking = false;
                        }
                    });

                    var taskResults = await Task.WhenAll(tasks);

                    if (taskResults.Any(success => !success))
                    {
                        Console.WriteLine($"[Workflow] Step group {stepGroup.Key} failed. Aborting workflow {workflow.WorkflowId}.");
                        return;
                    }
                }

                Console.WriteLine($"[Workflow] Workflow {workflow.WorkflowId} completed.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Workflow execution failed: {ex.Message}");
            }
        }
    }

    public class DeviceForWorkFlow
    {
        public DeviceForWorkFlow(Domain.Models.Device device)
        {
            Device = device;
            IsWorking = false;
        }

        public Domain.Models.Device Device { get; set; }
        public bool IsWorking { get; set; }
    }
}
