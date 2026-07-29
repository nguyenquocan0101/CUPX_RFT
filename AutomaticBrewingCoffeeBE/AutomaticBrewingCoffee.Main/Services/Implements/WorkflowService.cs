using System.Linq.Expressions;
using AutomaticBrewingCoffee.Repository.Interfaces;
using Microsoft.AspNetCore.Http;
using Services.Interfaces;
using Microsoft.Extensions.Logging;
using Services.Base;
using Services.Dtos.Step;
using Services.Dtos.Workflow;
using AutoMapper;
using AutomaticBrewingCoffee.Domain.Enums;
using AutomaticBrewingCoffee.Domain.Models;
using AutomaticBrewingCoffee.Repository.Pagination;
using Microsoft.EntityFrameworkCore;
using Services.Dtos.DeviceModel;
using Services.Dtos.Product;
using Services.Utils;

namespace Services.Implements
{
    public class WorkflowService : BaseService<WorkflowService>, IWorkflowService
    {
        public WorkflowService(
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


        public async Task<BaseResult<WorkflowQueryDto, Paginate<WorkflowDto>>> GetWorkflows(
            WorkflowQueryDto workflowQueryDto)
        {
            LogMessage(LogLevel.Information, "In GetWorkflows");

            var predicate = _unitOfWork.GetRepository<Workflow>()
                .BuildSearchPredicate(workflowQueryDto.FilterQuery, workflowQueryDto.FilterBy);

            if (workflowQueryDto.StartDate is not null && workflowQueryDto.EndDate is not null)
            {
                var dateRangePredicate = _unitOfWork.GetRepository<Workflow>().BuildDateRangePredicate(
                    workflowQueryDto.StartDate,
                    workflowQueryDto.EndDate
                );
                predicate = ExpressionHelper.CombineExpressions(predicate, dateRangePredicate);
            }

            if (workflowQueryDto.WorkflowType is not null)
            {
                Expression<Func<Workflow, bool>> statusFilter = x =>
                    x.Type == workflowQueryDto.WorkflowType;
                predicate = ExpressionHelper.CombineExpressions<Workflow>(predicate, statusFilter);
            }

            if (workflowQueryDto.ProductId is not null)
            {
                Expression<Func<Workflow, bool>> statusFilter = x =>
                    x.ProductId == workflowQueryDto.ProductId;
                predicate = ExpressionHelper.CombineExpressions<Workflow>(predicate, statusFilter);
            }

            var orderBy = _unitOfWork.GetRepository<Workflow>()
                .BuildSortingQuery(workflowQueryDto.SortBy, workflowQueryDto.IsAsc);

            var workflows = await _unitOfWork.GetRepository<Workflow>().GetPagingListAsync(
                predicate: predicate,
                orderBy: orderBy,
                page: workflowQueryDto.Page,
                size: workflowQueryDto.Size,
                include: x => x.Include(x => x.Steps)
            );

            var workflowsDto = _mapper.Map<Paginate<WorkflowDto>>(workflows);

            LogMessage(LogLevel.Information, "Out GetWorkflows");

            return new BaseResult<WorkflowQueryDto, Paginate<WorkflowDto>>()
            {
                IsSuccess = true,
                Message = MessageUtil.ReadSuccess<Workflow>(),
                Request = workflowQueryDto,
                Response = workflowsDto,
                StatusCode = StatusCodes.Status200OK
            };
        }

        public async Task<BaseResult<string, WorkflowDto>> GetWorkflow(string workflowId)
        {
            LogMessage(LogLevel.Information, "In GetWorkflow", workflowId);

            var workflow = await _unitOfWork.GetRepository<Workflow>()
                .SingleOrDefaultAsync(
                    predicate: w => w.WorkflowId == workflowId,
                    include: x => x.Include(x => x.Steps)
                );


            if (workflow == null)
            {
                return new BaseResult<string, WorkflowDto>
                {
                    IsSuccess = false,
                    Message = MessageUtil.NotFound<Workflow>(),
                    StatusCode = StatusCodes.Status404NotFound,
                    Request = workflowId,
                    Response = null
                };
            }

            if (workflow.Steps is not null)
            {
                workflow.Steps = workflow.Steps.OrderBy(s => s.Sequence).ToList();
            }

            var workflowDto = _mapper.Map<WorkflowDto>(workflow);

            if (workflowDto.ProductId is not null)
            {
                var product = await _unitOfWork.GetRepository<Product>().SingleOrDefaultAsync(
                    predicate: x => x.ProductId == workflowDto.ProductId);
                workflowDto.Product = _mapper.Map<ProductDto>(product);
            }

            if (workflowDto.Steps.Count > 0)
            {
                foreach (var step in workflowDto.Steps)
                {
                    var deviceModel = await _unitOfWork.GetRepository<DeviceModel>().SingleOrDefaultAsync(
                        predicate: x => x.DeviceModelId == step.DeviceModelId,
                        include: x => x.Include(x => x.DeviceType)
                            .Include(x => x.DeviceFunctions).ThenInclude(x => x.FunctionParameters)
                    );
                    var deviceModelDto = _mapper.Map<DeviceModelDto>(deviceModel);
                    step.DeviceModel = deviceModelDto;
                }
            }

            LogMessage(LogLevel.Information, "Out GetWorkflow");

            return new BaseResult<string, WorkflowDto>
            {
                IsSuccess = true,
                Message = MessageUtil.ReadSuccess<Workflow>(),
                Request = workflowId,
                Response = workflowDto,
                StatusCode = StatusCodes.Status200OK
            };
        }

        public async Task<BaseResult<string, WorkflowDto>> UpdateWorkflow(string workflowId,
            UpdateWorkflowDto updateWorkflowDto)
        {
            LogMessage(LogLevel.Information, "In UpdateWorkflow", updateWorkflowDto);

            var workflow = await _unitOfWork.GetRepository<Workflow>()
                .SingleOrDefaultAsync(
                    predicate: x => x.WorkflowId == workflowId,
                    include: x => x.Include(x => x.Steps)
                );

            if (workflow is null)
            {
                return new BaseResult<string, WorkflowDto>
                {
                    IsSuccess = false,
                    Message = MessageUtil.NotFound<Workflow>(),
                    StatusCode = StatusCodes.Status404NotFound,
                    Request = workflowId,
                    Response = null
                };
            }

            if (workflow.Steps is not null)
            {
                _unitOfWork.GetRepository<Step>().DeleteRange(workflow.Steps);
                // workflow.Steps = null;
            }

            workflow = _mapper.Map(updateWorkflowDto, workflow);

            // var steps = _mapper.Map<List<Step>>(updateWorkflowDto.Steps);

            _unitOfWork.GetRepository<Workflow>().Update(workflow);
            // _unitOfWork.GetRepository<Step>().UpdateRange(steps);
            await _unitOfWork.CommitAsync();


            var workflowDto = _mapper.Map<WorkflowDto>(workflow);

            LogMessage(LogLevel.Information, "Out UpdateWorkflow");

            return new BaseResult<string, WorkflowDto>
            {
                IsSuccess = true,
                Message = MessageUtil.UpdateSuccess<Workflow>(),
                StatusCode = StatusCodes.Status202Accepted,
                Response = workflowDto,
                Request = workflowId
            };
        }

        public async Task<BaseResult<string, WorkflowDto>> RemoveWorkflow(string workflowId)
        {
            LogMessage(LogLevel.Information, "In RemoveWorkflow", workflowId);

            var workflow = await _unitOfWork.GetRepository<Workflow>()
                .SingleOrDefaultAsync(predicate: x => x.WorkflowId == workflowId);

            if (workflow == null)
            {
                return new BaseResult<string, WorkflowDto>
                {
                    IsSuccess = false,
                    Message = MessageUtil.NotFound<Workflow>(),
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            var kiosk = await _unitOfWork.GetRepository<Kiosk>().SingleOrDefaultAsync(
                predicate: x => x.KioskVersionId == workflow.KioskVersionId
            );

            if (kiosk?.MenuId != null)
            {
                var product = await _unitOfWork.GetRepository<Product>().SingleOrDefaultAsync(
                    predicate: x => x.ProductId == workflow.ProductId
                );

                if (product is not null)
                {
                    var menuProduct = await _unitOfWork.GetRepository<MenuProductMapping>().SingleOrDefaultAsync(
                        predicate: x => x.MenuId == kiosk.MenuId && x.ProductId == product.ParentId
                    );

                    if (menuProduct is not null)
                    {
                        return new BaseResult<string, WorkflowDto>()
                        {
                            IsSuccess = false,
                            Message = MessageUtil.AlreadyUsing<Workflow, Product, Menu, Kiosk>(),
                            Request = workflowId,
                            Response = null,
                            StatusCode = StatusCodes.Status400BadRequest
                        };
                    }
                }
            }

            var steps = await _unitOfWork.GetRepository<Step>()
                .GetListAsync(predicate: x => x.WorkflowId == workflow.WorkflowId);
            _unitOfWork.GetRepository<Step>().DeleteRange(steps);
            _unitOfWork.GetRepository<Workflow>().Delete(workflow);
            await _unitOfWork.CommitAsync();

            LogMessage(LogLevel.Information, "Out RemoveWorkflow");

            return new BaseResult<string, WorkflowDto>
            {
                IsSuccess = true,
                Message = MessageUtil.DeleteSuccess<Workflow>(),
                StatusCode = StatusCodes.Status202Accepted
            };
        }

        public async Task<BaseResult<CreateStepDto, StepDto>> CreateStep(CreateStepDto createStepDto)
        {
            var workflow = await _unitOfWork.GetRepository<Workflow>().SingleOrDefaultAsync(
                predicate: x => x.WorkflowId == createStepDto.WorkflowId
            );

            if (workflow is null)
            {
                return new BaseResult<CreateStepDto, StepDto>()
                {
                    IsSuccess = false,
                    Message = MessageUtil.NotFound<Workflow>(),
                    StatusCode = StatusCodes.Status404NotFound,
                    Request = createStepDto,
                    Response = null
                };
            }

            var callBackWorkflow = await _unitOfWork.GetRepository<Workflow>().SingleOrDefaultAsync(
                predicate: x => x.WorkflowId == createStepDto.CallbackWorkflowId
            );


            if (callBackWorkflow is null)
            {
                return new BaseResult<CreateStepDto, StepDto>()
                {
                    IsSuccess = false,
                    Message = MessageUtil.NotFound<Workflow>(),
                    StatusCode = StatusCodes.Status404NotFound,
                    Request = createStepDto,
                    Response = null
                };
            }

            if (callBackWorkflow.Type != EWorkflowType.Callback.ToString())
            {
                return new BaseResult<CreateStepDto, StepDto>()
                {
                    IsSuccess = false,
                    Message = MessageUtil.Invalid<Workflow>(),
                    StatusCode = StatusCodes.Status404NotFound,
                    Request = createStepDto,
                    Response = null
                };
            }

            var steps = await _unitOfWork.GetRepository<Step>().GetListAsync(
                predicate: x => x.WorkflowId == createStepDto.WorkflowId
            );


            var step = _mapper.Map<Step>(createStepDto);

            await _unitOfWork.GetRepository<Step>().InsertAsync(step);
            await _unitOfWork.CommitAsync();

            var stepDto = _mapper.Map<StepDto>(
                step,
                opts => { opts.Items["Sequence"] = steps.Count; }
            );

            return new BaseResult<CreateStepDto, StepDto>()
            {
                IsSuccess = true,
                Message = MessageUtil.CreateSuccess<Workflow>(),
                StatusCode = StatusCodes.Status201Created,
                Request = createStepDto,
                Response = stepDto
            };
        }

        public async Task<BaseResult<UpdateStepDto, StepDto>> UpdateStep(string stepId, UpdateStepDto updateStepDto)
        {
            var step = await _unitOfWork.GetRepository<Step>()
                .SingleOrDefaultAsync(predicate: x => x.StepId == stepId);

            if (step is null)
            {
                return new BaseResult<UpdateStepDto, StepDto>()
                {
                    IsSuccess = false,
                    Message = MessageUtil.NotFound<Step>(),
                    Request = updateStepDto,
                    Response = null,
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            step = _mapper.Map(updateStepDto, step);

            _unitOfWork.GetRepository<Step>().Update(step);
            await _unitOfWork.CommitAsync();

            var deviceDto = _mapper.Map<StepDto>(step);

            return new BaseResult<UpdateStepDto, StepDto>()
            {
                IsSuccess = true,
                Message = MessageUtil.UpdateSuccess<Step>(),
                Request = updateStepDto,
                Response = deviceDto,
                StatusCode = StatusCodes.Status202Accepted
            };
        }

        public async Task<BaseResult<string, StepDto>> RemoveStep(string stepId)
        {
            var step = await _unitOfWork.GetRepository<Step>()
                .SingleOrDefaultAsync(predicate: x => x.StepId == stepId);

            if (step is null)
            {
                return new BaseResult<string, StepDto>()
                {
                    IsSuccess = false,
                    Message = MessageUtil.NotFound<Step>(),
                    Request = stepId,
                    Response = null,
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            _unitOfWork.GetRepository<Step>().Delete(step);

            return new BaseResult<string, StepDto>()
            {
                IsSuccess = true,
                Message = MessageUtil.DeleteSuccess<Step>(),
                Request = stepId,
                Response = null,
                StatusCode = StatusCodes.Status202Accepted
            };
        }

        public async Task<BaseResult<CreateWorkflowDto, WorkflowDto>> CreateWorkflow(
            CreateWorkflowDto createWorkflowDto)
        {
            LogMessage(LogLevel.Information, "In CreateWorkflow", createWorkflowDto);
            if (!string.IsNullOrEmpty(createWorkflowDto.ProductId))
            {
                var productExist = await _unitOfWork.GetRepository<Product>()
                    .SingleOrDefaultAsync(
                        predicate: x => x.ProductId == createWorkflowDto.ProductId,
                        include: x => x.Include(x => x.Workflows)
                    );
                if (productExist == null)
                {
                    return new BaseResult<CreateWorkflowDto, WorkflowDto>
                    {
                        IsSuccess = false,
                        Message = MessageUtil.NotFound<Product>(),
                        StatusCode = StatusCodes.Status409Conflict,
                        Request = createWorkflowDto,
                    };
                }

                if (productExist.Workflows is not null && productExist.Workflows.Count != 0)
                {
                    return new BaseResult<CreateWorkflowDto, WorkflowDto>
                    {
                        IsSuccess = false,
                        Message = MessageUtil.AlreadyExists<Workflow>(),
                        StatusCode = StatusCodes.Status400BadRequest,
                        Request = createWorkflowDto,
                    };
                }
            }

            foreach (var stepDto in createWorkflowDto.Steps)
            {
                if (!string.IsNullOrEmpty(stepDto.CallbackWorkflowId))
                {
                    var isCallbackWorkflowIdExist = await _unitOfWork.GetRepository<Workflow>()
                        .SingleOrDefaultAsync(predicate: x => x.WorkflowId == stepDto.CallbackWorkflowId);
                    if (isCallbackWorkflowIdExist == null)
                    {
                        return new BaseResult<CreateWorkflowDto, WorkflowDto>
                        {
                            IsSuccess = false,
                            Message = MessageUtil.NotFound<Workflow>(),
                            StatusCode = StatusCodes.Status404NotFound,
                            Request = createWorkflowDto,
                        };
                    }

                    if (isCallbackWorkflowIdExist.Type != EWorkflowType.Callback.ToString())
                    {
                        return new BaseResult<CreateWorkflowDto, WorkflowDto>
                        {
                            IsSuccess = false,
                            Message = MessageUtil.Invalid<Workflow>(),
                            StatusCode = StatusCodes.Status400BadRequest,
                            Request = createWorkflowDto,
                        };
                    }
                }
            }


            var workflow = _mapper.Map<Workflow>(createWorkflowDto);

            var steps = workflow.Steps;
            workflow.Steps = null;

            await _unitOfWork.GetRepository<Workflow>().InsertAsync(workflow);
            await _unitOfWork.CommitAsync();

            if (steps!.Count > 0)
            {
                await _unitOfWork.GetRepository<Step>().InsertRangeAsync(steps);
                await _unitOfWork.CommitAsync();
            }

            var workflowDto = _mapper.Map<WorkflowDto>(workflow);
            var stepInWorkflowDto = _mapper.Map<List<StepInsideDto>>(steps);
            workflowDto.Steps = stepInWorkflowDto;

            return new BaseResult<CreateWorkflowDto, WorkflowDto>
            {
                IsSuccess = true,
                Message = MessageUtil.CreateSuccess<Workflow>(),
                StatusCode = StatusCodes.Status200OK,
                Request = createWorkflowDto,
                Response = workflowDto
            };
        }

        public async Task<BaseResult<ReorderStepDto, List<StepDto>>> ReorderStep(
            string workflowId,
            ReorderStepDto reorderStepDto
        )
        {
            var steps = await _unitOfWork.GetRepository<Step>().GetListAsync(
                predicate: x => x.WorkflowId == workflowId,
                orderBy: q => q.OrderBy(x => x.Sequence)
            );

            var list = steps.ToList();

            var dragItem = list.FirstOrDefault(x => x.StepId == reorderStepDto.DragStepId);
            var targetItem = list.FirstOrDefault(x => x.StepId == reorderStepDto.TargetStepId);

            if (dragItem is null)
            {
                return new BaseResult<ReorderStepDto, List<StepDto>>
                {
                    IsSuccess = false,
                    Message = "Drag or target item not found",
                    Request = reorderStepDto,
                    Response = null,
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            if (targetItem is null)
            {
                return new BaseResult<ReorderStepDto, List<StepDto>>
                {
                    IsSuccess = false,
                    Message = "Drag or target item not found",
                    Request = reorderStepDto,
                    Response = null,
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            // Remove drag item temporarily
            list.Remove(dragItem);

            // Find index of target item
            var targetIndex = list.IndexOf(targetItem);
            var insertIndex = reorderStepDto.InsertAfter ? targetIndex + 1 : targetIndex;

            // Insert drag item to new position
            list.Insert(insertIndex, dragItem);

            // Reassign DisplayOrder
            for (int i = 0; i < list.Count; i++)
            {
                list[i].Sequence = i + 1;
                _unitOfWork.GetRepository<Step>().Update(list[i]);
            }

            await _unitOfWork.CommitAsync();

            var stepDtos = _mapper.Map<List<StepDto>>(steps);

            return new BaseResult<ReorderStepDto, List<StepDto>>
            {
                IsSuccess = true,
                Message = MessageUtil.UpdateSuccess<Product>(),
                Request = reorderStepDto,
                Response = stepDtos,
                StatusCode = StatusCodes.Status200OK
            };
        }
    }
}