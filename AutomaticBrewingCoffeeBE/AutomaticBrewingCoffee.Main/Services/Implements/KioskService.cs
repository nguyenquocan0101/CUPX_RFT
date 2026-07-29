using System.IO.Compression;
using AutomaticBrewingCoffee.Repository.Interfaces;
using Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Services.Base;
using Services.Dtos.Device;
using AutoMapper;
using AutomaticBrewingCoffee.Domain.Models;
using AutomaticBrewingCoffee.Domain.Enums;
using AutomaticBrewingCoffee.Repository.Pagination;
using System.Linq.Expressions;
using System.Net;
using AutomaticBrewingCoffee.Services.Utils;
using DotNetCore.CAP;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IO;
using Services.AzureIotHub;
using Services.Dtos.Kiosk;
using Services.Dtos.KioskDevice;
using Services.Utils;
using Services.Cludflare;
using Services.Cludflare.Models;
using Services.Dtos.Webhook;

namespace Services.Implements
{
    public class KioskService : BaseService<KioskService>, IKioskService
    {
        private readonly CloudflareApi _cloudflareApi;
        private readonly DeviceManager _deviceManager;
        private readonly IConfiguration _configuration;
        private readonly RecyclableMemoryStreamManager _memoryStreamManager;

        public KioskService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILoggerFactory loggerFactory,
            IHttpContextAccessor httpContextAccessor,
            CloudflareApi cloudflareApi,
            DeviceManager deviceManager, IConfiguration configuration,
            RecyclableMemoryStreamManager memoryStreamManager) : base(
            unitOfWork,
            mapper,
            loggerFactory,
            httpContextAccessor
        )
        {
            _cloudflareApi = cloudflareApi;
            _deviceManager = deviceManager;
            _configuration = configuration;
            _memoryStreamManager = memoryStreamManager;
        }

        /// <summary>
        /// Get list of device
        /// </summary>
        /// <param name="kioskQueryDto"></param>
        /// <returns></returns>
        public async Task<BaseResult<KioskQueryDto, Paginate<KioskDto>>> GetKiosks(KioskQueryDto kioskQueryDto)
        {
            LogMessage(LogLevel.Information, "In GetKiosks", kioskQueryDto);

            var roles = GetAccountRolesFromJwt();

            if (roles[0].Equals(ERoleName.Organization.ToString()))
            {
                var referenceId = GetReferenceIdFromJwt();
                kioskQueryDto.OrganizationId = referenceId;
            }

            var predicate = _unitOfWork.GetRepository<Kiosk>()
                .BuildSearchPredicate(kioskQueryDto.FilterQuery, kioskQueryDto.FilterBy);

            Expression<Func<Kiosk, bool>> isDeletedFilter = x =>
                x.IsDeleted == false;
            predicate = ExpressionHelper.CombineExpressions<Kiosk>(predicate, isDeletedFilter);

            if (kioskQueryDto.StartDate is not null && kioskQueryDto.EndDate is not null)
            {
                var dateRangePredicate = _unitOfWork.GetRepository<Kiosk>().BuildDateRangePredicate(
                    kioskQueryDto.StartDate,
                    kioskQueryDto.EndDate
                );
                predicate = ExpressionHelper.CombineExpressions(predicate, dateRangePredicate);
            }

            if (kioskQueryDto.Status is not null)
            {
                Expression<Func<Kiosk, bool>> statusFilter = x => x.Status == kioskQueryDto.Status;
                predicate = ExpressionHelper.CombineExpressions<Kiosk>(predicate, statusFilter);
            }

            if (kioskQueryDto.StoreId is not null)
            {
                Expression<Func<Kiosk, bool>> franchiseFilter = x => x.StoreId == kioskQueryDto.StoreId;
                predicate = ExpressionHelper.CombineExpressions<Kiosk>(predicate, franchiseFilter);
            }

            if (kioskQueryDto.OrganizationId is not null)
            {
                Expression<Func<Kiosk, bool>> franchiseFilter = x =>
                    x.Store != null && x.Store.OrganizationId == kioskQueryDto.OrganizationId;
                predicate = ExpressionHelper.CombineExpressions<Kiosk>(predicate, franchiseFilter);
            }

            var orderBy = _unitOfWork.GetRepository<Kiosk>()
                .BuildSortingQuery(kioskQueryDto.SortBy, kioskQueryDto.IsAsc);

            var kiosks = await _unitOfWork.GetRepository<Kiosk>().GetPagingListAsync(
                predicate: predicate,
                orderBy: orderBy,
                page: kioskQueryDto.Page,
                size: kioskQueryDto.Size,
                include: x => x
                    .Include(x => x.Store)
                    .Include(x => x.KioskVersion)
                    .Include(x => x.KioskDevices)
                    .ThenInclude(x => x.Device)
                    .Include(x => x.KioskDevices)
                    .ThenInclude(x => x.Device)
                    .ThenInclude(x => x.DeviceIngredientStates)
                    .Include(x => x.KioskDevices)
                    .ThenInclude(x => x.Device)
                    .ThenInclude(x => x.DeviceIngredientHistories)
                    .Include(x => x.KioskVersion)
                    .ThenInclude(x => x.KioskType)
            );

            var kioskDtos = _mapper.Map<Paginate<KioskDto>>(kiosks);

            LogMessage(LogLevel.Information, "Out GetKiosks", kioskDtos);

            return new BaseResult<KioskQueryDto, Paginate<KioskDto>>()
            {
                IsSuccess = true,
                Message = MessageUtil.ReadSuccess<Kiosk>(),
                Request = kioskQueryDto,
                Response = kioskDtos,
                StatusCode = StatusCodes.Status200OK
            };
        }

        public async Task<BaseResult<KioskQueryDto, Paginate<KioskDto>>> GetNoMenuKiosks(KioskQueryDto kioskQueryDto)
        {
            LogMessage(LogLevel.Information, "In GetKiosks", kioskQueryDto);

            var predicate = _unitOfWork.GetRepository<Kiosk>()
                .BuildSearchPredicate(kioskQueryDto.FilterQuery, kioskQueryDto.FilterBy);

            if (kioskQueryDto.Status is not null)
            {
                Expression<Func<Kiosk, bool>> statusFilter = x => x.Status == kioskQueryDto.Status;
                predicate = ExpressionHelper.CombineExpressions<Kiosk>(predicate, statusFilter);
            }

            if (kioskQueryDto.StoreId is not null)
            {
                Expression<Func<Kiosk, bool>> franchiseFilter = x => x.StoreId == kioskQueryDto.StoreId;
                predicate = ExpressionHelper.CombineExpressions<Kiosk>(predicate, franchiseFilter);
            }

            var orderBy = _unitOfWork.GetRepository<Kiosk>()
                .BuildSortingQuery(kioskQueryDto.SortBy, kioskQueryDto.IsAsc);

            var kiosks = await _unitOfWork.GetRepository<Kiosk>().GetPagingListAsync(
                predicate: predicate,
                orderBy: orderBy,
                page: kioskQueryDto.Page,
                size: kioskQueryDto.Size,
                include: x => x
                    .Include(x => x.Store)
                    .Include(x => x.KioskVersion)
                    .Include(x => x.KioskDevices)
                    .ThenInclude(x => x.Device)
                    .Include(x => x.KioskDevices)
                    .ThenInclude(x => x.Device)
                    .ThenInclude(x => x.DeviceIngredientStates)
            );

            var kioskDto = _mapper.Map<Paginate<KioskDto>>(kiosks);

            LogMessage(LogLevel.Information, "Out GetKiosks", kioskDto);

            return new BaseResult<KioskQueryDto, Paginate<KioskDto>>()
            {
                IsSuccess = true,
                Message = MessageUtil.ReadSuccess<Kiosk>(),
                Request = kioskQueryDto,
                Response = kioskDto,
                StatusCode = StatusCodes.Status200OK
            };
        }

