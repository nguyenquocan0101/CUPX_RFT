using Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Repositories.Interfaces;
using Services.Base;
using Services.Dtos.Step;
using Services.Dtos.Workflow;
using AutoMapper;
using Domain.Models;
using Domain.Pagination;
using Services.Utils;
using System.Linq.Expressions;
using Services.Dtos.Product;
using Microsoft.EntityFrameworkCore;
using Domain.Enums;

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

            if (workflowQueryDto.Type is not null)
            {
                Expression<Func<Workflow, bool>> statusFilter = x =>
                    x.Type ==workflowQueryDto.Type;
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
                include: null
            );

            var workflowsDto = _mapper.Map<Paginate<WorkflowDto>>(workflows);

            LogMessage(LogLevel.Information, "Out GetWorkflows");

            return new BaseResult<WorkflowQueryDto, Paginate<WorkflowDto>>()
            {
                IsSuccess = true,
                Message = "Workflows found.",
                Request = workflowQueryDto,
                Response = workflowsDto,
                StatusCode = StatusCodes.Status200OK
            };
        }

        public async Task<BaseResult<WorkflowDto>> GetWorkflow(string workflowId)
        {
            LogMessage(LogLevel.Information, "In GetWorkflow", workflowId);

            var workflow = await _unitOfWork.GetRepository<Workflow>()
                .SingleOrDefaultAsync(
                    predicate: w => w.WorkflowId == workflowId,
                    include: x => x.Include(x => x.Steps));

            if (workflow == null)
            {
                return new BaseResult<WorkflowDto>
                {
                    IsSuccess = false,
                    Message = "Workflow not found.",
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            var workflowDto = _mapper.Map<WorkflowDto>(workflow);

            LogMessage(LogLevel.Information, "Out GetWorkflow");

            return new BaseResult<WorkflowDto>
            {
                IsSuccess = true,
                Message = "Workflow found.",
                ResponseRequest = workflowDto,
                StatusCode = StatusCodes.Status200OK
            };
        }

        public async Task<BaseResult<CreateWorkflowDto, WorkflowDto>> CreateWorkflow(
            CreateWorkflowDto createWorkflowDto)
        {
            try
            {
                LogMessage(LogLevel.Information, "In CreateWorkflow", createWorkflowDto);
                if (!string.IsNullOrEmpty(createWorkflowDto.ProductId))
                {
                    var productExist = await _unitOfWork.GetRepository<Product>()
                        .SingleOrDefaultAsync(predicate: x => x.ProductId == createWorkflowDto.ProductId);
                    if (productExist == null)
                    {
                        return new BaseResult<CreateWorkflowDto, WorkflowDto>
                        {
                            IsSuccess = false,
                            Message = "Product is not exist",
                            StatusCode = StatusCodes.Status409Conflict,
                            Request = createWorkflowDto,
                        };
                    }
                }

                var workflow = new Workflow
                {
                    WorkflowId = Guid.NewGuid().ToString(),
                    ProductId = createWorkflowDto.ProductId,
                    Name = createWorkflowDto.Name,
                    Description = createWorkflowDto.Description,
                    Type = createWorkflowDto.Type
                };
                await _unitOfWork.GetRepository<Workflow>().InsertAsync(workflow);

                int sequence = 1;
                foreach (var stepDto in createWorkflowDto.Steps)
                {
                    if (!string.IsNullOrEmpty(stepDto.CallbackWorkflowId))
                    {
                        var isCallBackWorkflowIdExist = await _unitOfWork.GetRepository<Workflow>()
                            .SingleOrDefaultAsync(predicate: x => x.WorkflowId == stepDto.CallbackWorkflowId);
                        if (isCallBackWorkflowIdExist == null)
                        {
                            return new BaseResult<CreateWorkflowDto, WorkflowDto>
                            {
                                IsSuccess = false,
                                Message = "CallBackWorkFlowId not found.",
                                StatusCode = StatusCodes.Status404NotFound,
                                Request = createWorkflowDto,
                            };
                        }
                    }

                    var step = new Step
                    {
                        StepId = Guid.NewGuid().ToString(),
                        WorkflowId = workflow.WorkflowId,
                        Name = stepDto.Name,
                        
                        Sequence = sequence++,
                        MaxRetries = stepDto.MaxRetries,
                        CallbackWorkflowId = stepDto.CallbackWorkflowId,
                        Parameters = stepDto.Parameters
                    };
                    await _unitOfWork.GetRepository<Step>().InsertAsync(step);
                }

                var workflowDto = _mapper.Map<WorkflowDto>(workflow);
                await _unitOfWork.CommitAsync();
                return new BaseResult<CreateWorkflowDto, WorkflowDto>
                {
                    IsSuccess = true,
                    Message = "Workflow created successfully.",
                    StatusCode = StatusCodes.Status200OK,
                    Request = createWorkflowDto,
                    Response = workflowDto
                };
            }
            catch (Exception e)
            {
                return new BaseResult<CreateWorkflowDto, WorkflowDto>
                {
                    IsSuccess = false,
                    Message = e.ToString(),
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Request = createWorkflowDto,
                };
            }
        }

        public async Task<BaseResult> UpdateWorkflow(UpdateWorkflowDto updateWorkflowDto)
        {
            LogMessage(LogLevel.Information, "In UpdateWorkflow", updateWorkflowDto);

            var workflow = await _unitOfWork.GetRepository<Workflow>()
                .SingleOrDefaultAsync(predicate: x => x.WorkflowId == updateWorkflowDto.Id);

            if (workflow == null)
            {
                return new BaseResult
                {
                    IsSuccess = false,
                    Message = "Workflow not found.",
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            var newWorkflow = _mapper.Map<Workflow>(updateWorkflowDto);

            _unitOfWork.GetRepository<Workflow>().Update(workflow);
            var isRemoveAllStep = await IsRemoveAllStep(workflow.WorkflowId);
            var isCreateListStep = await IsCreateListStep(workflow.WorkflowId, updateWorkflowDto.Steps.ToList());
            await _unitOfWork.CommitAsync();

            LogMessage(LogLevel.Information, "Out UpdateWorkflow");

            return new BaseResult
            {
                IsSuccess = true,
                Message = "Workflow updated successfully.",
                StatusCode = StatusCodes.Status202Accepted
            };
        }

        public async Task<BaseResult> RemoveWorkflow(RemoveWorkflowDto removeWorkflowDto)
        {
            LogMessage(LogLevel.Information, "In RemoveWorkflow", removeWorkflowDto);

            var workflow = await _unitOfWork.GetRepository<Workflow>()
                .SingleOrDefaultAsync(predicate: x => x.WorkflowId == removeWorkflowDto.WorkflowId);

            if (workflow == null)
            {
                return new BaseResult
                {
                    IsSuccess = false,
                    Message = "Workflow not found.",
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            var steps = await _unitOfWork.GetRepository<Step>()
                .GetListAsync(predicate: x => x.WorkflowId == workflow.WorkflowId);
            _unitOfWork.GetRepository<Step>().DeleteRange(steps);
            _unitOfWork.GetRepository<Workflow>().Delete(workflow);
            await _unitOfWork.CommitAsync();

            LogMessage(LogLevel.Information, "Out RemoveWorkflow");

            return new BaseResult
            {
                IsSuccess = true,
                Message = "Workflow removed successfully.",
                StatusCode = StatusCodes.Status202Accepted
            };
        }

        public Task<BaseResult> CreateStep(string workflowId, CreateStepDto createStepDto)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseResult> CreateListStep(string workflowId, List<CreateStepDto> createListStepDto)
        {
            LogMessage(LogLevel.Information, "In CreateListStep", createListStepDto);

            var workflow = await _unitOfWork.GetRepository<Workflow>()
                .SingleOrDefaultAsync(predicate: x => x.WorkflowId == workflowId);

            if (workflow == null)
            {
                return new BaseResult
                {
                    IsSuccess = false,
                    Message = "Workflow not found.",
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            var maxSequence = await _unitOfWork.GetRepository<Step>()
                .GetListAsync(predicate: x => x.WorkflowId == workflowId)
                .ContinueWith(t => t.Result.Max(s => (int?)s.Sequence) ?? 0);

            foreach (var stepDto in createListStepDto)
            {
                maxSequence++;
                var step = new Step
                {
                    StepId = Guid.NewGuid().ToString(),
                    WorkflowId = workflowId,
                    Sequence = maxSequence,
                    MaxRetries = stepDto.MaxRetries,
                    CallbackWorkflowId = stepDto.CallbackWorkflowId,
                    Parameters = stepDto.Parameters
                };
                await _unitOfWork.GetRepository<Step>().InsertAsync(step);
            }

            await _unitOfWork.CommitAsync();

            LogMessage(LogLevel.Information, "Out CreateListStep");

            return new BaseResult
            {
                IsSuccess = true,
                Message = "Steps created successfully.",
                StatusCode = StatusCodes.Status200OK
            };
        }

        public Task<BaseResult> UpdateStep(string workflowId, UpdateStepDto updateStepDto)
        {
            throw new NotImplementedException();
        }

        public Task<BaseResult> UpdateListStep(string workflowId, List<UpdateStepDto> updateListStepDto)
        {
            throw new NotImplementedException();
        }

        public Task<BaseResult> RemoveStep(string workflowId, string stepId)
        {
            throw new NotImplementedException();
        }

        public Task<BaseResult> RemoveListStep(string workflowId, List<string> listStepId)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseResult> RemoveAllStep(string workflowId)
        {
            throw new NotImplementedException();
        }

        protected async Task<bool> IsRemoveAllStep(string workflowId)
        {
            try
            {
                LogMessage(LogLevel.Information, "In RemoveAllStep", workflowId);

                var steps = await _unitOfWork.GetRepository<Step>()
                    .GetListAsync(predicate: x => x.WorkflowId == workflowId);

                _unitOfWork.GetRepository<Step>().DeleteRange(steps);

                await _unitOfWork.CommitAsync();

                LogMessage(LogLevel.Information, "Out RemoveAllStep");

                return true;
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
        }

        protected async Task<bool> IsCreateListStep(string workflowId, List<CreateStepDto> createListStepDto)
        {
            try
            {
                LogMessage(LogLevel.Information, "In CreateListStep", createListStepDto);

                var workflow = await _unitOfWork.GetRepository<Workflow>()
                    .SingleOrDefaultAsync(predicate: x => x.WorkflowId == workflowId);

                if (workflow == null)
                {
                    throw new Exception("Workflow not found.");
                }

                int sequence = 1;
                foreach (var stepDto in createListStepDto)
                {
                    var isCallBackWorkflowIdExist = await _unitOfWork.GetRepository<Workflow>()
                        .SingleOrDefaultAsync(predicate: x => x.WorkflowId == stepDto.CallbackWorkflowId);
                    if (isCallBackWorkflowIdExist == null)
                    {
                        throw new Exception("CallBackWorkFlowId not found.");
                    }

                    var step = new Step
                    {
                        StepId = Guid.NewGuid().ToString(),
                        WorkflowId = workflowId,
                        Name = stepDto.Name,
                        Sequence = sequence++,
                        MaxRetries = stepDto.MaxRetries,
                        CallbackWorkflowId = stepDto.CallbackWorkflowId,
                        Parameters = stepDto.Parameters
                    };
                    await _unitOfWork.GetRepository<Step>().InsertAsync(step);
                }

                await _unitOfWork.CommitAsync();

                LogMessage(LogLevel.Information, "Out CreateListStep");

                return true;
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
        }
    }
}