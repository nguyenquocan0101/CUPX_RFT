using AutoMapper;
using Domain.CouchDbModels;
using Domain.Enums;
using Domain.Models;
using MassTransit.SagaStateMachine;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Services.Base;
using Services.Dtos.Device;
using Services.Dtos.Step;
using Services.Dtos.Sync;
using Services.Dtos.Workflow;
using Services.Interfaces;
using Services.StrategyPattern.Sync;
using System;

namespace Services.Implements
{
    public class KioskSyncService : IKioskSyncService
    {
        private readonly IMapper _mapper;
        private readonly ILogger<KioskSyncService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IWebHostEnvironment _web;

        public KioskSyncService(
            IMapper mapper,
            ILogger<KioskSyncService> logger,
            IServiceProvider serviceProvider,
            IWebHostEnvironment web)
        {
            _mapper = mapper;
            _logger = logger;
            _serviceProvider = serviceProvider;
            _web = web;
        }

        public async Task<BaseResult> SyncKioskData(SyncActionDto request)
        {
            var data = request.SyncActions.Actions;
            try
            {
                if (data.Device != null)
                {
                    var deviceDocumentStrategy = _serviceProvider.GetRequiredKeyedService<ISyncStrategy<DeviceDocument>>(typeof(DeviceDocument).Name);
                    await ProcessEntitySyncAsync(data.Device, deviceDocumentStrategy, dto => dto.DeviceId);
                    var deviceStatusStrategy = _serviceProvider.GetRequiredKeyedService<ISyncStrategy<DeviceStatusDocument>>(typeof(DeviceStatusDocument).Name);
                    await ProcessEntitySyncAsync(data.Device, deviceStatusStrategy, dto => dto.DeviceId);
                }

                if (data.Workflow != null)
                {
                    var strategy = _serviceProvider.GetRequiredKeyedService<ISyncStrategy<Workflow>>(typeof(Workflow).Name);
                    await ProcessWorkflowSyncAsync(data.Workflow, strategy);
                }

                if (data.Step != null)
                {
                    // Logic của Step không phải là CRUD trên Step, mà là cập nhật Workflow.
                    // Do đó nó không dùng được ProcessEntitySyncAsync một cách trực tiếp.
                    // Chúng ta giữ lại logic xử lý riêng cho nó.
                    await ProcessStepSyncAsync(data.Step);
                }

                return new BaseResult { IsSuccess = true, Message = "Sync Kiosk data successfully", StatusCode = StatusCodes.Status202Accepted };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing kiosk data");
                return new BaseResult { IsSuccess = false, Message = "Sync Kiosk data failed with " + ex.Message, StatusCode = StatusCodes.Status500InternalServerError };
            }
        }

        /// <summary>
        /// Phương thức generic để xử lý đồng bộ CRUD cho bất kỳ entity nào sử dụng một strategy.
        /// </summary>
        private async Task ProcessEntitySyncAsync<TDto, TEntity>(
            EntitySyncOperation<TDto> syncData,
            ISyncStrategy<TEntity> strategy,
            Func<TDto, string> idSelector) // Hàm để lấy ID từ DTO
            where TEntity : class
        {
            // Create
            if (syncData.Create != null)
            {
                foreach (var dto in syncData.Create)
                {
                    var entity = _mapper.Map<TEntity>(dto);
                    await strategy.SaveAsync(entity, idSelector(dto));
                }
            }

            if (syncData.Update != null)
            {
                foreach (var dto in syncData.Update)
                {
                    var entity = _mapper.Map<TEntity>(dto);
                    await strategy.SaveAsync(entity, idSelector(dto));
                }
            }

            if (syncData.Delete != null)
            {
                foreach (var dto in syncData.Delete)
                {
                    await strategy.DeleteAsync(idSelector(dto));
                }
            }
        }

        /// <summary>
        /// Xử lý riêng cho Step vì nó có logic nghiệp vụ đặc thù: cập nhật file Workflow cha.
        /// </summary>
        private async Task ProcessStepSyncAsync(EntitySyncOperation<StepSyncDto> stepData)
        {
            // Lấy strategy của Workflow để thao tác với file workflow
            var workflowStrategy = _serviceProvider.GetRequiredKeyedService<ISyncStrategy<Workflow>>(typeof(Workflow).Name);

            var allChanges = (stepData.Create ?? new List<StepSyncDto>())
                .Select(d => new { Dto = d, Type = "Create" })
                .Concat((stepData.Update ?? new List<StepSyncDto>()).Select(d => new { Dto = d, Type = "Update" }))
                .Concat((stepData.Delete ?? new List<StepSyncDto>()).Select(d => new { Dto = d, Type = "Delete" }));

            var changesByWorkflow = allChanges.GroupBy(c => c.Dto.WorkflowId);

            foreach (var group in changesByWorkflow)
            {
                var workflowId = group.Key;
                if (string.IsNullOrEmpty(workflowId)) continue;

                Func<string>? workflowDirectoryFunc = null;

                var workflow = await workflowStrategy.LoadAsync(workflowId);
                if (workflow != null)
                {
                    workflowDirectoryFunc = () => GetWorkflowDirectory(WorkflowType.Clean);
                   workflow = await workflowStrategy.LoadAsync(workflowId, () => GetWorkflowDirectory(WorkflowType.Clean));
                }
                if (workflow == null)
                {
                    _logger.LogError($"Cannot process steps for Workflow ID '{workflowId}' because the workflow was not found.");
                    continue;
                }

                foreach (var change in group)
                {
                    var stepDto = change.Dto;
                    var stepEntity = _mapper.Map<Step>(stepDto);

                    switch (change.Type)
                    {
                        case "Create":
                            // Tránh thêm trùng lặp nếu đã tồn tại
                            if (!workflow.Steps.Any(s => s.StepId == stepEntity.StepId))
                            {
                                workflow.Steps.Add(stepEntity);
                            }
                            break;
                        case "Update":
                            var existingStep = workflow.Steps.FirstOrDefault(s => s.StepId == stepEntity.StepId);
                            if (existingStep != null)
                            {
                                _mapper.Map(stepDto, existingStep);
                            }
                            break;

                        case "Delete":
                            var stepToRemove = workflow.Steps.FirstOrDefault(s => s.StepId == stepEntity.StepId);
                            if (stepToRemove != null)
                            {
                                workflow.Steps.Remove(stepToRemove);
                            }
                            break;
                    }
                }

                await workflowStrategy.SaveAsync(workflow, workflowId, workflowDirectoryFunc);
            }
        }


