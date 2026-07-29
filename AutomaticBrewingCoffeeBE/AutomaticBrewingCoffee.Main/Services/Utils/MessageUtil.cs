using AutomaticBrewingCoffee.Domain.Enums;

namespace Services.Utils
{
    public static class EntityNameMapping
    {
        private static readonly Dictionary<string, string> VietnameseMapping = new()
        {
            { "DeviceModel", "loại thiết bị" },
            { "Device", "thiết bị" },
            { "DeviceType", "loại thiết bị" },
            { "Kiosk", "ki-ốt" },
            { "KioskType", "loại ki-ốt" },
            { "KioskVersion", "phiên bản ki-ốt" },
            { "LocationType", "kiểu địa điểm" },
            { "Menu", "thực đơn" },
            { "Order", "đơn hàng" },
            { "OrderDetail", "chi tiết đơn hàng" },
            { "Organization", "tổ chức" },
            { "Product", "sản phẩm" },
            { "Store", "cửa hàng" },
            { "Workflow", "quy trình" },
            { "Step", "bước" },
            { "Webhook", "liên kết web" },
            { "Account", "tài khoản" },
            { "TunnelConfigurationDetail ", "đường hầm đám mây tia sáng" },
            { "RoleName", "vai trò" },
            { "ChangePasswordDto", "mật khẩu" },
            { "ProductCategory", "danh mục" },
            { "DeviceIngredientState", "nguyên liệu trong thiết bị" },
            { "SyncEvent", "sự kiện đồng bộ" },
            { "SyncTask", "tác vụ đồng bộ" },
            { "IngredientType", "loại nguyên liệu" },
            { "MenuProductMapping", "sản phẩm trong thực đơn" },
            { "RefundOrderDto", "yêu cầu hoàn tiền" },
            { "Notification", "thông báo" },
            { "KioskDeviceMapping", "thiết bị trong ki-ốt" },
            { "HourlyPeakDto", "Giờ bán hàng cao nhất" }
        };

        public static string GetDisplayName<T>()
        {
            var typeName = typeof(T).Name;
            return VietnameseMapping.TryGetValue(typeName, out var value) ? value : typeName;
        }
    }

    public enum ELanguage
    {
        English,
        Vietnamese
    }

    public static class MessageUtil
    {
        // Default ELanguage
        private static ELanguage CurrentELanguage { get; set; } = ELanguage.Vietnamese;

        public static void SetELanguage(ELanguage eLanguage)
        {
            CurrentELanguage = eLanguage;
        }

        // Lấy tên hiển thị của T
        private static string GetEntityDisplayName<T>(bool capitalizeFirst = false)
        {
            var name = CurrentELanguage == ELanguage.English
                ? typeof(T).Name
                : EntityNameMapping.GetDisplayName<T>();

            return capitalizeFirst ? char.ToUpper(name[0]) + name.Substring(1) : name;
        }


        # region Sync Messages

        public static string SyncSuccess<T>()
        {
            return CurrentELanguage == ELanguage.English
                ? $"{typeof(T).Name} synced successfully."
                : $"Đồng bộ {GetEntityDisplayName<T>()} thành công.";
        }

        public static string SyncFailure<T>()
        {
            return CurrentELanguage == ELanguage.English
                ? $"{typeof(T).Name} synced failed."
                : $"Đồng bộ {GetEntityDisplayName<T>()} không thành công.";
        }

        #endregion Sync Messages


        #region Hub Messages

        public static string CreateOnHubFailure<T>()
        {
            return CurrentELanguage == ELanguage.English
                ? $"Failed to create {typeof(T).Name} on hub."
                : $"Tạo {GetEntityDisplayName<T>()} trên hub thất bại.";
        }

        public static string GetOnHubFailure<T>()
        {
            return CurrentELanguage == ELanguage.English
                ? $"Failed to get {typeof(T).Name} on hub."
                : $"Không tìm thấy {GetEntityDisplayName<T>()} trên hub.";
        }

        public static string GetOnHubSuccess<T>()
        {
            return CurrentELanguage == ELanguage.English
                ? $"Found {typeof(T).Name} on hub."
                : $"Đã tìm thấy {GetEntityDisplayName<T>()} trên hub.";
        }

