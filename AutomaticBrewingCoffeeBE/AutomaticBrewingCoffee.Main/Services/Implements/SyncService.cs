using System.Linq.Expressions;
using AutoMapper;
using AutomaticBrewingCoffee.Domain.Enums;
using AutomaticBrewingCoffee.Domain.Models;
using AutomaticBrewingCoffee.Repository.Interfaces;
using AutomaticBrewingCoffee.Repository.Pagination;
using AutomaticBrewingCoffee.Services.Utils;
using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Services.BackgroundJobs;
using Services.Base;
using Services.Dtos.Sync;
using Services.Dtos.SyncEvent;
using Services.Dtos.SyncTask;
using Services.Interfaces;
using Services.Utils;

namespace Services.Implements;

public class SyncService : BaseService<SyncService>, ISyncService
{
    public SyncService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILoggerFactory loggerFactory,
        IHttpContextAccessor httpContextAccessor
    ) : base(
        unitOfWork,
        mapper,
        loggerFactory,
        httpContextAccessor
    )
    {
    }

    public async Task<BaseResult<string, SynchronizedKioskDataDto>> SynchronizedKioskData(string kioskId)
    {
        var syncTasks = await _unitOfWork.GetRepository<SyncTask>().GetListAsync(
            predicate: x => x.KioskId == kioskId && x.IsSynced == false
        );

        var syncEvents = new List<SyncEvent>();

        foreach (var syncTask in syncTasks)
        {
            var syncEvent = await _unitOfWork.GetRepository<SyncEvent>()
                .SingleOrDefaultAsync(predicate: x => x.SyncEventId == syncTask.SyncEventId);

            if (syncEvent != null)
            {
                syncEvents.Add(syncEvent);
            }
        }

        // Khởi tạo SyncActions
        var syncActions = new SyncActions();

        foreach (var syncEvent in syncEvents)
        {
            var entity = await GetEntityAsync(syncEvent);

            if (entity != null)
            {
                // Lấy SyncAction cho EntityType
                var syncAction = syncActions.GetSyncAction<dynamic>(syncEvent.EntityType);

                // Xử lý SyncEventType và phân loại vào đúng action
                switch (syncEvent.SyncEventType)
                {
                    case nameof(ESyncEventType.Create):
                        syncAction.Create.Add(entity);
                        break;
                    case nameof(ESyncEventType.Update):
                        syncAction.Update.Add(entity);
                        break;
                    case nameof(ESyncEventType.Delete):
                        syncAction.Delete.Add(entity);
                        break;
                }
            }
        }

        var synchronizedKioskDataDto = new SynchronizedKioskDataDto()
        {
            SyncActions = syncActions
        };

        var webhook = await _unitOfWork.GetRepository<Webhook>().SingleOrDefaultAsync(
            predicate: x => x.KioskId == kioskId && x.WebhookType == EWebhookType.SynchronizedData.ToString()
        );


        if (webhook is null)
        {
            return new BaseResult<string, SynchronizedKioskDataDto>()
            {
                Message = MessageUtil.NotFound<Webhook>(),
                IsSuccess = false,
                StatusCode = StatusCodes.Status404NotFound
            };
        }

        var kiosk = await _unitOfWork.GetRepository<Kiosk>().SingleOrDefaultAsync(
            predicate: x => x.KioskId == kioskId
        );

        if (kiosk is null)
        {
            return new BaseResult<string, SynchronizedKioskDataDto>()
            {
                IsSuccess = false,
                StatusCode = StatusCodes.Status404NotFound,
                Message = MessageUtil.NotFound<Kiosk>(),
                Request = kioskId,
                Response = null
            };
        }

        var result = await ApiUtil.PostAsync(
            webhook.WebhookUrl,
            synchronizedKioskDataDto,
            headers: new Dictionary<string, string>()
            {
                { "X-API-KEY", ApiKeyUtil.Decrypt(kiosk.ApiKey!) }
            }
        );

        if (!result.IsSuccessStatusCode)
        {
            return new BaseResult<string, SynchronizedKioskDataDto>()
            {
                IsSuccess = false,
                Message = MessageUtil.SyncFailure<Kiosk>(),
                Request = kioskId,
                Response = synchronizedKioskDataDto,
                StatusCode = StatusCodes.Status503ServiceUnavailable
            };
        }

        foreach (var syncTask in syncTasks)
        {
            syncTask.Sync();
        }

        _unitOfWork.GetRepository<SyncTask>().UpdateRange(syncTasks);
        await _unitOfWork.CommitAsync();


        return new BaseResult<string, SynchronizedKioskDataDto>()
        {
            IsSuccess = true,
            Message = MessageUtil.SyncSuccess<Kiosk>(),
            Request = kioskId,
            Response = synchronizedKioskDataDto,
            StatusCode = StatusCodes.Status200OK
        };
    }

    public async Task<BaseResult<string, OverridenKioskDataDto>> OverridenKioskData(string kioskId)
    {
        var kiosk = await _unitOfWork.GetRepository<Kiosk>().SingleOrDefaultAsync(
            predicate: x => x.KioskId == kioskId,
            include: x => x.Include(x => x.KioskDevices)
                .ThenInclude(x => x.Device)
        );

        if (kiosk is null)
        {
            return new BaseResult<string, OverridenKioskDataDto>()
            {
                IsSuccess = false,
                StatusCode = StatusCodes.Status404NotFound,
                Message = MessageUtil.NotFound<Kiosk>(),
                Request = kioskId,
                Response = null
            };
        }

        var kioskVersion = await _unitOfWork.GetRepository<KioskVersion>().SingleOrDefaultAsync(
            predicate: x => x.KioskVersionId == kiosk.KioskVersionId,
            include: x =>
                x.Include(x => x.KioskVersionProductMappings)
                    .ThenInclude(x => x.Product)
                    .ThenInclude(x => x.Workflows)
                    .ThenInclude(x => x.Steps)
        );

        if (kioskVersion is null)
        {
            return new BaseResult<string, OverridenKioskDataDto>()
            {
                IsSuccess = false,
                StatusCode = StatusCodes.Status404NotFound,
                Message = MessageUtil.NotFound<KioskVersion>(),
                Request = kioskId,
                Response = null
            };
        }

        var supportDevices = (kiosk.KioskDevices ?? new List<KioskDeviceMapping>())
            .Where(x => x.Device != null)
            .Select(x => x.Device!)
            .ToList();

        var supportProducts = kioskVersion.KioskVersionProductMappings.Select(x => x.Product).ToList();

        var supportWorkflows = supportProducts
            .SelectMany(x => x.Workflows ?? Enumerable.Empty<Workflow>())
            .ToList();

        var cleanWorkflows = await _unitOfWork.GetRepository<Workflow>().GetListAsync(
            predicate: x => x.KioskVersionId == kioskVersion.KioskVersionId && x.Type == EWorkflowType.Clean.ToString()
        );

        supportWorkflows.AddRange(cleanWorkflows);

        var supportSteps = new List<Step>();

        foreach (var workflow in supportWorkflows.ToList())
        {
            await TraverseWorkflowAsync(
                workflow.WorkflowId,
                supportWorkflows,
                supportSteps,
                new HashSet<string>(),
                new HashSet<string>()
            );
        }

        var supportWorkflowDtos = _mapper.Map<List<WorkflowSyncDto>>(supportWorkflows);
        var supportDeviceDtos = _mapper.Map<List<DeviceSyncDto>>(supportDevices);
        var supportStepDtos = _mapper.Map<List<StepSyncDto>>(supportSteps);

        var overridenKioskDataDto = new OverridenKioskDataDto
        {
            Workflows = supportWorkflowDtos,
            Devices = supportDeviceDtos,
            Steps = supportStepDtos
        };

        var webhook = await _unitOfWork.GetRepository<Webhook>().SingleOrDefaultAsync(
            predicate: x => x.KioskId == kiosk.KioskId && x.WebhookType == EWebhookType.OverriddenData.ToString()
        );

        if (webhook is null)
        {
            return new BaseResult<string, OverridenKioskDataDto>()
            {
                IsSuccess = false,
                StatusCode = StatusCodes.Status404NotFound,
                Message = MessageUtil.NotFound<Webhook>(),
                Request = kioskId,
                Response = null
            };
        }

        var result = await ApiUtil.PostAsync(
            webhook.WebhookUrl,
            overridenKioskDataDto,
            headers: new Dictionary<string, string>()
            {
                { "X-API-KEY", ApiKeyUtil.Decrypt(kiosk.ApiKey!) }
            }
        );

        if (!result.IsSuccessStatusCode)
        {
            return new BaseResult<string, OverridenKioskDataDto>()
            {
                IsSuccess = false,
                StatusCode = StatusCodes.Status503ServiceUnavailable,
                Request = kioskId,
                Message = MessageUtil.SyncFailure<Kiosk>(),
                Response = overridenKioskDataDto
            };
        }

        _logger.LogInformation("Enqueuing background job to mark all SyncTasks as synced for kiosk {KioskId} at {Time}",
            kiosk.KioskId, DateTime.UtcNow);

        BackgroundJob.Enqueue<SyncTaskSyncedJob>(job => job.MarkAllSyncTaskSyncedManually(kiosk.KioskId));

        _logger.LogInformation("Successfully enqueued SyncTaskSyncedJob for kiosk {KioskId}", kiosk.KioskId);

        return new BaseResult<string, OverridenKioskDataDto>()
        {
            IsSuccess = true,
            StatusCode = StatusCodes.Status201Created,
            Request = kioskId,
            Message = MessageUtil.SyncSuccess<Kiosk>(),
            Response = overridenKioskDataDto
        };
    }

    public async Task<BaseResult<SyncTaskQueryDto, Paginate<SyncTaskDto>>> GetSyncTasks(
        SyncTaskQueryDto syncTaskQueryDto)
    {
        LogMessage(LogLevel.Information, "In GetSyncTasks", syncTaskQueryDto);

        var predicate = _unitOfWork.GetRepository<SyncTask>()
            .BuildSearchPredicate(syncTaskQueryDto.FilterQuery, syncTaskQueryDto.FilterBy);

        if (syncTaskQueryDto.SyncEventId is not null)
        {
            Expression<Func<SyncTask, bool>> statusFilter = x =>
                x.SyncEventId == syncTaskQueryDto.SyncEventId;
            predicate = ExpressionHelper.CombineExpressions<SyncTask>(predicate, statusFilter);
        }

        if (syncTaskQueryDto.StartDate is not null && syncTaskQueryDto.EndDate is not null)
        {
            var dateRangePredicate = _unitOfWork.GetRepository<SyncTask>().BuildDateRangePredicate(
                syncTaskQueryDto.StartDate,
                syncTaskQueryDto.EndDate
            );
            predicate = ExpressionHelper.CombineExpressions(predicate, dateRangePredicate);
        }

        if (syncTaskQueryDto.SyncTaskId is not null)
        {
            Expression<Func<SyncTask, bool>> statusFilter = x =>
                x.SyncTaskId == syncTaskQueryDto.SyncTaskId;
            predicate = ExpressionHelper.CombineExpressions<SyncTask>(predicate, statusFilter);
        }

        if (syncTaskQueryDto.KioskId is not null)
        {
            Expression<Func<SyncTask, bool>> statusFilter = x =>
                x.KioskId == syncTaskQueryDto.KioskId;
            predicate = ExpressionHelper.CombineExpressions<SyncTask>(predicate, statusFilter);
        }

        if (syncTaskQueryDto.IsSynced is not null)
        {
            Expression<Func<SyncTask, bool>> statusFilter = x =>
                x.IsSynced == syncTaskQueryDto.IsSynced;
            predicate = ExpressionHelper.CombineExpressions<SyncTask>(predicate, statusFilter);
        }

        if (syncTaskQueryDto.CreatedDate is not null)
        {
            Expression<Func<SyncTask, bool>> statusFilter = x =>
                x.CreatedDate == syncTaskQueryDto.CreatedDate;
            predicate = ExpressionHelper.CombineExpressions<SyncTask>(predicate, statusFilter);
        }

        if (syncTaskQueryDto.SyncedAt is not null)
        {
            Expression<Func<SyncTask, bool>> statusFilter = x =>
                x.SyncedAt == syncTaskQueryDto.SyncedAt;
            predicate = ExpressionHelper.CombineExpressions<SyncTask>(predicate, statusFilter);
        }

        var orderBy = _unitOfWork.GetRepository<SyncTask>()
            .BuildSortingQuery(syncTaskQueryDto.SortBy, syncTaskQueryDto.IsAsc);

        var syncTasks = await _unitOfWork.GetRepository<SyncTask>().GetPagingListAsync(
            predicate: predicate,
            orderBy: orderBy,
            page: syncTaskQueryDto.Page,
            size: syncTaskQueryDto.Size,
            include: x => x.Include(x => x.SyncEvent).Include(x => x.Kiosk).ThenInclude(x => x.Store)
        );

        var syncTaskDtos = _mapper.Map<Paginate<SyncTaskDto>>(syncTasks);

        LogMessage(LogLevel.Information, "Out GetSyncTasks", syncTaskDtos);

        return new BaseResult<SyncTaskQueryDto, Paginate<SyncTaskDto>>()
        {
            IsSuccess = true,
            Message = MessageUtil.ReadSuccess<SyncTask>(),
            Request = syncTaskQueryDto,
            Response = syncTaskDtos,
            StatusCode = StatusCodes.Status200OK
        };
    }

    public async Task<BaseResult<SyncEventQueryDto, Paginate<SyncEventDto>>> GetSyncEvents(
        SyncEventQueryDto syncEventQueryDto)
    {
        LogMessage(LogLevel.Information, "In GetSyncEvents", syncEventQueryDto);

        var predicate = _unitOfWork.GetRepository<SyncEvent>()
            .BuildSearchPredicate(syncEventQueryDto.FilterQuery, syncEventQueryDto.FilterBy);

        Expression<Func<SyncEvent, bool>> isDeletedFilter = x =>
            x.IsDeleted == false;
        predicate = ExpressionHelper.CombineExpressions<SyncEvent>(predicate, isDeletedFilter);

        if (syncEventQueryDto.SyncEventId is not null)
        {
            Expression<Func<SyncEvent, bool>> statusFilter = x =>
                x.SyncEventId == syncEventQueryDto.SyncEventId;
            predicate = ExpressionHelper.CombineExpressions<SyncEvent>(predicate, statusFilter);
        }

        if (syncEventQueryDto.StartDate is not null && syncEventQueryDto.EndDate is not null)
        {
            var dateRangePredicate = _unitOfWork.GetRepository<SyncEvent>().BuildDateRangePredicate(
                syncEventQueryDto.StartDate,
                syncEventQueryDto.EndDate
            );
            predicate = ExpressionHelper.CombineExpressions(predicate, dateRangePredicate);
        }

        if (syncEventQueryDto.SyncEventType is not null)
        {
            Expression<Func<SyncEvent, bool>> statusFilter = x =>
                x.SyncEventType == syncEventQueryDto.SyncEventType;
            predicate = ExpressionHelper.CombineExpressions<SyncEvent>(predicate, statusFilter);
        }

        if (syncEventQueryDto.EntityType is not null)
        {
            Expression<Func<SyncEvent, bool>> statusFilter = x =>
                x.EntityType == syncEventQueryDto.EntityType;
            predicate = ExpressionHelper.CombineExpressions<SyncEvent>(predicate, statusFilter);
        }

        if (syncEventQueryDto.EntityId is not null)
        {
            Expression<Func<SyncEvent, bool>> statusFilter = x =>
                x.EntityId == syncEventQueryDto.EntityId;
            predicate = ExpressionHelper.CombineExpressions<SyncEvent>(predicate, statusFilter);
        }

        if (syncEventQueryDto.SecondEntityId is not null)
        {
            Expression<Func<SyncEvent, bool>> statusFilter = x =>
                x.SecondEntityId == syncEventQueryDto.SecondEntityId;
            predicate = ExpressionHelper.CombineExpressions<SyncEvent>(predicate, statusFilter);
        }

        if (syncEventQueryDto.CreatedDate is not null)
        {
            Expression<Func<SyncEvent, bool>> statusFilter = x =>
                x.CreatedDate == syncEventQueryDto.CreatedDate;
            predicate = ExpressionHelper.CombineExpressions<SyncEvent>(predicate, statusFilter);
        }

        if (syncEventQueryDto.UpdatedDate is not null)
        {
            Expression<Func<SyncEvent, bool>> statusFilter = x =>
                x.UpdatedDate == syncEventQueryDto.UpdatedDate;
            predicate = ExpressionHelper.CombineExpressions<SyncEvent>(predicate, statusFilter);
        }

        var orderBy = _unitOfWork.GetRepository<SyncEvent>()
            .BuildSortingQuery(syncEventQueryDto.SortBy, syncEventQueryDto.IsAsc);

        var products = await _unitOfWork.GetRepository<SyncEvent>().GetPagingListAsync(
            predicate: predicate,
            orderBy: orderBy,
            page: syncEventQueryDto.Page,
            size: syncEventQueryDto.Size,
            include: x => x.Include(x => x.SyncTasks)
        );

        var syncEventDtos = _mapper.Map<Paginate<SyncEventDto>>(products);

        LogMessage(LogLevel.Information, "Out GetSyncEvents", syncEventDtos);

        return new BaseResult<SyncEventQueryDto, Paginate<SyncEventDto>>()
        {
            IsSuccess = true,
            Message = MessageUtil.ReadSuccess<SyncEvent>(),
            Request = syncEventQueryDto,
            Response = syncEventDtos,
            StatusCode = StatusCodes.Status200OK
        };
    }

    private async Task<object?> GetEntityAsync(SyncEvent syncEvent)
    {
        object? entity = null;

        switch (syncEvent.EntityType)
        {
            case nameof(Product):
                var product = await _unitOfWork.GetRepository<Product>()
                    .SingleOrDefaultAsync(predicate: x => x.ProductId == syncEvent.EntityId);
                entity = _mapper.Map<ProductSyncDto>(product);
                break;

            case nameof(Workflow):
                var workflow = await _unitOfWork.GetRepository<Workflow>()
                    .SingleOrDefaultAsync(predicate: x => x.WorkflowId == syncEvent.EntityId);
                entity = _mapper.Map<WorkflowSyncDto>(workflow);
                break;

            case nameof(Step):
                var step = await _unitOfWork.GetRepository<Step>()
                    .SingleOrDefaultAsync(predicate: x => x.StepId == syncEvent.EntityId);

                if (step is not null && !string.IsNullOrEmpty(step.Parameters))
                {
                    step.Parameters = step.Parameters.Replace("\n", "");
                }

                entity = _mapper.Map<StepSyncDto>(step);
                break;

            case nameof(Menu):
                entity = await _unitOfWork.GetRepository<Menu>()
                    .SingleOrDefaultAsync(predicate: x => x.MenuId == syncEvent.EntityId);
                break;

            case nameof(MenuProductMapping):
                entity = await _unitOfWork.GetRepository<MenuProductMapping>()
                    .SingleOrDefaultAsync(predicate: x =>
                        x.MenuId == syncEvent.EntityId && x.ProductId == syncEvent.SecondEntityId);
                break;

            case nameof(Kiosk):
                entity = await _unitOfWork.GetRepository<Kiosk>()
                    .SingleOrDefaultAsync(predicate: x => x.KioskId == syncEvent.EntityId);
                break;

            case nameof(KioskDeviceMapping):
                entity = await _unitOfWork.GetRepository<KioskDeviceMapping>()
                    .SingleOrDefaultAsync(predicate: x => x.KioskDeviceMappingId == syncEvent.EntityId);
                break;

            case nameof(Device):
                var device = await _unitOfWork.GetRepository<Device>()
                    .SingleOrDefaultAsync(predicate: x => x.DeviceId == syncEvent.EntityId);
                entity = _mapper.Map<DeviceSyncDto>(device);
                break;

            default:
                entity = null;
                break;
        }

        return entity;
    }

    private async Task TraverseStepAsync(List<Workflow> workflows, Step step, List<Step> result,
        HashSet<string> visitedStepIds)
    {
        if (visitedStepIds.Contains(step.StepId))
            return;

        result.Add(step);
        visitedStepIds.Add(step.StepId);

        if (step.CallbackWorkflowId is not null)
        {
            var workflow = await _unitOfWork.GetRepository<Workflow>().SingleOrDefaultAsync(
                predicate: x => x.WorkflowId == step.CallbackWorkflowId);

            if (workflow is not null)
            {
                workflows.Add(workflow);
            }

            var nextStep = await _unitOfWork.GetRepository<Step>().SingleOrDefaultAsync(
                predicate: x => x.WorkflowId == step.CallbackWorkflowId);

            if (nextStep != null)
            {
                await TraverseStepAsync(workflows, nextStep, result, visitedStepIds);
            }
        }
    }


    private async Task<(List<Workflow> Workflows, List<Step> Steps)> TraverseWorkflowTreeAsync(string rootWorkflowId)
    {
        var visitedStepIds = new HashSet<string>();
        var visitedWorkflowIds = new HashSet<string>();
        var allSteps = new List<Step>();
        var allWorkflows = new List<Workflow>();

        await TraverseWorkflowAsync(rootWorkflowId, allWorkflows, allSteps, visitedStepIds, visitedWorkflowIds);

        return (allWorkflows, allSteps);
    }

    private async Task TraverseWorkflowAsync(
        string workflowId,
        List<Workflow> workflows,
        List<Step> steps,
        HashSet<string> visitedStepIds,
        HashSet<string> visitedWorkflowIds)
    {
        if (visitedWorkflowIds.Contains(workflowId))
            return;

        var workflow = await _unitOfWork.GetRepository<Workflow>()
            .SingleOrDefaultAsync(predicate: x => x.WorkflowId == workflowId);

        if (workflow == null)
            return;

        workflows.Add(workflow);
        visitedWorkflowIds.Add(workflowId);

        var workflowSteps = await _unitOfWork.GetRepository<Step>()
            .GetListAsync(predicate: x => x.WorkflowId == workflowId);

        foreach (var step in workflowSteps)
        {
            if (visitedStepIds.Contains(step.StepId))
                continue;

            steps.Add(step);
            visitedStepIds.Add(step.StepId);

            if (!string.IsNullOrEmpty(step.CallbackWorkflowId))
            {
                await TraverseWorkflowAsync(step.CallbackWorkflowId, workflows, steps, visitedStepIds,
                    visitedWorkflowIds);
            }
        }
    }
}