        public async Task<BaseResult> SyncOverridenKioskData(OverridenKioskDataSyncDto request)
        {
            var data = request;
            _logger.LogInformation("Starting Sync (overwrite mode)...");
            try
            {
                if (data.Devices != null)
                {
                    _logger.LogInformation("Starting Device sync (overwrite mode)...");
                    var deviceDocumentStrategy = _serviceProvider.GetRequiredKeyedService<ISyncStrategy<DeviceDocument>>(typeof(DeviceDocument).Name);
                    var deviceEntities = _mapper.Map<IEnumerable<DeviceDocument>>(request.Devices);
                    await deviceDocumentStrategy.OverwriteAllAsync(deviceEntities);
                    var deviceStatus = _mapper.Map<IEnumerable<DeviceStatusDocument>>(request.Devices);
                    var deviceStatusStrategy = _serviceProvider.GetRequiredKeyedService<ISyncStrategy<DeviceStatusDocument>>(typeof(DeviceStatusDocument).Name);
                    await deviceStatusStrategy.OverwriteAllAsync(deviceStatus);
                }

                if (data.Workflows != null)
                {
                    var executeWorkflows = new List<Workflow>();
                    var cleanWorkflows = new List<Workflow>();
                    _logger.LogInformation("Starting Workflow sync (overwrite mode)...");
                    var strategy = _serviceProvider.GetRequiredKeyedService<ISyncStrategy<Workflow>>(typeof(Workflow).Name);
                    var workflowEntities = _mapper.Map<IEnumerable<Workflow>>(request.Workflows);

                    if (request.Steps != null && request.Steps.Any())
                    {
                        _logger.LogInformation("Mapping Steps to Workflows...");
                        var stepEntities = _mapper.Map<IEnumerable<Step>>(request.Steps);
                        var stepsByWorkflowId = stepEntities.GroupBy(s => s.WorkflowId);

                        foreach (var workflow in workflowEntities)
                        {
                            // Tìm các step thuộc về workflow này
                            var correspondingSteps = stepsByWorkflowId.FirstOrDefault(g => g.Key == workflow.WorkflowId);
                            if (correspondingSteps != null)
                            {
                                workflow.Steps = correspondingSteps.ToList();
                            }
                            if(workflow.Type == WorkflowType.Clean)
                            {
                                cleanWorkflows.Add(workflow);
                            } else executeWorkflows.Add(workflow);
                        }
                        _logger.LogInformation($"Mapped {stepEntities.Count()} steps to {workflowEntities.Count()} workflows.");
                    }

                    // Ghi đè toàn bộ bằng danh sách chỉ chứa 1 workflow này
                    await strategy.OverwriteAllAsync(executeWorkflows);
                    await strategy.OverwriteAllAsync(cleanWorkflows, () => GetWorkflowDirectory(WorkflowType.Clean));
                }

                return new BaseResult { IsSuccess = true, Message = "Sync Kiosk data successfully", StatusCode = StatusCodes.Status202Accepted };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing kiosk data");
                return new BaseResult { IsSuccess = false, Message = "Sync Kiosk data failed with " + ex.Message, StatusCode = StatusCodes.Status500InternalServerError };
            }
        }

        private async Task ProcessWorkflowSyncAsync(EntitySyncOperation<WorkflowSyncDto> syncData, ISyncStrategy<Workflow> strategy)
        {
            // Create
            if (syncData.Create != null)
            {
                foreach (var dto in syncData.Create)
                {
                    var entity = _mapper.Map<Workflow>(dto);
                    var directoryFunc = () => GetWorkflowDirectory(dto.Type);
                    await strategy.SaveAsync(entity, dto.ProductId, directoryFunc);
                }
            }

            // Update
            if (syncData.Update != null)
            {
                foreach (var dto in syncData.Update)
                {
                    var entity = _mapper.Map<Workflow>(dto);
                    var directoryFunc = () => GetWorkflowDirectory(dto.Type);
                    await strategy.SaveAsync(entity, dto.ProductId, directoryFunc);
                }
            }

            // Delete
            if (syncData.Delete != null)
            {
                foreach (var dto in syncData.Delete)
                {
                    var directoryFunc = () => GetWorkflowDirectory(dto.Type);
                    await strategy.DeleteAsync(dto.ProductId, directoryFunc);
                }
            }
        }

        private string GetWorkflowDirectory(WorkflowType workflowType)
        {
            var baseDataPath = Path.Combine(_web.ContentRootPath, "DataStorage");

            if (workflowType == WorkflowType.Clean)
            {
                return Path.Combine(baseDataPath, "Clean");
            }
            return Path.Combine(baseDataPath, nameof(Workflow));
        }
    }

}