        #endregion Hub Messages


        #region Cloudflare Messages

        public static string RemoveTunnelFailure<T>()
        {
            return CurrentELanguage == ELanguage.English
                ? $"Remove {typeof(T).Name} on the tunnel remove fail."
                : $"Xóa {GetEntityDisplayName<T>()} trên tunnel xóa không thành công.";
        }

        #endregion Cloudflare Messages

        public static string Accept<T>()
        {
            return CurrentELanguage == ELanguage.English
                ? $"{typeof(T).Name} accepted."
                : $"Đã xác nhận {GetEntityDisplayName<T>()}.";
        }

        public static string RequireChildEntity<T>()
        {
            return CurrentELanguage == ELanguage.English
                ? $"{typeof(T).Name} requires at least one {typeof(T).Name} child to use."
                : $"{GetEntityDisplayName<T>(true)} cần ít nhất một {GetEntityDisplayName<T>()} con để sử dụng.";
        }

        public static string Reject<T>()
        {
            return CurrentELanguage == ELanguage.English
                ? $"{typeof(T).Name} rejected."
                : $"Đã từ chối {GetEntityDisplayName<T>()}.";
        }

        public static string CreateSuccess<T>()
        {
            return CurrentELanguage == ELanguage.English
                ? $"Create {typeof(T).Name} successfully."
                : $"Tạo {GetEntityDisplayName<T>()} thành công.";
        }

        public static string MissingChildProducts(string versionName, IEnumerable<string> missingParents)
        {
            if (CurrentELanguage == ELanguage.English)
            {
                return $"Cannot assign menu to kiosk because version '{versionName}' " +
                       $"does not support *at least one* child product for the following parent products:\n- " +
                       string.Join("\n- ", missingParents);
            }
            else
            {
                return $"Không thể gán menu cho ki-ốt vì phiên bản '{versionName}' " +
                       $"không hỗ trợ *ít nhất một* sản phẩm con cho các sản phẩm cha:\n- " +
                       string.Join("\n- ", missingParents);
            }
        }
        
        public static string MissingWorkflows<TParent, TChild>(IEnumerable<string> errors)
        {
            if (CurrentELanguage == ELanguage.English)
            {
                return
                    $"Cannot add {typeof(TParent).Name} to {typeof(TChild).Name} because some kiosks do not fully support:\n- "
                    + string.Join("\n- ", errors);
            }
            else
            {
                return $"Không thể thêm {GetEntityDisplayName<TParent>()} vào {GetEntityDisplayName<TChild>()} " +
                       $"vì một số sản phẩm không có quy trình :\n- " + string.Join("\n- ", errors);
            }
        }


        public static string UnsupportedEntities<TParent, TChild>(IEnumerable<string> errors)
        {
            if (CurrentELanguage == ELanguage.English)
            {
                return
                    $"Cannot add {typeof(TParent).Name} to {typeof(TChild).Name} because some kiosks do not fully support:\n- "
                    + string.Join("\n- ", errors);
            }
            else
            {
                return $"Không thể thêm {GetEntityDisplayName<TParent>()} vào {GetEntityDisplayName<TChild>()} " +
                       $"vì một số ki-ốt không hỗ trợ đầy đủ:\n- " + string.Join("\n- ", errors);
            }
        }

        public static string CreateFailure<T>()
        {
            return CurrentELanguage == ELanguage.English
                ? $"Failed to create {typeof(T).Name}."
                : $"Tạo {GetEntityDisplayName<T>()} thất bại.";
        }

        public static string NotEnough<T>()
        {
            return CurrentELanguage == ELanguage.English
                ? $"Not enough {typeof(T).Name}."
                : $"Không đủ {GetEntityDisplayName<T>()}.";
        }

        public static string AddSuccess<T>()
        {
            return CurrentELanguage == ELanguage.English
                ? $"{typeof(T).Name} added successfully."
                : $"Thêm {GetEntityDisplayName<T>()} thành công.";
        }

        public static string AddFailure<T>()
        {
            return CurrentELanguage == ELanguage.English
                ? $"Failed to add {typeof(T).Name}."
                : $"Thêm {GetEntityDisplayName<T>()} thất bại.";
        }