        /// <summary>
        /// Get a kiosk by id.
        /// </summary>
        /// <param name="kioskId"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<BaseResult<string, KioskDto>> GetKiosk(string kioskId)
        {
            LogMessage(LogLevel.Information, "In GetKiosk", kioskId);

            var kiosk = await _unitOfWork.GetRepository<Kiosk>().SingleOrDefaultAsync(
                predicate: x => x.KioskId == kioskId,
                ignoreQueryFilter: true,
                include: x => x
                    .Include(x => x.Store)
                    .Include(x => x.KioskVersion)
                    .Include(x => x.KioskDevices)
                    .ThenInclude(x => x.Device)
                    .ThenInclude(x => x.DeviceModel)
                    .ThenInclude(x => x.DeviceType)
                    .Include(x => x.KioskDevices)
                    .ThenInclude(x => x.Device)
                    .ThenInclude(x => x.DeviceIngredientStates)
                    .Include(x => x.KioskDevices)
                    .ThenInclude(x => x.Device)
                    .ThenInclude(x => x.DeviceIngredientHistories)
                    .Include(x => x.KioskVersion)
                    .ThenInclude(x => x.KioskType)
            );

            if (kiosk is null)
            {
                return new BaseResult<string, KioskDto>()
                {
                    IsSuccess = false,
                    Message = MessageUtil.NotFound<Kiosk>(),
                    Request = kioskId,
                    Response = null,
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            var webhooks = await _unitOfWork.GetRepository<Webhook>().GetListAsync(
                predicate: x => x.KioskId == kiosk.KioskId
            );


            var tunnelToken = await _cloudflareApi.GetTunnelTokenAsync(kiosk.KioskId);

            var webhookDto = _mapper.Map<List<WebhookDto>>(webhooks);

            var kioskDto = _mapper.Map<KioskDto>(kiosk);
            kioskDto.Webhooks = webhookDto;
            kioskDto.TunnelToken = tunnelToken;

            // Ping kiosk for status
            var healthCheckWebhook = webhooks.FirstOrDefault(x => x.WebhookType == EWebhookType.HealthCheck.ToString());
            if (healthCheckWebhook is not null)
            {
                try
                {
                    var resultHealthCheck = await ApiUtil.GetAsync(
                        healthCheckWebhook.WebhookUrl,
                        headers: new Dictionary<string, string>()
                        {
                            { "X-API-KEY", ApiKeyUtil.Decrypt(kiosk.ApiKey!) }
                        }
                    );

                    if (!resultHealthCheck.IsSuccessStatusCode)
                    {
                        switch (resultHealthCheck.StatusCode)
                        {
                            case HttpStatusCode.ServiceUnavailable:
                                kioskDto.IsOnline = true;
                                kioskDto.IsBusy = true;
                                break;

                            default:
                                // Gọi được API nhưng trả mã lỗi khác → vẫn online nhưng không bận
                                kioskDto.IsOnline = false;
                                kioskDto.IsBusy = true;
                                break;
                        }
                    }
                    else
                    {
                        // Gọi API thành công
                        kioskDto.IsOnline = true;
                        kioskDto.IsBusy = false;
                    }
                }
                catch (Exception ex)
                {
                    // Không gọi được API (server down, DNS fail, timeout, v.v.) → kiosk offline
                    kioskDto.IsOnline = false;
                    kioskDto.IsBusy = false;
                }
            }

            LogMessage(LogLevel.Information, "Out GetKiosk", kioskDto);

            return new BaseResult<string, KioskDto>()
            {
                IsSuccess = true,
                Message = MessageUtil.ReadSuccess<Kiosk>(),
                Request = kioskId,
                Response = kioskDto,
                StatusCode = StatusCodes.Status200OK
            };
        }

        public async Task<BaseResult<string, KioskDto>> GetCurrentKiosk()
        {
            var kioskId = GetKioskIdFromJwt();

            var kiosk = await _unitOfWork.GetRepository<Kiosk>().SingleOrDefaultAsync(
                predicate: x => x.KioskId == kioskId,
                include: x => x.Include(x => x.KioskDevices)
                    .ThenInclude(x => x.Device)
                    .ThenInclude(x => x.DeviceIngredientStates)
                    .Include(x => x.KioskDevices)
                    .ThenInclude(x => x.Device)
                    .ThenInclude(x => x.DeviceModel)
                    .ThenInclude(x => x.DeviceType)
                    .Include(x => x.KioskDevices)
                    .ThenInclude(x => x.Device)
                    .ThenInclude(x => x.DeviceIngredientStates)
                    .Include(x => x.KioskDevices)
                    .ThenInclude(x => x.Device)
                    .ThenInclude(x => x.DeviceIngredientHistories)
            );

            if (kiosk is null)
            {
                return new BaseResult<string, KioskDto>()
                {
                    IsSuccess = false,
                    Message = MessageUtil.NotFound<Kiosk>(),
                    Request = kioskId,
                    Response = null,
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            var kioskDto = _mapper.Map<KioskDto>(kiosk);

            return new BaseResult<string, KioskDto>()
            {
                IsSuccess = true,
                Message = MessageUtil.ReadSuccess<Kiosk>(),
                Request = kioskId,
                Response = kioskDto,
                StatusCode = StatusCodes.Status200OK
            };
        }

        private bool ValidKioskDevices(List<KioskVersionDeviceModelMapping> deviceModels, List<Device> devices)
        {
            var totalDeviceModel = deviceModels.Sum(x => x.Quantity);

            if (totalDeviceModel != devices.Count)
            {
                return false;
            }

            foreach (var deviceModel in deviceModels)
            {
                var device = devices.Where(x => x.DeviceModelId == deviceModel.DeviceModelId).ToList();
                if (device.Count != deviceModel.Quantity)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Creates a new kiosk.
        /// </summary>
        /// <param name="createKioskDto"></param>
        /// <returns></returns>
        public async Task<BaseResult<CreateKioskDto, KioskDto>> CreateKiosk(CreateKioskDto createKioskDto)
        {
            LogMessage(LogLevel.Information, "In CreateKiosk", createKioskDto);

            var existingDevices = await _unitOfWork.GetRepository<Device>()
                .GetListAsync(
                    predicate: d => createKioskDto.DeviceIds.Contains(d.DeviceId),
                    include: x => x.Include(x => x.DeviceModel)
                        .ThenInclude(x => x.DeviceType)
                );

            if (existingDevices.Count != createKioskDto.DeviceIds.Count)
            {
                var missing = createKioskDto.DeviceIds.Except(existingDevices.Select(d => d.DeviceId)).ToList();
                return new BaseResult<CreateKioskDto, KioskDto>()
                {
                    IsSuccess = false,
                    Message = $"Some devices not found: {string.Join(", ", missing)}",
                    Request = createKioskDto,
                    Response = null,
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            var store = await _unitOfWork.GetRepository<Store>()
                .SingleOrDefaultAsync
                (
                    predicate: x => x.StoreId == createKioskDto.StoreId,
                    include: x => x.Include(x => x.Organization)
                );

            if (store is null)
            {
                return new BaseResult<CreateKioskDto, KioskDto>()
                {
                    IsSuccess = false,
                    Message = MessageUtil.NotFound<Kiosk>(),
                    Request = createKioskDto,
                    Response = null,
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            var tunnel = await SetUpTunnelForKioskInStoreAsync(store.Organization!.OrganizationCode);

            if (tunnel is null)
            {
                return new BaseResult<CreateKioskDto, KioskDto>()
                {
                    IsSuccess = false,
                    Message = MessageUtil.CreateFailure<TunnelConfigurationDetail>(),
                    Request = createKioskDto,
                    Response = null,
                    StatusCode = StatusCodes.Status503ServiceUnavailable
                };
            }

            var newKiosk = _mapper.Map<Kiosk>(createKioskDto,
                opts =>
                {
                    opts.Items["ApiKey"] = ApiKeyUtil.GenerateApiKey();
                    opts.Items["TunnelId"] = tunnel.TunnelId;
                    opts.Items["Hostname"] = tunnel.Config.Ingress[0].Hostname;
                    opts.Items["OriginServer"] = tunnel.Config.Ingress[0].Service;
                }
            );

            var kioskDevices = new List<KioskDeviceMapping>();

            var mobileDevices = existingDevices
                .Where(x => x.DeviceModel?.DeviceType?.IsMobileDevice == true)
                .ToList();

            for (int i = 0; i < createKioskDto.DeviceIds.Count; i++)
            {
                var deviceId = createKioskDto.DeviceIds[i];
                var device = existingDevices.FirstOrDefault(x => x.DeviceId == deviceId);

                if (device == null)
                    throw new Exception($"Không tìm thấy thiết bị có ID: {deviceId}");

                // Mặc định không có side
                ESide? side = null;

                // Nếu có đúng 2 thiết bị mobile thì gán side theo thứ tự
                if (mobileDevices.Count == 2 && device.DeviceModel?.DeviceType?.IsMobileDevice == true)
                {
                    var indexInMobile = mobileDevices.FindIndex(x => x.DeviceId == deviceId);
                    if (indexInMobile == 0)
                        side = ESide.Left;
                    else if (indexInMobile == 1)
                        side = ESide.Right;
                }

                var kioskDevice = new KioskDeviceMapping
                {
                    KioskDeviceMappingId = Guid.NewGuid().ToString(),
                    KioskId = newKiosk.KioskId,
                    DeviceId = deviceId,
                    Status = EKioskDeviceStatus.Online.ToString(),
                    Side = side.ToString(),
                    CreatedDate = DateTime.UtcNow
                };

                kioskDevices.Add(kioskDevice);
            }


            var kioskVersion = await _unitOfWork.GetRepository<KioskVersion>().SingleOrDefaultAsync(
                predicate: x => x.KioskVersionId == createKioskDto.KioskVersionId,
                include: x => x.Include(x => x.KioskVersionDeviceModelMappings)
                    .Include(x => x.KioskVersionProductMappings)
            );

            if (kioskVersion is null)
            {
                await _cloudflareApi.DeleteTunnelAsync(tunnel.TunnelId);
                return new BaseResult<CreateKioskDto, KioskDto>()
                {
                    IsSuccess = false,
                    Message = MessageUtil.NotFound<KioskVersion>(),
                    Request = createKioskDto,
                    Response = null,
                    StatusCode = StatusCodes.Status404NotFound
                };
            }


            if (!ValidKioskDevices(kioskVersion.KioskVersionDeviceModelMappings.ToList(), existingDevices.ToList()))
            {
                await _cloudflareApi.DeleteTunnelAsync(tunnel.TunnelId);
                return new BaseResult<CreateKioskDto, KioskDto>()
                {
                    IsSuccess = false,
                    Message = MessageUtil.Invalid<Device>(),
                    Request = createKioskDto,
                    Response = null,
                    StatusCode = StatusCodes.Status404NotFound
                };
            }


            // Valid Product
            if (createKioskDto.MenuId is not null)
            {
                var menu = await _unitOfWork.GetRepository<Menu>().SingleOrDefaultAsync(
                    predicate: x => x.MenuId == createKioskDto.MenuId,
                    include: x => x.Include(x => x.MenuProductMappings).ThenInclude(x => x.Product)
                );

                if (menu is null)
                {
                    return new BaseResult<CreateKioskDto, KioskDto>()
                    {
                        IsSuccess = false,
                        Message = MessageUtil.NotFound<Menu>(),
                        Request = createKioskDto,
                        Response = null,
                        StatusCode = StatusCodes.Status404NotFound
                    };
                }

                if (menu.MenuProductMappings is { Count: > 0 })
                {
                    var menuProductIds = menu.MenuProductMappings
                        .Select(mp => mp.ProductId)
                        .Distinct()
                        .ToList();

                    // Lấy con của các parent trong menu
                    var productChildren = await _unitOfWork.GetRepository<Product>().GetListAsync(
                        predicate: p => p.ParentId != null && menuProductIds.Contains(p.ParentId)
                    );

                    // Map tên parent để báo lỗi
                    var parentNameById = menu.MenuProductMappings.ToDictionary(
                        p => p.ProductId,
                        p => p.Product.Name,
                        StringComparer.OrdinalIgnoreCase
                    );

                    // Group con theo ParentId
                    var childrenByParent = productChildren
                        .GroupBy(c => c.ParentId!, StringComparer.OrdinalIgnoreCase)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(x => x.ProductId).ToList(),
                            StringComparer.OrdinalIgnoreCase
                        );

                    // Tập sản phẩm được hỗ trợ bởi phiên bản kiosk
                    var supportedIds = (kioskVersion.KioskVersionProductMappings?
                            .Select(m => m.ProductId) ?? Enumerable.Empty<string>())
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);

                    // Cha nào không có BẤT KỲ con nào trong supportedIds → lỗi
                    var missingParents = new List<string>();
                    foreach (var parentId in menuProductIds)
                    {
                        if (!childrenByParent.TryGetValue(parentId, out var childIds) || childIds.Count == 0)
                            continue; // policy hiện tại: parent không có con thì pass

                        var ok = childIds.Any(childId => supportedIds.Contains(childId));
                        if (!ok)
                        {
                            var parentName = parentNameById.TryGetValue(parentId, out var n) ? n : parentId;
                            missingParents.Add(parentName);
                        }
                    }

                    if (missingParents.Count > 0)
                    {
                        var versionName = string.IsNullOrEmpty(kioskVersion.VersionTitle)
                            ? kioskVersion.KioskVersionId
                            : kioskVersion.VersionTitle;

                        await _cloudflareApi.DeleteTunnelAsync(tunnel.TunnelId);

                        return new BaseResult<CreateKioskDto, KioskDto>
                        {
                            IsSuccess = false,
                            Message = MessageUtil.MissingChildProducts(versionName, missingParents),
                            Request = createKioskDto,
                            Response = null,
                            StatusCode = StatusCodes.Status400BadRequest
                        };
                    }
                }
            }


            foreach (var device in existingDevices)
            {
                device.Working();

                // If is mobile device then continue
                if (device.DeviceModel!.DeviceType!.IsMobileDevice)
                {
                    continue;
                }

                if (device.IsOnHub)
                {
                    continue;
                }

                var deviceOnHub = await _deviceManager.AddHubDevice(device.DeviceId!);
                device.OnHub();
                if (deviceOnHub is null)
                {
                    return new BaseResult<CreateKioskDto, KioskDto>()
                    {
                        IsSuccess = false,
                        Message = MessageUtil.CreateOnHubFailure<Device>(),
                        Request = createKioskDto,
                        Response = null,
                        StatusCode = StatusCodes.Status503ServiceUnavailable
                    };
                }
            }

            await _unitOfWork.GetRepository<Kiosk>().InsertAsync(newKiosk);

            await _unitOfWork.CommitAsync();

            await _unitOfWork.GetRepository<KioskDeviceMapping>().InsertRangeAsync(kioskDevices);

            await _unitOfWork.CommitAsync();

            foreach (var device in existingDevices)
            {
                device.DeviceModel = null;
            }

            _unitOfWork.GetRepository<Device>().UpdateRange(existingDevices);

            await _unitOfWork.CommitAsync();

            LogMessage(LogLevel.Information, "Insert Kiosk");

            var hostName = tunnel.Config.Ingress[0].Hostname;
            var domain = hostName.Contains("https://") ? hostName : "https://" + hostName;

            var webHooks = new List<Webhook>()
            {
                new Webhook()
                {
                    KioskId = newKiosk.KioskId,
                    CreatedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedDate = null,
                    UpdatedDate = null,
                    WebhookId = Guid.NewGuid().ToString(),
                    WebhookType = EWebhookType.SynchronizedData.ToString(),
                    WebhookUrl = $"{domain}/api/v1/synchronized-data"
                },
                new Webhook()
                {
                    KioskId = newKiosk.KioskId,
                    CreatedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedDate = null,
                    UpdatedDate = null,
                    WebhookId = Guid.NewGuid().ToString(),
                    WebhookType = EWebhookType.ExecuteProduct.ToString(),
                    WebhookUrl = $"{domain}/api/v1/execute"
                },
                new Webhook()
                {
                    KioskId = newKiosk.KioskId,
                    CreatedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedDate = null,
                    UpdatedDate = null,
                    WebhookId = Guid.NewGuid().ToString(),
                    WebhookType = EWebhookType.RetrieveDevice.ToString(),
                    WebhookUrl = $"{domain}/api/v1/doc/devices"
                },
                new Webhook()
                {
                    KioskId = newKiosk.KioskId,
                    CreatedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedDate = null,
                    UpdatedDate = null,
                    WebhookId = Guid.NewGuid().ToString(),
                    WebhookType = EWebhookType.OverriddenData.ToString(),
                    WebhookUrl = $"{domain}/api/v1/overridden-data"
                },
                new Webhook()
                {
                    KioskId = newKiosk.KioskId,
                    CreatedDate = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedDate = null,
                    UpdatedDate = null,
                    WebhookId = Guid.NewGuid().ToString(),
                    WebhookType = EWebhookType.HealthCheck.ToString(),
                    WebhookUrl = $"{domain}/api/v1/ping"
                }
            };

            await _unitOfWork.GetRepository<Webhook>().InsertRangeAsync(webHooks);
            await _unitOfWork.CommitAsync();

            // var kioskWithDevices = await _unitOfWork.GetRepository<Kiosk>().SingleOrDefaultAsync(
            //     predicate: k => k.KioskId == newKiosk.KioskId,
            //     include: k => k
            //         .Include(x => x.Store)
            //         .Include(x => x.KioskVersion)
            //         .Include(x => x.KioskDevices)
            //         .ThenInclude(kd => kd.Device)!
            // );

            var kioskDto = _mapper.Map<KioskDto>(newKiosk);

            LogMessage(LogLevel.Information, "Out CreateKiosk", kioskDto);

            return new BaseResult<CreateKioskDto, KioskDto>
            {
                IsSuccess = true,
                Message = MessageUtil.CreateSuccess<Kiosk>(),
                Request = createKioskDto,
                Response = kioskDto,
                StatusCode = StatusCodes.Status201Created
            };
        }

        /// <summary>
        /// Update a kiosk
        /// </summary>
        /// <param name="kioskId"></param>
        /// <param name="updateKioskDto"></param>
        /// <returns></returns>
        public async Task<BaseResult<UpdateKioskDto, KioskDto>> UpdateKiosk(string kioskId,
            UpdateKioskDto updateKioskDto)
        {
            var kiosk = await _unitOfWork.GetRepository<Kiosk>()
                .SingleOrDefaultAsync(
                    predicate: x => x.KioskId == kioskId
                );

            if (kiosk is null)
            {
                return new BaseResult<UpdateKioskDto, KioskDto>()
                {
                    IsSuccess = false,
                    Message = MessageUtil.NotFound<Kiosk>(),
                    Request = updateKioskDto,
                    Response = null,
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            // Valid Product
            if (updateKioskDto.MenuId is not null)
            {
                var kioskVersion = await _unitOfWork.GetRepository<KioskVersion>().SingleOrDefaultAsync(
                    predicate: x => x.KioskVersionId == kiosk.KioskVersionId,
                    include: x => x.Include(x => x.KioskVersionDeviceModelMappings)
                        .Include(x => x.KioskVersionProductMappings)
                );

                var menu = await _unitOfWork.GetRepository<Menu>().SingleOrDefaultAsync(
                    predicate: x => x.MenuId == updateKioskDto.MenuId,
                    include: x => x.Include(x => x.MenuProductMappings).ThenInclude(x => x.Product)
                );

                if (menu is null)
                {
                    return new BaseResult<UpdateKioskDto, KioskDto>()
                    {
                        IsSuccess = false,
                        Message = MessageUtil.NotFound<Menu>(),
                        Request = updateKioskDto,
                        Response = null,
                        StatusCode = StatusCodes.Status404NotFound
                    };
                }

                if (menu.MenuProductMappings is { Count: > 0 })
                {
                    var menuProductIds = menu.MenuProductMappings
                        .Select(mp => mp.ProductId)
                        .Distinct()
                        .ToList();

                    // Lấy con của các parent trong menu
                    var productChildren = await _unitOfWork.GetRepository<Product>().GetListAsync(
                        predicate: p => p.ParentId != null && menuProductIds.Contains(p.ParentId)
                    );

                    // Map tên parent để báo lỗi
                    var parentNameById = menu.MenuProductMappings.ToDictionary(
                        p => p.ProductId,
                        p => p.Product.Name,
                        StringComparer.OrdinalIgnoreCase
                    );

                    // Group con theo ParentId
                    var childrenByParent = productChildren
                        .GroupBy(c => c.ParentId!, StringComparer.OrdinalIgnoreCase)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(x => x.ProductId).ToList(),
                            StringComparer.OrdinalIgnoreCase
                        );

                    // Tập sản phẩm được hỗ trợ bởi phiên bản kiosk
                    var supportedIds = (kioskVersion.KioskVersionProductMappings?
                            .Select(m => m.ProductId) ?? Enumerable.Empty<string>())
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);

                    // Cha nào không có BẤT KỲ con nào trong supportedIds → lỗi
                    var missingParents = new List<string>();
                    foreach (var parentId in menuProductIds)
                    {
                        if (!childrenByParent.TryGetValue(parentId, out var childIds) || childIds.Count == 0)
                            continue; // policy hiện tại: parent không có con thì pass

                        var ok = childIds.Any(childId => supportedIds.Contains(childId));
                        if (!ok)
                        {
                            var parentName = parentNameById.TryGetValue(parentId, out var n) ? n : parentId;
                            missingParents.Add(parentName);
                        }
                    }

                    if (missingParents.Count > 0)
                    {
                        var versionName = string.IsNullOrEmpty(kioskVersion.VersionTitle)
                            ? kioskVersion.KioskVersionId
                            : kioskVersion.VersionTitle;

                        return new BaseResult<UpdateKioskDto, KioskDto>
                        {
                            IsSuccess = false,
                            Message = MessageUtil.MissingChildProducts(versionName, missingParents),
                            Request = updateKioskDto,
                            Response = null,
                            StatusCode = StatusCodes.Status400BadRequest
                        };
                    }
                }
            }

            kiosk = _mapper.Map(updateKioskDto, kiosk);

            _unitOfWork.GetRepository<Kiosk>().Update(kiosk);
            await _unitOfWork.CommitAsync();

            var kioskDto = _mapper.Map<KioskDto>(kiosk);

            return new BaseResult<UpdateKioskDto, KioskDto>()
            {
                IsSuccess = true,
                Message = MessageUtil.UpdateSuccess<Kiosk>(),
                Request = updateKioskDto,
                Response = kioskDto,
                StatusCode = StatusCodes.Status202Accepted
            };
        }

        /// <summary>
        /// Remove a kiosk
        /// </summary>
        /// <param name="kioskId"></param>
        /// <returns></returns>
        public async Task<BaseResult<string, KioskDto>> RemoveKiosk(string kioskId)
        {
            LogMessage(LogLevel.Information, "In RemoveKiosk", kioskId);

            var kiosk = await _unitOfWork.GetRepository<Kiosk>()
                .SingleOrDefaultAsync(predicate: x => x.KioskId == kioskId);

            if (kiosk is null)
            {
                return new BaseResult<string, KioskDto>()
                {
                    IsSuccess = false,
                    Message = MessageUtil.NotFound<Kiosk>(),
                    Request = kioskId,
                    Response = null,
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            if (kiosk.Status == nameof(EBaseStatus.Active))
            {
                return new BaseResult<string, KioskDto>()
                {
                    IsSuccess = false,
                    Message = MessageUtil.AlreadyUsing<Kiosk>(),
                    Request = kioskId,
                    Response = null,
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }

            var kioskDevices = await _unitOfWork.GetRepository<KioskDeviceMapping>().GetListAsync(
                predicate: x => x.KioskId == kiosk.KioskId && x.IsDisposed == false
            );

            if (kioskDevices.Count > 0)
            {
                foreach (var kioskDevice in kioskDevices)
                {
                    kioskDevice.Delete();
                    await _deviceManager.RemoveHubDevice(kioskDevice.DeviceId!);
                    _unitOfWork.GetRepository<KioskDeviceMapping>().Update(kioskDevice);

                    var device = await _unitOfWork.GetRepository<Device>().SingleOrDefaultAsync(
                        predicate: x => x.DeviceId == kioskDevice.DeviceId
                    );

                    if (device is not null)
                    {
                        device.Stock();
                        device.DownHub();
                        _unitOfWork.GetRepository<Device>().Update(device);
                    }
                }
            }

            var result = await RemoveTunnelForKioskAsync(kiosk.KioskId, kiosk.Hostname!);

            if (!result)
            {
                return new BaseResult<string, KioskDto>()
                {
                    IsSuccess = false,
                    Message = MessageUtil.RemoveTunnelFailure<Kiosk>(),
                    Request = kioskId,
                    Response = null,
                    StatusCode = StatusCodes.Status503ServiceUnavailable
                };
            }

            kiosk.Delete();

            _unitOfWork.GetRepository<Kiosk>().Update(kiosk);
            await _unitOfWork.CommitAsync();

            var kioskDto = _mapper.Map<KioskDto>(kiosk);

            LogMessage(LogLevel.Information, "Out RemoveKiosk", kioskDto);
            return new BaseResult<string, KioskDto>()
            {
                IsSuccess = true,
                Message = MessageUtil.DeleteSuccess<Kiosk>(),
                Request = kioskId,
                Response = kioskDto,
                StatusCode = StatusCodes.Status202Accepted
            };
        }

        public async Task<BaseResult<AddKioskDeviceDto, KioskDeviceDto>> AddKioskDevice(
            AddKioskDeviceDto addKioskDeviceDto)
        {
            var kiosk = await _unitOfWork.GetRepository<Kiosk>().SingleOrDefaultAsync(
                predicate: x => x.KioskId == addKioskDeviceDto.KioskId
            );

            if (kiosk is null)
            {
                return new BaseResult<AddKioskDeviceDto, KioskDeviceDto>()
                {
                    Request = addKioskDeviceDto,
                    Response = null,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = MessageUtil.NotFound<Kiosk>(),
                    IsSuccess = false
                };
            }

            var device = await _unitOfWork.GetRepository<Device>().SingleOrDefaultAsync(
                predicate: x => x.DeviceId == addKioskDeviceDto.DeviceId
            );

            if (device is null)
            {
                return new BaseResult<AddKioskDeviceDto, KioskDeviceDto>()
                {
                    Request = addKioskDeviceDto,
                    Response = null,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = MessageUtil.NotFound<Device>(),
                    IsSuccess = false
                };
            }

            if (device.IsDeleted)
            {
                return new BaseResult<AddKioskDeviceDto, KioskDeviceDto>()
                {
                    Request = addKioskDeviceDto,
                    Response = null,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = MessageUtil.NotFound<Device>(),
                    IsSuccess = false
                };
            }

            if (device.Status != EDeviceStatus.Stock.ToString())
            {
                return new BaseResult<AddKioskDeviceDto, KioskDeviceDto>()
                {
                    Request = addKioskDeviceDto,
                    Response = null,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = MessageUtil.DeviceStatusError(Enum.Parse<EDeviceStatus>(device.Status)),
                    IsSuccess = false
                };
            }

            var kioskDevice = new KioskDeviceMapping
            {
                KioskDeviceMappingId = Guid.NewGuid().ToString(),
                Status = EKioskDeviceStatus.Online.ToString(),
                DeviceId = addKioskDeviceDto.DeviceId,
                KioskId = addKioskDeviceDto.KioskId,
                Note = "Newly registered device",
                CreatedDate = DateTime.UtcNow,
            };

            device.Working();

            await _unitOfWork.GetRepository<KioskDeviceMapping>().InsertAsync(kioskDevice);
            _unitOfWork.GetRepository<Device>().Update(device);

            await _unitOfWork.CommitAsync();

            var kioskDeviceDto = _mapper.Map<KioskDeviceDto>(kioskDevice);

            return new BaseResult<AddKioskDeviceDto, KioskDeviceDto>()
            {
                IsSuccess = true,
                Message = MessageUtil.AddSuccess<Device>(),
                StatusCode = StatusCodes.Status200OK,
                Response = kioskDeviceDto
            };
        }

        public async Task<BaseResult<string, KioskDeviceDto>> ChangeKioskDeviceStatus(string kioskDeviceId,
            ChangeKioskDeviceStatusDto changeKioskDeviceStatusDto)
        {
            var kioskDevice = await _unitOfWork.GetRepository<KioskDeviceMapping>().SingleOrDefaultAsync(
                predicate: x => x.KioskDeviceMappingId == kioskDeviceId
            );

            if (kioskDevice is null)
            {
                return new BaseResult<string, KioskDeviceDto>()
                {
                    IsSuccess = false,
                    Message = MessageUtil.NotFound<Device>(),
                    StatusCode = StatusCodes.Status404NotFound,
                    Response = null,
                    Request = kioskDeviceId
                };
            }

            switch (changeKioskDeviceStatusDto.Status)
            {
                case nameof(EKioskDeviceStatus.Online):
                {
                    kioskDevice.Online("Device is online");
                    break;
                }
                case nameof(EKioskDeviceStatus.Offline):
                {
                    kioskDevice.Offline("Device is offline");
                    break;
                }
                case nameof(EKioskDeviceStatus.Warning):
                {
                    kioskDevice.Warning("Device is warning");
                    break;
                }
                case nameof(EKioskDeviceStatus.Error):
                {
                    kioskDevice.Error("Device is warning");
                    break;
                }
            }

            _unitOfWork.GetRepository<KioskDeviceMapping>().Update(kioskDevice);
            await _unitOfWork.CommitAsync();

            var kioskDeviceDto = _mapper.Map<KioskDeviceDto>(kioskDevice);

            return new BaseResult<string, KioskDeviceDto>()
            {
                IsSuccess = false,
                Message = MessageUtil.UpdateSuccess<Device>(),
                StatusCode = StatusCodes.Status200OK,
                Response = kioskDeviceDto,
                Request = kioskDeviceId
            };
        }

        public async Task<BaseResult<string, KioskDeviceDto>> DisposeKioskDevice(string kioskDeviceId)
        {
            var kioskDevice = await _unitOfWork.GetRepository<KioskDeviceMapping>().SingleOrDefaultAsync(
                predicate: x => x.KioskDeviceMappingId == kioskDeviceId
            );

            if (kioskDevice is null)
            {
                return new BaseResult<string, KioskDeviceDto>()
                {
                    IsSuccess = false,
                    Message = MessageUtil.NotFound<Device>(),
                    Request = kioskDeviceId,
                    Response = null,
                    StatusCode = StatusCodes.Status404NotFound
                };
            }


            var device = await _unitOfWork.GetRepository<Device>().SingleOrDefaultAsync(
                predicate: x => x.DeviceId == kioskDevice.DeviceId
            );

            if (device is null)
            {
                return new BaseResult<string, KioskDeviceDto>()
                {
                    IsSuccess = false,
                    Message = MessageUtil.NotFound<Device>(),
                    Request = kioskDeviceId,
                    Response = null,
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            device.Stock();
            kioskDevice.Dispose();

            _unitOfWork.GetRepository<Device>().Update(device);
            _unitOfWork.GetRepository<KioskDeviceMapping>().Update(kioskDevice);

            await _unitOfWork.CommitAsync();

            var kioskDeviceDto = _mapper.Map<KioskDeviceDto>(kioskDevice);

            return new BaseResult<string, KioskDeviceDto>()
            {
                IsSuccess = true,
                Message = MessageUtil.UpdateSuccess<Device>(),
                Request = kioskDeviceId,
                Response = kioskDeviceDto,
                StatusCode = StatusCodes.Status202Accepted
            };
        }


        public async Task<BaseResult<ReplaceDeviceDto, KioskDeviceDto>> ReplaceDevice(string kioskDeviceId,
            ReplaceDeviceDto replaceDeviceDto)
        {
            var kioskDevice = await _unitOfWork.GetRepository<KioskDeviceMapping>().SingleOrDefaultAsync(
                predicate: x => x.KioskDeviceMappingId == kioskDeviceId
            );

            if (kioskDevice is null)
            {
                return new BaseResult<ReplaceDeviceDto, KioskDeviceDto>()
                {
                    IsSuccess = false,
                    Message = MessageUtil.NotFound<Device>(),
                    Request = replaceDeviceDto,
                    Response = null,
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            var kiosk = await _unitOfWork.GetRepository<Kiosk>().SingleOrDefaultAsync(
                predicate: x => x.KioskId == kioskDevice.KioskId
            );

            if (kiosk is null)
            {
                return new BaseResult<ReplaceDeviceDto, KioskDeviceDto>()
                {
                    IsSuccess = false,
                    Message = MessageUtil.NotFound<Kiosk>(),
                    Request = replaceDeviceDto,
                    Response = null,
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            if (kiosk.Status == EBaseStatus.Active.ToString())
            {
                return new BaseResult<ReplaceDeviceDto, KioskDeviceDto>()
                {
                    IsSuccess = false,
                    Message = MessageUtil.ReplaceDeviceInvalid<KioskDeviceMapping>(),
                    Request = replaceDeviceDto,
                    Response = null,
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }

            var deviceToReplace = await _unitOfWork.GetRepository<Device>().SingleOrDefaultAsync(
                predicate: x => x.DeviceId == replaceDeviceDto.DeviceReplaceId
            );

            if (deviceToReplace is null)
            {
                return new BaseResult<ReplaceDeviceDto, KioskDeviceDto>()
                {
                    IsSuccess = false,
                    Message = MessageUtil.NotFound<Device>(),
                    Request = replaceDeviceDto,
                    Response = null,
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            if (deviceToReplace.Status != EDeviceStatus.Stock.ToString())
            {
                return new BaseResult<ReplaceDeviceDto, KioskDeviceDto>()
                {
                    IsSuccess = false,
                    Message = MessageUtil.DeviceStatusError(Enum.Parse<EDeviceStatus>(deviceToReplace.Status)),
                    Request = replaceDeviceDto,
                    Response = null,
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }

            var existDevice = await _unitOfWork.GetRepository<Device>().SingleOrDefaultAsync(
                predicate: x => x.DeviceId == kioskDevice.DeviceId
            );

            if (existDevice is null)
            {
                return new BaseResult<ReplaceDeviceDto, KioskDeviceDto>()
                {
                    IsSuccess = false,
                    Message = MessageUtil.NotFound<Device>(),
                    Request = replaceDeviceDto,
                    Response = null,
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            if (existDevice.DeviceModelId != deviceToReplace.DeviceModelId)
            {
                return new BaseResult<ReplaceDeviceDto, KioskDeviceDto>()
                {
                    IsSuccess = false,
                    Message = MessageUtil.Invalid<Device>(),
                    Request = replaceDeviceDto,
                    Response = null,
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }

            kioskDevice.Dispose();
            _unitOfWork.GetRepository<KioskDeviceMapping>().Update(kioskDevice);

            var newKioskDevice = new KioskDeviceMapping()
            {
                KioskDeviceMappingId = Guid.NewGuid().ToString(),
                DeviceId = deviceToReplace.DeviceId,
                KioskId = kioskDevice.KioskId,
                CreatedDate = DateTime.UtcNow,
                IsDisposed = false,
                IsDeleted = false,
                DisposedDate = null,
                DeletedDate = null,
                Status = EKioskDeviceStatus.Online.ToString()
            };

            existDevice.Stock();
            existDevice.DownHub();

            await _deviceManager.RemoveHubDevice(kioskDevice.DeviceId!);

            deviceToReplace.Working();

            if (!deviceToReplace.IsOnHub)
            {
                var deviceOnHub = await _deviceManager.AddHubDevice(deviceToReplace.DeviceId);

                if (deviceOnHub is null)
                {
                    return new BaseResult<ReplaceDeviceDto, KioskDeviceDto>()
                    {
                        IsSuccess = false,
                        Message = MessageUtil.CreateOnHubFailure<Device>(),
                        Request = replaceDeviceDto,
                        Response = null,
                        StatusCode = StatusCodes.Status503ServiceUnavailable
                    };
                }

                deviceToReplace.OnHub();
            }

            await _unitOfWork.GetRepository<KioskDeviceMapping>().InsertAsync(newKioskDevice);
            _unitOfWork.GetRepository<Device>().Update(existDevice);
            _unitOfWork.GetRepository<Device>().Update(deviceToReplace);

            await _unitOfWork.CommitAsync();

            var kioskDeviceDto = _mapper.Map<KioskDeviceDto>(kioskDevice);
            return new BaseResult<ReplaceDeviceDto, KioskDeviceDto>()
            {
                IsSuccess = true,
                Message = MessageUtil.UpdateSuccess<Device>(),
                Request = replaceDeviceDto,
                Response = kioskDeviceDto,
                StatusCode = StatusCodes.Status202Accepted
            };
        }

        public async Task<BaseResult<string, KioskDeviceOnHubDto>> GetKioskDeviceOnHub(string kioskDeviceId)
        {
            var kioskDevice = await _unitOfWork.GetRepository<KioskDeviceMapping>().SingleOrDefaultAsync(
                predicate: x => x.KioskDeviceMappingId == kioskDeviceId
            );

            if (kioskDevice is null)
            {
                return new BaseResult<string, KioskDeviceOnHubDto>()
                {
                    IsSuccess = false,
                    Message = MessageUtil.NotFound<Device>(),
                    Request = kioskDeviceId,
                    Response = null,
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            var deviceId = kioskDevice.DeviceId!;

            var deviceOnHub = await _deviceManager.GetHubDevice(deviceId);

            if (deviceOnHub is null)
            {
                return new BaseResult<string, KioskDeviceOnHubDto>()
                {
                    IsSuccess = false,
                    Message = MessageUtil.GetOnHubFailure<Device>(),
                    Request = deviceId,
                    StatusCode = StatusCodes.Status503ServiceUnavailable,
                    Response = null
                };
            }

            var kioskDeviceOnHubDto = new KioskDeviceOnHubDto()
            {
                Status = deviceOnHub.Status.ToString(),
                StatusUpdatedTime = deviceOnHub.StatusUpdatedTime,
                ConnectionState = deviceOnHub.ConnectionState.ToString(),
                ConnectionStateUpdatedTime = deviceOnHub.ConnectionStateUpdatedTime,
                LastActivityTime = deviceOnHub.LastActivityTime,
                CloudToDeviceMessageCount = deviceOnHub.CloudToDeviceMessageCount,
                ConnectionString = _deviceManager.BuildDevicePrimaryConnectionStr(deviceOnHub)
            };

            return new BaseResult<string, KioskDeviceOnHubDto>()
            {
                IsSuccess = true,
                Message = MessageUtil.GetOnHubSuccess<Device>(),
                Request = deviceId,
                StatusCode = StatusCodes.Status200OK,
                Response = kioskDeviceOnHubDto
            };
        }

        public async Task<BaseResult<string, BaseResult<List<KioskDeviceOnPlaceDto>>>> GetKioskDeviceOnPlace(
            string kioskId,
            KioskDeviceOnPlaceQueryDto kioskDeviceOnPlaceQueryDto
        )
        {
            var kiosk = await _unitOfWork.GetRepository<Kiosk>().SingleOrDefaultAsync(
                predicate: x => x.KioskId == kioskId
            );

            if (kiosk is null)
            {
                return new BaseResult<string, BaseResult<List<KioskDeviceOnPlaceDto>>>()
                {
                    IsSuccess = false,
                    Message = MessageUtil.NotFound<Kiosk>(),
                    Request = kioskId,
                    Response = null,
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            var webhook = await _unitOfWork.GetRepository<Webhook>().SingleOrDefaultAsync(
                predicate: x => x.KioskId == kioskId && x.WebhookType == EWebhookType.RetrieveDevice.ToString()
            );

            if (webhook is null)
            {
                return new BaseResult<string, BaseResult<List<KioskDeviceOnPlaceDto>>>()
                {
                    IsSuccess = false,
                    Message = MessageUtil.NotFound<Webhook>(),
                    Request = kioskId,
                    Response = null,
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            var result = await ApiUtil.GetAsync(
                webhook.WebhookUrl,
                queryParams: new Dictionary<string, string?>()
                {
                    { "WorkingStatus", kioskDeviceOnPlaceQueryDto.WorkingStatus },
                    { "DeviceModelId", kioskDeviceOnPlaceQueryDto.DeviceModelId }
                },
                headers: new Dictionary<string, string>()
                {
                    { "X-API-KEY", ApiKeyUtil.Decrypt(kiosk.ApiKey!) }
                });

            if (!result.IsSuccessStatusCode)
            {
                return new BaseResult<string, BaseResult<List<KioskDeviceOnPlaceDto>>>()
                {
                    IsSuccess = false,
                    Message = MessageUtil.NoResponse<Device>(),
                    Request = kioskId,
                    Response = null,
                    StatusCode = StatusCodes.Status503ServiceUnavailable
                };
            }

            var kioskDeviceOnPlaceDto = ApiUtil.HandleResponse<BaseResult<List<KioskDeviceOnPlaceDto>>>(result);

            return new BaseResult<string, BaseResult<List<KioskDeviceOnPlaceDto>>>()
            {
                IsSuccess = true,
                Message = MessageUtil.ReadSuccess<Device>(),
                Request = kioskId,
                Response = kioskDeviceOnPlaceDto,
                StatusCode = StatusCodes.Status200OK
            };
        }

        public async Task<MemoryStream?> ExportKioskSetup(string kioskId)
        {
            var kiosk = await _unitOfWork.GetRepository<Kiosk>().SingleOrDefaultAsync(
                predicate: x => x.KioskId == kioskId,
                include: x => x
                    .Include(x => x.KioskVersion)
                    .ThenInclude(x => x.KioskType)
                    .Include(k => k.KioskDevices)
                    .ThenInclude(d => d.Device)
                    .ThenInclude(dm => dm.DeviceModel)
                    .ThenInclude(x => x.DeviceType)
            );

            if (kiosk is null)
            {
                return null;
            }

            var tunnelToken = await _cloudflareApi.GetTunnelTokenAsync(kiosk.KioskId);

            if (tunnelToken is null)
            {
                return null;
            }

            var azureServiceConn = _configuration["AzureIotHub:Service"];

            if (azureServiceConn is null)
            {
                return null;
            }

            var memoryStream = _memoryStreamManager.GetStream();
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                var kioskIdentify = kiosk.KioskId;
                var kioskVersionTitle = kiosk.KioskVersion!.VersionTitle.Trim().ToLower().Replace(" ", "");
                var kioskTypeName = kiosk.KioskVersion.KioskType!.Name.Trim().ToLower().Replace(" ", "");

                var folderName = $"kiosk_{kioskIdentify}_{kioskVersionTitle}_{kioskTypeName}_setup";

                var kioskEnv = archive.CreateEntry($"{folderName}/kiosk/.env");
                await using (var entryStream = kioskEnv.Open())
                await using (var streamWriter = new StreamWriter(entryStream))
                {
                    await streamWriter.WriteLineAsync($"TUNNEL_TOKEN={tunnelToken}");
                    await streamWriter.WriteLineAsync($"ASPNETCORE_ENVIRONMENT=Production");
                    await streamWriter.WriteLineAsync($"AzureServiceConn={azureServiceConn}");
                    await streamWriter.WriteLineAsync($"ApiKey={ApiKeyUtil.Decrypt(kiosk.ApiKey ?? "")}");
                }

                const string serialPortName = "COM";
                const int baudRate = 115200;

                foreach (var kioskDevice in kiosk.KioskDevices.Where(x => x.IsDisposed == false))
                {
                    var device = kioskDevice.Device;

                    if (device is not null)
                    {
                        var model = device.DeviceModel;


                        if (device.DeviceModel!.DeviceType!.IsMobileDevice)
                        {
                            if (model is not null)
                            {
                                var deviceEnv = archive.CreateEntry(
                                    $"{folderName}/{model.ModelName}_{device.DeviceId}/.env");

                                await using var entryStream = deviceEnv.Open();
                                await using var streamWriter = new StreamWriter(entryStream);
                                await streamWriter.WriteLineAsync($"API_KEY={ApiKeyUtil.Decrypt(kiosk.ApiKey!)}");
                                await streamWriter.WriteLineAsync($"CLIENT_ID={device.DeviceId}");
                                await streamWriter.WriteLineAsync($"KIOSK_ID={kiosk.KioskId}");
                                await streamWriter.WriteLineAsync($"SIDE={kioskDevice.Side}");
                            }
                        }
                        else
                        {
                            var deviceOnHub = await _deviceManager.GetHubDevice(device.DeviceId);

                            if (deviceOnHub is null)
                            {
                                continue;
                            }

                            var devicePrimaryConnStr = _deviceManager.BuildDevicePrimaryConnectionStr(deviceOnHub);

                            if (model is not null)
                            {
                                var deviceEnv =
                                    archive.CreateEntry($"{folderName}/{model.ModelName}_{device.DeviceId}/.env");

                                await using var entryStream = deviceEnv.Open();
                                await using var streamWriter = new StreamWriter(entryStream);

                                await streamWriter.WriteLineAsync($"DEVICE_PRIMARY_CONN_STR={devicePrimaryConnStr}");
                                await streamWriter.WriteLineAsync($"SERIAL_PORT={serialPortName}");
                                await streamWriter.WriteLineAsync($"BAUD_RATE={baudRate}");
                                await streamWriter.WriteLineAsync($"KIOSKID={kiosk.KioskId}");
                            }
                        }
                    }
                }
            }

            memoryStream.Position = 0;

            return memoryStream;
        }

        /// <summary>
        /// delete tunnel flow
        /// </summary>
        /// <param name="tunnelId"></param>
        /// <param name="hostname"></param>
        /// <returns></returns>
        private async Task<bool> RemoveTunnelForKioskAsync(string tunnelId, string hostname)
        {
            try
            {
                var deleteTunnelTask = _cloudflareApi.DeleteTunnelAsync(tunnelId);

                var dnsRecordTask = _cloudflareApi.GetDNSRecordByTunnelHostname(hostname)
                    .ContinueWith(async dnsTask =>
                    {
                        var dnsRecord = await dnsTask;
                        if (dnsRecord != null)
                        {
                            await _cloudflareApi.DeleteDnsRecordAsync(hostname);
                        }
                    }).Unwrap();

                await Task.WhenAll(deleteTunnelTask, dnsRecordTask);
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<BaseResult<AssignKioskMenuDto, KioskDto>> AssignKioskMenu(
            AssignKioskMenuDto assignKioskMenuDto)
        {
            var accountId = GetAccountIdFromJwt();
            var account = await _unitOfWork.GetRepository<Account>().SingleOrDefaultAsync(
                predicate: x => x.AccountId == accountId
            );

            if (account is null)
            {
                return new BaseResult<AssignKioskMenuDto, KioskDto>()
                {
                    IsSuccess = false,
                    Message = MessageUtil.NotFound<Account>(),
                    Request = assignKioskMenuDto,
                    StatusCode = StatusCodes.Status404NotFound,
                    Response = null
                };
            }

            var organization = await _unitOfWork.GetRepository<Organization>().SingleOrDefaultAsync(
                predicate: x => x.OrganizationId == account.OrganizationId
            );

            if (organization is null)
            {
                return new BaseResult<AssignKioskMenuDto, KioskDto>()
                {
                    IsSuccess = false,
                    Message = MessageUtil.NotFound<Organization>(),
                    Request = assignKioskMenuDto,
                    StatusCode = StatusCodes.Status404NotFound,
                    Response = null
                };
            }

            var menu = await _unitOfWork.GetRepository<Menu>().SingleOrDefaultAsync(
                predicate: x => x.MenuId == assignKioskMenuDto.MenuId && x.OrganizationId == organization.OrganizationId
            );

            if (menu is null)
            {
                return new BaseResult<AssignKioskMenuDto, KioskDto>()
                {
                    IsSuccess = false,
                    Message = MessageUtil.NotFound<Menu>(),
                    Request = assignKioskMenuDto,
                    StatusCode = StatusCodes.Status404NotFound,
                    Response = null
                };
            }

            var stores = await _unitOfWork.GetRepository<Store>().GetListAsync(
                predicate: x => x.OrganizationId == organization.OrganizationId
            );

            var storeIds = stores.Select(x => x.StoreId);

            var kiosk = await _unitOfWork.GetRepository<Kiosk>().SingleOrDefaultAsync(
                predicate: x => x.KioskId == assignKioskMenuDto.KioskId && storeIds.Contains(x.StoreId)
            );

            if (kiosk is null)
            {
                return new BaseResult<AssignKioskMenuDto, KioskDto>()
                {
                    IsSuccess = false,
                    Message = MessageUtil.NotFound<Kiosk>(),
                    Request = assignKioskMenuDto,
                    StatusCode = StatusCodes.Status404NotFound,
                    Response = null
                };
            }

            kiosk.MenuId = assignKioskMenuDto.MenuId;
            _unitOfWork.GetRepository<Kiosk>().Update(kiosk);
            await _unitOfWork.CommitAsync();


            var kioskDto = _mapper.Map<KioskDto>(kiosk);

            return new BaseResult<AssignKioskMenuDto, KioskDto>()
            {
                IsSuccess = true,
                Message = MessageUtil.UpdateSuccess<Kiosk>(),
                Request = assignKioskMenuDto,
                StatusCode = StatusCodes.Status202Accepted,
                Response = kioskDto
            };
        }

        public async Task RemoveAllKioskDeviceOnHub()
        {
            var devices = await _unitOfWork.GetRepository<Device>().GetListAsync();
            foreach (var device in devices)
            {
                await _deviceManager.RemoveHubDevice(device.DeviceId!);
            }
        }

        public async Task<BaseResult<string, KioskDto>> Clean()
        {
            var kioskId = GetKioskIdFromJwt();

            var kiosk = await _unitOfWork.GetRepository<Kiosk>().SingleOrDefaultAsync(
                predicate: x => x.KioskId == kioskId
            );

            if (kiosk is null)
            {
                return new BaseResult<string, KioskDto>()
                {
                    IsSuccess = false,
                    Message = MessageUtil.NotFound<Kiosk>(),
                    Request = kioskId,
                    StatusCode = StatusCodes.Status404NotFound,
                    Response = null
                };
            }

            var webhook = await _unitOfWork.GetRepository<Webhook>().SingleOrDefaultAsync(
                predicate: x => x.KioskId == kiosk.KioskId && x.WebhookType == EWebhookType.ExecuteClean.ToString()
            );

            if (webhook is null)
            {
                return new BaseResult<string, KioskDto>()
                {
                    IsSuccess = false,
                    Message = MessageUtil.NotFound<Webhook>(),
                    Request = kioskId,
                    StatusCode = StatusCodes.Status404NotFound,
                    Response = null
                };
            }

            var workflow = await _unitOfWork.GetRepository<Workflow>().SingleOrDefaultAsync(
                predicate: x =>
                    x.KioskVersionId == kiosk.KioskVersionId && x.Type == EWebhookType.ExecuteClean.ToString()
            );

            if (workflow is null)
            {
                return new BaseResult<string, KioskDto>()
                {
                    IsSuccess = false,
                    Message = MessageUtil.NotFound<Workflow>(),
                    Request = kioskId,
                    StatusCode = StatusCodes.Status404NotFound,
                    Response = null
                };
            }

            var result = await ApiUtil.PostAsync(
                webhook.WebhookUrl,
                headers:
                new Dictionary<string, string>()
                {
                    { "X-API-KEY", ApiKeyUtil.Decrypt(kiosk.ApiKey!) }
                }
            );

            if (!result.IsSuccessStatusCode)
            {
                return new BaseResult<string, KioskDto>()
                {
                    IsSuccess = false,
                    Message = MessageUtil.NoResponse<Kiosk>(),
                    Request = kioskId,
                    StatusCode = StatusCodes.Status503ServiceUnavailable,
                    Response = null
                };
            }

            return new BaseResult<string, KioskDto>()
            {
                IsSuccess = true,
                Message = MessageUtil.NotifySuccess<Kiosk>(),
                Request = kioskId,
                Response = null
            };
        }

        public Task<BaseResult<string, KioskDto>> Ping()
        {
            throw new NotImplementedException();
        }

        //orgCode == tunnelName pattern
        private async Task<TunnelConfiguration?> SetUpTunnelForKioskInStoreAsync(string orgCode)
        {
            //Check tunnel name existed or not
            var tunnelList = await _cloudflareApi.GetTunnelsAsync();

            int numberOfSameTunnelName = tunnelList.Where(t => t.Name.Contains(orgCode.ToLower())).ToList().Count;
            int newIndex = numberOfSameTunnelName + 1;
            var appliedName = $"{orgCode.ToLower()}{newIndex}";
            //Step1: create tunnel
            var tunnel = await _cloudflareApi.CreateTunnelAsync(appliedName);

            //Step2: create dns record
            var dns = await _cloudflareApi.CreateDnsRecordAsync(appliedName, tunnel.Id);

            //Step3: update tunnel to add dns as public hostname
            var tunnelConfiguration =
                await _cloudflareApi.UpdateTunnelConfigurationAsync(tunnel.Id,
                    $"{appliedName}.{_cloudflareApi.ZoneDomain}");
            return tunnelConfiguration;
        }
    }
}