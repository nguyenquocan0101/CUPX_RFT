import 'dart:convert';
import 'package:abc_androidapp/app/data/enums/device_status_enum.dart';
import 'package:abc_androidapp/app/data/models/arm_coordinate.dart';
import 'package:abc_androidapp/app/data/models/device.dart';
import 'package:abc_androidapp/app/data/models/device_location.dart';
import 'package:abc_androidapp/app/data/models/device_parameter.dart';
import 'package:abc_androidapp/app/data/models/organization/kiosk.dart';
import 'package:abc_androidapp/app/data/models/organization/organization.dart';
import 'package:abc_androidapp/app/data/models/organization/update_ingredient_request.dart';
import 'package:abc_androidapp/app/presentation/blocs/device/device_bloc.dart';
import 'package:abc_androidapp/app/presentation/blocs/device/device_event.dart';
import 'package:abc_androidapp/app/presentation/blocs/device/device_state.dart';
import 'package:abc_androidapp/app/presentation/blocs/kiosk/kiosk_bloc.dart';
import 'package:abc_androidapp/app/presentation/blocs/kiosk/kiosk_event.dart';
import 'package:abc_androidapp/app/presentation/blocs/kiosk/kiosk_state.dart';
import 'package:abc_androidapp/app/presentation/blocs/organization/organization_bloc.dart';
import 'package:abc_androidapp/app/presentation/blocs/organization/organization_event.dart';
import 'package:abc_androidapp/app/presentation/blocs/organization/organization_state.dart';
import 'package:abc_androidapp/app/presentation/widgets/common/dialog.dart';
import 'package:abc_androidapp/app/presentation/widgets/common/toast.dart';
import 'package:abc_androidapp/app/presentation/widgets/setting_screen/admin_auth_dialog.dart';
import 'package:abc_androidapp/app/presentation/widgets/setting_screen/device_info_card.dart';
import 'package:abc_androidapp/app/presentation/widgets/setting_screen/info_row.dart';
import 'package:abc_androidapp/app/presentation/widgets/setting_screen/refill_ingredient_dialog.dart';
import 'package:abc_androidapp/app/presentation/widgets/setting_screen/section_header.dart';
import 'package:abc_androidapp/app/presentation/widgets/setting_screen/setting_card.dart';
import 'package:abc_androidapp/config/themes/app_palette.dart';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:tdesign_flutter/tdesign_flutter.dart';
import 'package:web_socket_channel/web_socket_channel.dart';
import 'package:logger/logger.dart';
import 'package:web_socket_channel/status.dart' as status;

class SettingScreen extends StatefulWidget {
  static const String route = "/setting";
  const SettingScreen({super.key});

  @override
  State<SettingScreen> createState() => _SettingScreenState();
}

class _SettingScreenState extends State<SettingScreen> {
  final ScrollController _scrollController = ScrollController();
  bool _isAuthenticated = false;
  late String _adminPassword;
  String _activeSection = 'store';

  Organization? _organization;
  Kiosk? _kiosk;

  final GlobalKey _storeInfoKey = GlobalKey();
  final GlobalKey _devicesKey = GlobalKey();