        public static string ReadSuccess<T>()
        {
            return CurrentELanguage == ELanguage.English
                ? $"{typeof(T).Name} found."
                : $"Đã tìm thấy {GetEntityDisplayName<T>()}.";
        }

        public static string ReadFailure<T>()
        {
            return CurrentELanguage == ELanguage.English
                ? $"{typeof(T).Name} not found."
                : $"Không tìm thấy {GetEntityDisplayName<T>()}.";
        }

        public static string UpdateSuccess<T>()
        {
            return CurrentELanguage == ELanguage.English
                ? $"{typeof(T).Name} updated successfully."
                : $"Cập nhật {GetEntityDisplayName<T>()} thành công.";
        }

        public static string UpdateFailure<T>()
        {
            return CurrentELanguage == ELanguage.English
                ? $"Failed to update {typeof(T).Name}."
                : $"Cập nhật {GetEntityDisplayName<T>()} thất bại.";
        }

        public static string DeleteSuccess<T>()
        {
            return CurrentELanguage == ELanguage.English
                ? $"{typeof(T).Name} deleted successfully."
                : $"Xóa {GetEntityDisplayName<T>()} thành công.";
        }

        public static string DeleteFailure<T>()
        {
            return CurrentELanguage == ELanguage.English
                ? $"Failed to delete {typeof(T).Name}."
                : $"Xóa {GetEntityDisplayName<T>()} thất bại.";
        }

        public static string NotFound<T>()
        {
            return CurrentELanguage == ELanguage.English
                ? $"{typeof(T).Name} not found."
                : $"Không tìm thấy {GetEntityDisplayName<T>()}.";
        }

        public static string AlreadyExists<T>(string? key = null)
        {
            return CurrentELanguage == ELanguage.English
                ? $"{typeof(T).Name} already exists."
                : $"Đã tồn tại {GetEntityDisplayName<T>()} {key ?? ""} trong hệ thống.";
        }

        public static string AlreadyUsing<T>(string? inWhere = null)
        {
            return CurrentELanguage == ELanguage.English
                ? $"{typeof(T).Name} already using."
                : $"Đang sử dụng {GetEntityDisplayName<T>()} trong {inWhere ?? "hệ thống"}.";
        }

        public static string AlreadyUsing<TO, TW>()
        {
            return CurrentELanguage == ELanguage.English
                ? $"{typeof(TO).Name} already using."
                : $"Đang sử dụng {GetEntityDisplayName<TO>()} trong {GetEntityDisplayName<TW>()}.";
        }

        public static string AlreadyUsing<TM, TB, TW, TK>()
        {
            return CurrentELanguage == ELanguage.English
                ? $"{typeof(TM).Name} already using."
                : $"Đang sử dụng {GetEntityDisplayName<TM>()} bởi {GetEntityDisplayName<TB>()} trong {GetEntityDisplayName<TW>()} của {GetEntityDisplayName<TK>()}.";
        }

        public static string Invalid<T>()
        {
            return CurrentELanguage == ELanguage.English
                ? $"{typeof(T).Name} is invalid."
                : $"Không hợp lệ ở {GetEntityDisplayName<T>()}.";
        }

        public static string ReplaceDeviceInvalid<T>()
        {
            return CurrentELanguage == ELanguage.English
                ? $"{typeof(T).Name} is invalid to replace because kiosk still working, please inactive kiosk first."
                : $"Không thể thay thế {GetEntityDisplayName<T>()} do ki-ốt vẫn đang hoạt dộng, vui lòng tạm dừng ki-ốt trước khi thay thế.";
        }

        public static string Incorrect<T>()
        {
            return CurrentELanguage == ELanguage.English
                ? $"Incorrect {typeof(T).Name}."
                : $"Sai {GetEntityDisplayName<T>()}.";
        }

        public static string BanSuccess<T>()
        {
            return CurrentELanguage == ELanguage.English
                ? $"Ban {typeof(T).Name} successfully."
                : $"Đã khóa {GetEntityDisplayName<T>()}.";
        }

