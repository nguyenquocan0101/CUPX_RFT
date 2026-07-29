using AutoMapper;
using AutomaticBrewingCoffee.Domain.Models;
using AutomaticBrewingCoffee.Repository.Interfaces;
using AutomaticBrewingCoffee.Repository.Pagination;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Services.Base;
using Services.Dtos.Organization;
using Services.Interfaces;
using AutomaticBrewingCoffee.Services.Utils;
using System.Linq.Expressions;
using AutomaticBrewingCoffee.Domain.Enums;
using DotNetCore.CAP;
using Microsoft.EntityFrameworkCore;
using Services.CapRabbitMQ.Messages.Email;
using Services.CapRabbitMQ.Topics;
using Services.Dtos.Store;
using Services.Supabase;
using Services.Utils;

namespace Services.Implements;

public class OrganizationService : BaseService<OrganizationService>, IOrganizationService
{
    private readonly ISupabaseStorageService _supabaseStorageService;
    private readonly ICapPublisher _capPublisher;

    public OrganizationService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILoggerFactory loggerFactory,
        IHttpContextAccessor httpContextAccessor,
        ISupabaseStorageService supabaseStorageService,
        ICapPublisher capPublisher
    )
        : base(
            unitOfWork,
            mapper,
            loggerFactory,
            httpContextAccessor
        )
    {
        _supabaseStorageService = supabaseStorageService;
        _capPublisher = capPublisher;
    }

    public async Task<BaseResult<OrganizationQueryDto, Paginate<OrganizationDto>>> GetOrganizations(
        OrganizationQueryDto organizationQueryDto)
    {
        LogMessage(LogLevel.Information, "In GetOrganizations", organizationQueryDto);

        var predicate = _unitOfWork.GetRepository<Organization>()
            .BuildSearchPredicate(organizationQueryDto.FilterQuery, organizationQueryDto.FilterBy);

        Expression<Func<Organization, bool>> isDeletedFilter = x => !x.IsDeleted;
        predicate = ExpressionHelper.CombineExpressions(predicate, isDeletedFilter);

        var roles = GetAccountRolesFromJwt();
        if (roles[0].Equals(nameof(ERoleName.Organization)))
        {
            var referenceId = GetReferenceIdFromJwt();
            organizationQueryDto.OrganizationId = referenceId;
        }

        if (!string.IsNullOrEmpty(organizationQueryDto.Status))
        {
            Expression<Func<Organization, bool>> isStatus = x => x.Status == organizationQueryDto.Status;
            predicate = ExpressionHelper.CombineExpressions(predicate, isStatus);
        }

        if (!string.IsNullOrEmpty(organizationQueryDto.OrganizationId))
        {
            Expression<Func<Organization, bool>>
                isStatus = x => x.OrganizationId == organizationQueryDto.OrganizationId;
            predicate = ExpressionHelper.CombineExpressions(predicate, isStatus);
        }

        if (organizationQueryDto.StartDate is not null && organizationQueryDto.EndDate is not null)
        {
            var dateRangePredicate = _unitOfWork.GetRepository<Organization>().BuildDateRangePredicate(
                organizationQueryDto.StartDate,
                organizationQueryDto.EndDate
            );
            predicate = ExpressionHelper.CombineExpressions(predicate, dateRangePredicate);
        }

        var orderBy = _unitOfWork.GetRepository<Organization>()
            .BuildSortingQuery(organizationQueryDto.SortBy, organizationQueryDto.IsAsc);

        var organizations = await _unitOfWork.GetRepository<Organization>().GetPagingListAsync(
            predicate: predicate,
            orderBy: orderBy,
            page: organizationQueryDto.Page,
            size: organizationQueryDto.Size
        );

        var dto = _mapper.Map<Paginate<OrganizationDto>>(organizations);

        LogMessage(LogLevel.Information, "Out GetOrganizations", dto);

        return new BaseResult<OrganizationQueryDto, Paginate<OrganizationDto>>
        {
            IsSuccess = true,
            Message = MessageUtil.ReadSuccess<Organization>(),
            Request = organizationQueryDto,
            Response = dto,
            StatusCode = StatusCodes.Status200OK
        };
    }

    public async Task<BaseResult<string, OrganizationDto>> GetOrganization(string organizationId)
    {
        LogMessage(LogLevel.Information, "In GetOrganization", organizationId);

        var roles = GetAccountRolesFromJwt();
        if (roles[0].Equals(nameof(ERoleName.Organization)))
        {
            var referenceId = GetReferenceIdFromJwt();

            if (referenceId is null)
            {
                return new BaseResult<string, OrganizationDto>()
                {
                    IsSuccess = false,
                    Message = MessageUtil.Invalid<Organization>(),
                    Request = organizationId,
                    Response = null,
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }

            organizationId = referenceId;
        }

        var organization = await _unitOfWork.GetRepository<Organization>()
            .SingleOrDefaultAsync(predicate: x => x.OrganizationId == organizationId);

        if (organization == null)
        {
            return new BaseResult<string, OrganizationDto>
            {
                IsSuccess = false,
                Message = MessageUtil.NotFound<Organization>(),
                Request = organizationId,
                Response = null,
                StatusCode = StatusCodes.Status404NotFound
            };
        }

        var dto = _mapper.Map<OrganizationDto>(organization);

        LogMessage(LogLevel.Information, "Out GetOrganization", dto);

        return new BaseResult<string, OrganizationDto>
        {
            IsSuccess = true,
            Message = MessageUtil.ReadSuccess<Organization>(),
            Request = organizationId,
            Response = dto,
            StatusCode = StatusCodes.Status200OK
        };
    }

    public async Task<BaseResult<string, OrganizationReverseDto>> GetCurrentOrganization()
    {
        var kioskId = GetKioskIdFromJwt();

        var kiosk = await _unitOfWork.GetRepository<Kiosk>().SingleOrDefaultAsync(
            predicate: x => x.KioskId == kioskId,
            include: x => x.Include(x => x.Store)
                .ThenInclude(x => x.LocationType)
                .Include(x => x.Store)
                .ThenInclude(x => x.Organization)
        );

        if (kiosk is null)
        {
            return new BaseResult<string, OrganizationReverseDto>()
            {
                IsSuccess = false,
                Message = MessageUtil.NotFound<Kiosk>(),
                Request = kioskId,
                StatusCode = StatusCodes.Status404NotFound,
                Response = null
            };
        }

        if (kiosk.Store is null)
        {
            return new BaseResult<string, OrganizationReverseDto>()
            {
                IsSuccess = false,
                Message = MessageUtil.NotFound<Store>(),
                Request = kioskId,
                StatusCode = StatusCodes.Status404NotFound,
                Response = null
            };
        }

        if (kiosk.Store.Organization is null)
        {
            return new BaseResult<string, OrganizationReverseDto>()
            {
                IsSuccess = false,
                Message = MessageUtil.NotFound<Organization>(),
                Request = kioskId,
                StatusCode = StatusCodes.Status404NotFound,
                Response = null
            };
        }

        var organizationReverseDto = _mapper.Map<OrganizationReverseDto>(kiosk.Store.Organization);
        var storeReverseDto = _mapper.Map<StoreReverseDto>(kiosk.Store);
        organizationReverseDto.Store = storeReverseDto;

        return new BaseResult<string, OrganizationReverseDto>()
        {
            IsSuccess = true,
            Message = MessageUtil.ReadSuccess<Organization>(),
            Request = kioskId,
            StatusCode = StatusCodes.Status200OK,
            Response = organizationReverseDto
        };
    }

    public async Task<BaseResult<CreateOrganizationDto, OrganizationDto>> CreateOrganization(
        CreateOrganizationDto createOrganizationDto)
    {
        LogMessage(LogLevel.Information, "In CreateOrganization", createOrganizationDto);

        var existOrganization = await _unitOfWork.GetRepository<Organization>().SingleOrDefaultAsync(
            predicate: x =>
                createOrganizationDto.TaxId != null && x.TaxId != null &&
                x.TaxId.Trim() == createOrganizationDto.TaxId.Trim()
        );

        if (existOrganization is not null)
        {
            return new BaseResult<CreateOrganizationDto, OrganizationDto>()
            {
                IsSuccess = false,
                Message = MessageUtil.AlreadyExists<Organization>(createOrganizationDto.TaxId ?? string.Empty),
                Request = createOrganizationDto,
                StatusCode = StatusCodes.Status400BadRequest,
                Response = null
            };
        }

        var entity = _mapper.Map<Organization>(createOrganizationDto);

        if (!string.IsNullOrEmpty(createOrganizationDto.LogoBase64))
        {
            var base64Data = createOrganizationDto.LogoBase64.Contains(",")
                ? createOrganizationDto.LogoBase64.Split(',')[1]
                : createOrganizationDto.LogoBase64;

            var fileByte = Convert.FromBase64String(base64Data);
            var fileExtension = FileHelper.GetFileExtensionFromBase64(createOrganizationDto.LogoBase64);
            var fileName = $"{entity.OrganizationId}{fileExtension}";
            var filePath = $"{SupabaseSetting.Root.Organizations}/{entity.OrganizationId}/{fileName}";

            await _supabaseStorageService.UploadFile(fileByte, filePath, SupabaseSetting.Bucket.Images, true);

            var imageUrl = _supabaseStorageService.RetrievePublicUrl(SupabaseSetting.Bucket.Images, filePath);

            entity.LogoUrl = imageUrl;
        }

        await _unitOfWork.GetRepository<Organization>().InsertAsync(entity);

        var existAccount = await _unitOfWork.GetRepository<Account>()
            .SingleOrDefaultAsync(
                predicate: x => x.Email.ToLower() == createOrganizationDto.ContactEmail.ToLower()
            );

        if (existAccount is not null)
        {
            return new BaseResult<CreateOrganizationDto, OrganizationDto>()
            {
                IsSuccess = false,
                Message = MessageUtil.AlreadyExists<Organization>(existAccount.Email),
                Request = createOrganizationDto,
                StatusCode = StatusCodes.Status400BadRequest,
                Response = null
            };
        }

        var tempPassword = PasswordUtil.GenerateTemporaryPassword(6);

        var account = new Account()
        {
            Email = entity.ContactEmail!,
            Password = Hasher.Hash(tempPassword),
            AccountId = Guid.NewGuid().ToString(),
            FullName = entity.Name,
            RoleName = ERoleName.Organization.ToString(),
            OrganizationId = entity.OrganizationId!,
            RefreshToken = null,
            CreatedDate = DateTime.UtcNow,
            DeletedDate = null,
            IsDeleted = false,
            IsBanned = false,
            BannedReason = null,
            Status = EBaseStatus.Active.ToString()
        };

        await _unitOfWork.GetRepository<Account>().InsertAsync(account);
        await _unitOfWork.CommitAsync();

        await _capPublisher.PublishAsync(
            EmailCapTopic.EmailInvitation,
            new EmailInvitationCapMessage()
            {
                AccountEmail = account.Email,
                AccountPassword = tempPassword,
                OrganizationName = entity.Name
            });

        LogMessage(LogLevel.Information, "Insert Organization");

        var dto = _mapper.Map<OrganizationDto>(entity);

        LogMessage(LogLevel.Information, "Out CreateOrganization", dto);

        return new BaseResult<CreateOrganizationDto, OrganizationDto>
        {
            IsSuccess = true,
            Message = MessageUtil.CreateSuccess<Organization>(),
            Request = null,
            Response = dto,
            StatusCode = StatusCodes.Status201Created
        };
    }

    public async Task<BaseResult<UpdateOrganizationDto, OrganizationDto>> UpdateOrganization(
        string organizationId, UpdateOrganizationDto updateOrganizationDto)
    {
        var entity = await _unitOfWork.GetRepository<Organization>()
            .SingleOrDefaultAsync(predicate: x => x.OrganizationId == organizationId);

        if (entity == null)
        {
            return new BaseResult<UpdateOrganizationDto, OrganizationDto>
            {
                IsSuccess = false,
                Message = MessageUtil.NotFound<Organization>(),
                Request = updateOrganizationDto,
                Response = null,
                StatusCode = StatusCodes.Status404NotFound
            };
        }

        if (entity.TaxId != updateOrganizationDto.TaxId)
        {
            var organization = await _unitOfWork.GetRepository<Organization>()
                .SingleOrDefaultAsync(predicate: x => x.TaxId == updateOrganizationDto.TaxId);

            if (organization is not null)
            {
                return new BaseResult<UpdateOrganizationDto, OrganizationDto>
                {
                    IsSuccess = false,
                    Message = MessageUtil.AlreadyExists<Organization>(organization.TaxId ?? string.Empty),
                    Request = updateOrganizationDto,
                    Response = null,
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }
        }

        entity = _mapper.Map(updateOrganizationDto, entity);

        if (!string.IsNullOrEmpty(updateOrganizationDto.LogoBase64))
        {
            var base64Data = updateOrganizationDto.LogoBase64.Contains(",")
                ? updateOrganizationDto.LogoBase64.Split(',')[1]
                : updateOrganizationDto.LogoBase64;

            var fileByte = Convert.FromBase64String(base64Data);
            var fileExtension = FileHelper.GetFileExtensionFromBase64(updateOrganizationDto.LogoBase64);
            var fileName = $"{entity.Name}.{fileExtension}";
            var filePath = $"{SupabaseSetting.Root.Organizations}/{entity.OrganizationId}/{fileName}";

            await _supabaseStorageService.UploadFile(fileByte, filePath, SupabaseSetting.Bucket.Images, true);

            var imageUrl = _supabaseStorageService.RetrievePublicUrl(SupabaseSetting.Bucket.Images, filePath);

            entity.LogoUrl = imageUrl;
        }

        _unitOfWork.GetRepository<Organization>().Update(entity);
        await _unitOfWork.CommitAsync();

        var organizationDto = _mapper.Map<OrganizationDto>(entity);

        return new BaseResult<UpdateOrganizationDto, OrganizationDto>
        {
            IsSuccess = true,
            Message = MessageUtil.UpdateSuccess<Organization>(),
            Request = null,
            Response = organizationDto,
            StatusCode = StatusCodes.Status202Accepted
        };
    }

    public async Task<BaseResult<string, OrganizationDto>> RemoveOrganization(string organizationId)
    {
        LogMessage(LogLevel.Information, "In RemoveOrganization", organizationId);

        var entity = await _unitOfWork.GetRepository<Organization>()
            .SingleOrDefaultAsync(predicate: x => x.OrganizationId == organizationId);

        if (entity == null)
        {
            return new BaseResult<string, OrganizationDto>
            {
                IsSuccess = false,
                Message = MessageUtil.NotFound<Organization>(),
                Request = organizationId,
                Response = null,
                StatusCode = StatusCodes.Status404NotFound
            };
        }

        var stores = await _unitOfWork.GetRepository<Store>().GetListAsync(
            predicate: x => x.OrganizationId == organizationId
        );

        if (stores.Count > 0)
        {
            return new BaseResult<string, OrganizationDto>
            {
                IsSuccess = false,
                Message = MessageUtil.AlreadyUsing<Organization>(),
                Request = organizationId,
                Response = null,
                StatusCode = StatusCodes.Status400BadRequest
            };
        }

        entity.Delete();
        _unitOfWork.GetRepository<Organization>().Update(entity);

        var account = await _unitOfWork.GetRepository<Account>().SingleOrDefaultAsync(
            predicate: x => x.OrganizationId == entity.OrganizationId
        );

        if (account is not null)
        {
            account.Delete();
            account.Email += GuidUtil.ShortenGuid(Guid.NewGuid());
            _unitOfWork.GetRepository<Account>().Update(account);
        }

        await _unitOfWork.CommitAsync();

        var dto = _mapper.Map<OrganizationDto>(entity);

        LogMessage(LogLevel.Information, "Out RemoveOrganization", dto);

        return new BaseResult<string, OrganizationDto>
        {
            IsSuccess = true,
            Message = MessageUtil.DeleteSuccess<Organization>(),
            Request = organizationId,
            Response = dto,
            StatusCode = StatusCodes.Status202Accepted
        };
    }
}