  @override
  void initState() {
    super.initState();
    _adminPassword = _generatePasswordFromDate();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      _showPasswordDialog();
    });
    _scrollController.addListener(_updateActiveSection);
  }

  static String _generatePasswordFromDate() {
    final now = DateTime.now();
    final year = now.year % 100;
    final dateStr =
        '${now.day.toString().padLeft(2, '0')}${now.month.toString().padLeft(2, '0')}${year.toString().padLeft(2, '0')}';
    return dateStr.split('').reversed.join();
  }

   void _loadData() {
    _loadOrganization();
    _loadKiosk();
  }

  void _loadOrganization() {
    if (!mounted) return;
    context.read<OrganizationBloc>().add(GetOrganizationEvent());
  }

  void _loadKiosk() {
    if (!mounted) return;
    context.read<KioskBloc>().add(GetKioskEvent());
  }

  void _updateActiveSection() {
    if (!_scrollController.hasClients) return;

    final double offset = _scrollController.offset;
    final RenderBox? storeInfoBox =
        _storeInfoKey.currentContext?.findRenderObject() as RenderBox?;
    final RenderBox? devicesBox =
        _devicesKey.currentContext?.findRenderObject() as RenderBox?;

    if (storeInfoBox == null || devicesBox == null) return;

    final storeInfoPosition = storeInfoBox.localToGlobal(Offset.zero).dy;
    final devicesPosition = devicesBox.localToGlobal(Offset.zero).dy;

    String newActiveSection = _activeSection;

    if (offset < (devicesPosition - storeInfoPosition) / 2) {
      newActiveSection = 'store';
    } else {
      newActiveSection = 'devices';
    }

    if (newActiveSection != _activeSection) {
      setState(() {
        _activeSection = newActiveSection;
      });
    }
  }

  void _scrollToSection(String section) {
    GlobalKey targetKey;

    switch (section) {
      case 'store':
        targetKey = _storeInfoKey;
        break;
      case 'devices':
        targetKey = _devicesKey;
        break;
      default:
        return;
    }

    final RenderBox? box =
        targetKey.currentContext?.findRenderObject() as RenderBox?;
    if (box == null) return;

    final position = box.localToGlobal(Offset.zero).dy;

    _scrollController.animateTo(
      position - 100,
      duration: const Duration(milliseconds: 500),
      curve: Curves.easeInOut,
    );
  }

  void _handleIngredientRefill(DeviceIngredientState ingredient) {
  showDialog(
    context: context,
    builder: (context) => BlocProvider.value(
      value: context.read<KioskBloc>(), // ✅ Provide KioskBloc to dialog
      child: RefillIngredientDialog(
        ingredient: ingredient,
        onConfirm: (value) {
          // ✅ Now dialog context has access to KioskBloc
          context.read<KioskBloc>().add(UpdateIngredientEvent(
            request: UpdateIngredientRequest(
              deviceIngredientStateId: ingredient.deviceIngredientStateId,
              warningPercent: ingredient.warningPercent,
              currentCapacity: value,
              isWarning: false,
              isRenewable: ingredient.isRenewable,
              isPrimary: ingredient.isPrimary,
            ),
          ));
        },
      ),
    ),
  );
}

 void _showPasswordDialog() {
    showDialog(
      context: context,
      barrierDismissible: false,
      builder: (context) => AdminAuthDialog(
        adminPassword: _adminPassword,
        onSuccess: () {
          setState(() {
            _isAuthenticated = true;
          });
          _loadData();
        },
      ),
    );
  }

  @override
  void dispose() {
    _scrollController.removeListener(_updateActiveSection);
    _scrollController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Colors.white,
      appBar: _buildAppBar(),
      body: !_isAuthenticated
          ? const Center(child: Text('Đang xác thực quyền truy cập...', style: TextStyle(fontSize: 20)))
          : _buildAuthenticatedBody(),
    );
  }

  PreferredSizeWidget? _buildAppBar() {
    if (!_isAuthenticated) return null;
    
    return AppBar(
      elevation: 0,
      backgroundColor: Colors.white,
      title: const Text(
        'Cài đặt hệ thống',
        style: TextStyle(
          fontWeight: FontWeight.bold,
          fontSize: 24,
          color: Color(0xFF57B7E7),
        ),
      ),
      centerTitle: false,
      leading: IconButton(
        icon: const Icon(TDIcons.chevron_left, size: 28, color: Color(0xFF57B7E7)),
        onPressed: () => Navigator.of(context).pop(),
      ),
      actions: [
        IconButton(
          icon: const Icon(TDIcons.refresh, size: 24, color: Color(0xFF57B7E7)),
          onPressed: _loadData,
        ),
        const SizedBox(width: 8),
      ],
    );
  }

  Widget _buildAuthenticatedBody() {
    return MultiBlocListener(
      listeners: [
        BlocListener<OrganizationBloc, OrganizationState>(
          listener: (context, state) {
            if (state is OrganizationLoaded) {
              setState(() => _organization = state.organization);
            } else if (state is OrganizationError) {
              CustomToast.showError(context, 'Lỗi tải thông tin tổ chức: ${state.message}');
            }
          },
        ),
        BlocListener<KioskBloc, KioskState>(
          listener: (context, state) {
            if (state is KioskLoaded) {
              setState(() => _kiosk = state.kiosk);
            } else if (state is KioskError) {
              CustomToast.showError(context, 'Lỗi tải thông tin kiosk: ${state.message}');
            } else if (state is KioskUpdateIngredientLoaded) {
              if (state.isSuccess) {
                CustomToast.showSuccess(context, 'Cập nhật nguyên liệu thành công!');
                _loadKiosk();
              } else {
                CustomToast.showError(context, 'Cập nhật nguyên liệu thất bại!');
              }
            }
          },
        ),
      ],
      child: BlocBuilder<KioskBloc, KioskState>(
        builder: (context, kioskState) {
          return BlocBuilder<OrganizationBloc, OrganizationState>(
            builder: (context, orgState) {
              final bool isLoading = (kioskState is KioskLoading) || (orgState is OrganizationLoading);
              
              return RefreshIndicator(
                onRefresh: () async => _loadData(),
                color: const Color(0xFF57B7E7),
                child: Row(
                  children: [
                    _buildSidebar(),
                    _buildMainContent(isLoading),
                  ],
                ),
              );
            },
          );
        },
      ),
    );
  }

   Widget _buildSidebar() {
    return Container(
      width: 265,
      padding: const EdgeInsets.symmetric(vertical: 24, horizontal: 16),
      decoration: BoxDecoration(
        color: Colors.grey.shade50,
        border: Border(right: BorderSide(color: Colors.grey.shade200, width: 1)),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          _buildNavigation(),
          const SizedBox(height: 24),
          _buildKioskStatus(),
          const Spacer(),
        ],
      ),
    );
  }

   Widget _buildNavigation() {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(16),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.04),
            blurRadius: 8,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Text(
            'Điều hướng',
            style: TextStyle(
              fontSize: 18,
              fontWeight: FontWeight.bold,
              color: Color(0xFF57B7E7),
            ),
          ),
          const SizedBox(height: 16),
          _buildNavItem('Thông tin cửa hàng', TDIcons.shop, _activeSection == 'store', 'store'),
          _buildNavItem('Thiết bị', TDIcons.device, _activeSection == 'devices', 'devices'),
        ],
      ),
    );
  }

  Widget _buildKioskStatus() {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(16),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.04),
            blurRadius: 8,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          SectionHeader(title: 'Thông tin kiosk', icon: TDIcons.device),
          const SizedBox(height: 16),
          const Divider(height: 1),
          const SizedBox(height: 16),
          InfoRow(label: 'Tổng thiết bị', value: '${_kiosk?.kioskDevices.length ?? 0}'),
          InfoRow(
            label: 'Đang hoạt động',
            value: '${_kiosk?.kioskDevices.where((d) => d.status.toLowerCase() == "online").length ?? 0}/${_kiosk?.kioskDevices.length ?? 0}',
          ),
          InfoRow(
            label: 'Trạng thái kiosk',
            value: _kiosk?.status ?? 'Đang tải...',
            valueColor: _kiosk?.status.toLowerCase() == 'active' ? Colors.green : Colors.red,
          ),
        ],
      ),
    );
  }

  Widget _buildMainContent(bool isLoading) {
    return Expanded(
      child: isLoading && _kiosk == null && _organization == null
          ? const Center(
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  CircularProgressIndicator(color: Color(0xFF57B7E7)),
                  SizedBox(height: 16),
                  Text('Đang tải dữ liệu...', style: TextStyle(fontSize: 18, color: Color(0xFF57B7E7))),
                ],
              ),
            )
          : ListView(
              controller: _scrollController,
              padding: const EdgeInsets.all(0),
              children: [
                SizedBox(key: _storeInfoKey, child: _buildStoreInfoSection()),
                const SizedBox(height: 32),
                SizedBox(key: _devicesKey, child: _buildDevicesSection(isLoading)),
              ],
            ),
    );
  }

  Widget _buildStoreInfoSection() {
    return SettingCard(
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const SectionHeader(title: 'Thông tin tổ chức', icon: TDIcons.shop),
          const SizedBox(height: 24),
          const Divider(height: 1),
          const SizedBox(height: 24),
          if (_organization == null)
            const Center(child: CircularProgressIndicator(color: Color(0xFF57B7E7)))
          else
            _buildOrganizationInfo(),
        ],
      ),
    );
  }

  Widget _buildOrganizationInfo() {
    return Column(
      children: [
        InfoRow(label: 'Tên tổ chức:', value: _organization!.name),
        InfoRow(label: 'Mô tả:', value: _organization!.description),
        InfoRow(label: 'Email liên hệ:', value: _organization!.contactEmail),
        if (_organization!.store != null) ...[
          const SizedBox(height: 16),
          _buildStoreDivider(),
          const SizedBox(height: 16),
          _buildStoreInfo(),
        ] else
          _buildNoStoreInfo(),
      ],
    );
  }

  Widget _buildStoreDivider() {
    return Row(
      children: [
        const Expanded(child: Divider()),
        Padding(
          padding: const EdgeInsets.symmetric(horizontal: 16),
          child: Text(
            'Thông tin cửa hàng',
            style: TextStyle(
              fontSize: 14,
              fontWeight: FontWeight.w600,
              color: Colors.grey[600],
            ),
          ),
        ),
        const Expanded(child: Divider()),
      ],
    );
  }

  Widget _buildStoreInfo() {
    final store = _organization!.store!;
    return Column(
      children: [
        InfoRow(label: 'Tên cửa hàng:', value: store.name),
        if (store.contactPhone != null) 
          InfoRow(label: 'Số điện thoại:', value: store.contactPhone!),
        InfoRow(label: 'Địa chỉ:', value: store.locationAddress),
        InfoRow(
          label: 'Trạng thái:',
          value: store.status,
          valueColor: store.status.toLowerCase() == 'active' ? Colors.green : Colors.red,
        ),
      ],
    );
  }

  Widget _buildNoStoreInfo() {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(16),
      margin: const EdgeInsets.only(top: 16),
      decoration: BoxDecoration(
        color: Colors.grey.shade50,
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: Colors.grey.shade200),
      ),
      child: const Text(
        'Chưa có thông tin cửa hàng',
        style: TextStyle(fontSize: 16, color: Colors.grey, fontStyle: FontStyle.italic),
        textAlign: TextAlign.center,
      ),
    );
  }

  Widget _buildDevicesSection(bool isLoading) {
    return SettingCard(
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          SectionHeader(
            title: 'Thiết bị',
            icon: TDIcons.device,
            trailing: isLoading
                ? const SizedBox(
                    width: 20,
                    height: 20,
                    child: CircularProgressIndicator(strokeWidth: 2, color: Color(0xFF57B7E7)),
                  )
                : null,
          ),
          const SizedBox(height: 24),
          const Divider(height: 1),
          const SizedBox(height: 24),
          if (_kiosk?.kioskDevices.isEmpty ?? true)
            const Center(
              child: Padding(
                padding: EdgeInsets.symmetric(vertical: 32.0),
                child: Text('Không có thiết bị nào', style: TextStyle(fontSize: 16, color: Colors.grey)),
              ),
            )
          else
            _buildDevicesList(),
        ],
      ),
    );
  }

  Widget _buildDevicesList() {
    return Column(
      children: _kiosk!.kioskDevices.asMap().entries.map((entry) {
        final index = entry.key;
        final deviceMapping = entry.value;
        final isLast = index == _kiosk!.kioskDevices.length - 1;

        return Container(
          margin: EdgeInsets.only(bottom: isLast ? 0 : 24),
          child: _buildKioskDeviceCard(deviceMapping),
        );
      }).toList(),
    );
  }

  Widget _buildKioskDeviceCard(KioskDeviceMapping deviceMapping) {
    final device = deviceMapping.device;
    final bool isOnline = deviceMapping.status.toLowerCase() == "online";

    return Card(
      elevation: 0,
      margin: EdgeInsets.zero,
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(20),
        side: BorderSide(color: Colors.grey.shade200),
      ),
      color: Colors.white,
      child: Theme(
        data: Theme.of(context).copyWith(dividerColor: Colors.transparent),
        child: ExpansionTile(
          tilePadding: const EdgeInsets.fromLTRB(24, 20, 24, 20),
          childrenPadding: const EdgeInsets.fromLTRB(24, 0, 24, 24),
          backgroundColor: Colors.white,
          collapsedBackgroundColor: Colors.white,
          shape: Border.all(color: Colors.transparent),
          leading: Container(
            padding: const EdgeInsets.all(12),
            decoration: BoxDecoration(
              color: isOnline ? const Color(0xFFE7F7ED) : const Color(0xFFFEEFEF),
              borderRadius: BorderRadius.circular(12),
            ),
            child: Icon(
              device.deviceModel.deviceType.isMobileDevice ? TDIcons.mobile : TDIcons.device,
              color: isOnline ? Colors.green : Colors.red,
              size: 24,
            ),
          ),
          title: Text(
            device.name,
            style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 18, color: Colors.black87),
          ),
          subtitle: _buildDeviceSubtitle(device, isOnline),
          trailing: const Icon(TDIcons.chevron_down, color: Color(0xFF57B7E7), size: 22),
          children: [
            const Divider(height: 1),
            const SizedBox(height: 20),
            DeviceInfoCard(
              device: device,
              onIngredientRefill: _handleIngredientRefill,
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildDeviceSubtitle(KioskDevice device, bool isOnline) {
    return Padding(
      padding: const EdgeInsets.only(top: 8),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text('Serial: ${device.serialNumber}', style: TextStyle(color: Colors.grey[700], fontSize: 14)),
          const SizedBox(height: 4),
          Text('Model: ${device.deviceModel.modelName}', style: TextStyle(color: Colors.grey[700], fontSize: 14)),
          const SizedBox(height: 6),
          Row(
            children: [
              Container(
                width: 10,
                height: 10,
                decoration: BoxDecoration(
                  color: isOnline ? Colors.green : Colors.red,
                  shape: BoxShape.circle,
                ),
              ),
              const SizedBox(width: 8),
              Text(
                isOnline ? 'Đang hoạt động' : 'Ngắt kết nối',
                style: TextStyle(
                  color: isOnline ? Colors.green : Colors.red,
                  fontSize: 14,
                  fontWeight: FontWeight.w500,
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }

  Widget _buildNavItem(String title, IconData icon, bool isActive, String section) {
    return Container(
      margin: const EdgeInsets.only(bottom: 8),
      decoration: BoxDecoration(
        color: isActive ? const Color(0xFFE6F5FC) : Colors.transparent,
        borderRadius: BorderRadius.circular(12),
      ),
      child: ListTile(
        leading: Icon(icon, color: isActive ? const Color(0xFF57B7E7) : Colors.grey, size: 20),
        title: Text(
          title,
          style: TextStyle(
            fontSize: 16,
            fontWeight: isActive ? FontWeight.bold : FontWeight.normal,
            color: isActive ? const Color(0xFF57B7E7) : Colors.black87,
          ),
        ),
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
        dense: true,
        contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 4),
        onTap: () => _scrollToSection(section),
      ),
    );
  }
}