        public static string IsBan<T>()
        {
            return CurrentELanguage == ELanguage.English
                ? $"{typeof(T).Name} is banned."
                : $"{GetEntityDisplayName<T>(true)} đã bị khóa.";
        }

        public static string UnbanSuccess<T>()
        {
            return CurrentELanguage == ELanguage.English
                ? $"Unban {typeof(T).Name} successfully."
                : $"Đã mở khóa {GetEntityDisplayName<T>()}.";
        }

        public static string IsInactive<T>()
        {
            return CurrentELanguage == ELanguage.English
                ? $"{typeof(T).Name} is inactive."
                : $"{GetEntityDisplayName<T>(true)} hiện không khả dụng.";
        }

        public static string IsPause<T>()
        {
            return CurrentELanguage == ELanguage.English
                ? $"{typeof(T).Name} is pause."
                : $"{GetEntityDisplayName<T>(true)} hiện đang tạm ngừng.";
        }

        public static string NoResponse<T>()
        {
            return CurrentELanguage == ELanguage.English
                ? $"{typeof(T).Name} did not response."
                : $"{GetEntityDisplayName<T>(true)} không phản hồi.";
        }

        public static string Busy<T>()
        {
            return CurrentELanguage == ELanguage.English
                ? $"{typeof(T).Name} is busy."
                : $"{GetEntityDisplayName<T>(true)} đang bận.";
        }

        public static string NotifySuccess<T>()
        {
            return CurrentELanguage == ELanguage.English
                ? $"{typeof(T).Name} activated successfully."
                : $"{GetEntityDisplayName<T>(true)} kích hoạt thành công.";
        }

        public static string SummarizeSuccess<T>()
        {
            return CurrentELanguage == ELanguage.English
                ? $"{typeof(T).Name} summarize successfully."
                : $"Tóm tắt các {GetEntityDisplayName<T>()} thành công.";
        }

        public static string DeviceStatusError(EDeviceStatus status)
        {
            var statusDisplayName = status.ToString();

            if (CurrentELanguage == ELanguage.Vietnamese)
            {
                return status switch
                {
                    EDeviceStatus.Stock => "Thiết bị đang trong kho.",
                    EDeviceStatus.Working => "Thiết bị đang hoạt động.",
                    EDeviceStatus.Maintain => "Thiết bị đang bảo trì.",
                    _ => $"{statusDisplayName}, không thể thực hiện hành động."
                };
            }

            // Mặc định là tiếng Anh
            return $"Device is currently in status: {statusDisplayName}.";
        }

        public static string KioskDeviceStatusError(EKioskDeviceStatus status)
        {
            var statusDisplayName = status.ToString();

            if (CurrentELanguage == ELanguage.Vietnamese)
            {
                return status switch
                {
                    EKioskDeviceStatus.Online => "Thiết bị đang họat động.",
                    EKioskDeviceStatus.Error => "Thiết bị đang gặp sự cố.",
                    EKioskDeviceStatus.Offline => "Thiết bị đang ngoại tuyến.",
                    EKioskDeviceStatus.Warning => "Thiết bị đang cảnh báo.",
                    _ => $"Trạng thái của thiết bị trong ki-ốt: {statusDisplayName}."
                };
            }

            // Mặc định là tiếng Anh
            return $"Device is currently in status: {statusDisplayName}.";
        }

        public static string OrderStatusError(EOrderStatus status)
        {
            var statusDisplayName = status.ToString();

            if (CurrentELanguage == ELanguage.Vietnamese)
            {
                return status switch
                {
                    EOrderStatus.Pending => "Đơn hàng đang chờ.",
                    EOrderStatus.Preparing => "Đơn hàng đang làm bởi kiosk.",
                    EOrderStatus.Completed => "Đơn hàng đã hoàn thành.",
                    EOrderStatus.Cancelled => "Đơn hàng đã bị hủy.",
                    EOrderStatus.Failed => "Đơn hàng thất bại.",
                    _ => $"{statusDisplayName}, không thể thực hiện hành động."
                };
            }

            // Mặc định là tiếng Anh
            return $"Order is currently in status: {statusDisplayName}.";
        }
    }
}