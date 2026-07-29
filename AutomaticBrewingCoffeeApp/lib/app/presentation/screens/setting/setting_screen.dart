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
import 'package:abc_androidapp/app/presentation/widgets/setting_screen/refill_ingredient_dialog.dart';
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

  void _confirmIngredientRefill(DeviceIngredientState ingredient) {
    String ingredientType = ingredient.ingredientType;
    final BuildContext widgetContext = context;
    final TextEditingController capacityController = TextEditingController();
    final FocusNode capacityFocusNode = FocusNode();

    // Set default value to max capacity
    capacityController.text = ingredient.maxCapacity.toString();

    showDialog(
      context: context,
      builder: (dialogContext) => StatefulBuilder(
        builder: (context, setDialogState) {
          bool isValidCapacity = true;
          String? errorMessage;

          void validateCapacity() {
            final value = int.tryParse(capacityController.text);

            print(
                'Validating: $value, min: ${ingredient.minCapacity}, max: ${ingredient.maxCapacity}');

            if (value == null) {
              isValidCapacity = false;
              errorMessage = 'Vui lòng nhập số hợp lệ';
              print('❌ Invalid: null value');
            } else if (value < ingredient.minCapacity) {
              isValidCapacity = false;
              errorMessage =
                  'Giá trị tối thiểu: ${ingredient.minCapacity} ${ingredient.unit}';
              print(
                  '❌ Invalid: below min ($value < ${ingredient.minCapacity})');
            } else if (value > ingredient.maxCapacity) {
              isValidCapacity = false;
              errorMessage =
                  'Giá trị tối đa: ${ingredient.maxCapacity} ${ingredient.unit}';
              print(
                  '❌ Invalid: above max ($value > ${ingredient.maxCapacity})');
            } else {
              isValidCapacity = true;
              errorMessage = null;
              print('✅ Valid: $value');
            }
          }

          // ✅ Run initial validation
          validateCapacity();

          return AlertDialog(
            backgroundColor: Colors.white,
            shape:
                RoundedRectangleBorder(borderRadius: BorderRadius.circular(20)),
            title: Row(
              children: [
                Container(
                  padding: const EdgeInsets.all(8),
                  decoration: BoxDecoration(
                    color: const Color(0xFF57B7E7).withOpacity(0.1),
                    borderRadius: BorderRadius.circular(8),
                  ),
                  child: const Icon(
                    TDIcons.refresh,
                    color: Color(0xFF57B7E7),
                    size: 20,
                  ),
                ),
                const SizedBox(width: 12),
                const Expanded(
                  child: Text(
                    'Xác nhận refill nguyên liệu',
                    style: TextStyle(
                      fontSize: 20,
                      fontWeight: FontWeight.bold,
                      color: Color(0xFF57B7E7),
                    ),
                  ),
                ),
              ],
            ),
            content: Column(
              mainAxisSize: MainAxisSize.min,
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                // Ingredient info
                Container(
                  padding: const EdgeInsets.all(16),
                  decoration: BoxDecoration(
                    color: Colors.grey.shade50,
                    borderRadius: BorderRadius.circular(12),
                    border: Border.all(color: Colors.grey.shade200),
                  ),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Row(
                        children: [
                          Icon(
                            TDIcons.layers,
                            size: 16,
                            color: Colors.grey.shade600,
                          ),
                          const SizedBox(width: 8),
                          Text(
                            ingredientType,
                            style: const TextStyle(
                              fontSize: 16,
                              fontWeight: FontWeight.bold,
                              color: Colors.black87,
                            ),
                          ),
                        ],
                      ),
                      const SizedBox(height: 8),
                      Text(
                        'Dung lượng hiện tại: ${ingredient.currentCapacity} ${ingredient.unit}',
                        style: TextStyle(
                          fontSize: 14,
                          color: Colors.grey.shade600,
                        ),
                      ),
                      Text(
                        'Khoảng cho phép: ${ingredient.minCapacity} - ${ingredient.maxCapacity} ${ingredient.unit}',
                        style: TextStyle(
                          fontSize: 14,
                          color: Colors.grey.shade600,
                        ),
                      ),
                    ],
                  ),
                ),
                const SizedBox(height: 20),

                // Input section
                const Text(
                  'Nhập dung lượng sau khi refill:',
                  style: TextStyle(
                    fontSize: 16,
                    fontWeight: FontWeight.w600,
                    color: Colors.black87,
                  ),
                ),
                const SizedBox(height: 12),

                // Capacity input field
                TextField(
                  controller: capacityController,
                  focusNode: capacityFocusNode,
                  keyboardType: TextInputType.number,
                  inputFormatters: [
                    FilteringTextInputFormatter.digitsOnly,
                  ],
                  onChanged: (value) {
                    // ✅ Use setDialogState to trigger rebuild
                    setDialogState(() {
                      validateCapacity();
                    });
                    print(
                        'Text changed to: $value, isValid: $isValidCapacity'); // Debug
                  },
                  decoration: InputDecoration(
                    hintText: 'Nhập dung lượng...',
                    suffixText: ingredient.unit,
                    suffixStyle: TextStyle(
                      color: Colors.grey.shade600,
                      fontWeight: FontWeight.w500,
                    ),
                    prefixIcon: Icon(
                      TDIcons.edit,
                      color: isValidCapacity
                          ? const Color(0xFF57B7E7)
                          : Colors.red,
                      size: 20,
                    ),
                    border: OutlineInputBorder(
                      borderRadius: BorderRadius.circular(12),
                      borderSide: BorderSide(color: Colors.grey.shade300),
                    ),
                    enabledBorder: OutlineInputBorder(
                      borderRadius: BorderRadius.circular(12),
                      borderSide: BorderSide(
                        color: isValidCapacity
                            ? Colors.grey.shade300
                            : Colors.red.shade300,
                      ),
                    ),
                    focusedBorder: OutlineInputBorder(
                      borderRadius: BorderRadius.circular(12),
                      borderSide: BorderSide(
                        color: isValidCapacity
                            ? const Color(0xFF57B7E7)
                            : Colors.red,
                        width: 2,
                      ),
                    ),
                    errorBorder: OutlineInputBorder(
                      borderRadius: BorderRadius.circular(12),
                      borderSide: const BorderSide(color: Colors.red, width: 2),
                    ),
                    focusedErrorBorder: OutlineInputBorder(
                      borderRadius: BorderRadius.circular(12),
                      borderSide: const BorderSide(color: Colors.red, width: 2),
                    ),
                    filled: true,
                    fillColor:
                        isValidCapacity ? Colors.white : Colors.red.shade50,
                    contentPadding: const EdgeInsets.symmetric(
                        horizontal: 16, vertical: 16),
                    errorText: errorMessage,
                  ),
                  style: const TextStyle(
                    fontSize: 16,
                    fontWeight: FontWeight.w600,
                  ),
                ),

                // Quick select buttons
                const SizedBox(height: 16),
                const Text(
                  'Chọn nhanh:',
                  style: TextStyle(
                    fontSize: 14,
                    fontWeight: FontWeight.w500,
                    color: Colors.grey,
                  ),
                ),
                const SizedBox(height: 8),
                Row(
                  children: [
                    Expanded(
                      child: OutlinedButton(
                        onPressed: () {
                          capacityController.text =
                              ingredient.maxCapacity.toString();
                          // ✅ Use setDialogState for quick buttons too
                          setDialogState(() {
                            validateCapacity();
                          });
                        },
                        style: OutlinedButton.styleFrom(
                          side: const BorderSide(color: Color(0xFF57B7E7)),
                          shape: RoundedRectangleBorder(
                            borderRadius: BorderRadius.circular(8),
                          ),
                          padding: const EdgeInsets.symmetric(vertical: 8),
                        ),
                        child: Text(
                          'Đầy (${ingredient.maxCapacity})',
                          style: const TextStyle(
                            color: Color(0xFF57B7E7),
                            fontSize: 12,
                            fontWeight: FontWeight.w600,
                          ),
                        ),
                      ),
                    ),
                    const SizedBox(width: 8),
                    Expanded(
                      child: OutlinedButton(
                        onPressed: () {
                          final halfCapacity = ((ingredient.maxCapacity +
                                      ingredient.minCapacity) /
                                  2)
                              .floor();
                          capacityController.text = halfCapacity.toString();
                          // ✅ Use setDialogState for quick buttons too
                          setDialogState(() {
                            validateCapacity();
                          });
                        },
                        style: OutlinedButton.styleFrom(
                          side: BorderSide(color: Colors.grey.shade400),
                          shape: RoundedRectangleBorder(
                            borderRadius: BorderRadius.circular(8),
                          ),
                          padding: const EdgeInsets.symmetric(vertical: 8),
                        ),
                        child: Text(
                          'Nửa (${((ingredient.maxCapacity + ingredient.minCapacity) / 2).floor()})',
                          style: TextStyle(
                            color: Colors.grey.shade600,
                            fontSize: 12,
                            fontWeight: FontWeight.w600,
                          ),
                        ),
                      ),
                    ),
                  ],
                ),
              ],
            ),
            actions: [
              TextButton(
                onPressed: () => Navigator.of(dialogContext).pop(),
                style: TextButton.styleFrom(
                  padding:
                      const EdgeInsets.symmetric(horizontal: 20, vertical: 12),
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(12),
                  ),
                ),
                child: const Text(
                  'Hủy',
                  style: TextStyle(
                    fontSize: 16,
                    color: Colors.grey,
                    fontWeight: FontWeight.w500,
                  ),
                ),
              ),
              ElevatedButton(
                onPressed: !isValidCapacity
                    ? null
                    : () {
                        final inputValue = int.parse(capacityController.text);
                        Navigator.of(dialogContext).pop();
                        widgetContext
                            .read<KioskBloc>()
                            .add(UpdateIngredientEvent(
                              request: UpdateIngredientRequest(
                                deviceIngredientStateId:
                                    ingredient.deviceIngredientStateId,
                                warningPercent: ingredient.warningPercent,
                                currentCapacity: inputValue,
                                isWarning: false,
                                isRenewable: ingredient.isRenewable,
                                isPrimary: ingredient.isPrimary,
                              ),
                            ));
                      },
                style: ElevatedButton.styleFrom(
                  backgroundColor: isValidCapacity
                      ? const Color(0xFF57B7E7)
                      : Colors.grey.shade300,
                  foregroundColor: Colors.white,
                  padding:
                      const EdgeInsets.symmetric(horizontal: 24, vertical: 14),
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(12),
                  ),
                  elevation: 0,
                ),
                child: const Text(
                  'Xác nhận',
                  style: TextStyle(
                    fontSize: 16,
                    fontWeight: FontWeight.w600,
                  ),
                ),
              ),
            ],
            actionsPadding: const EdgeInsets.fromLTRB(16, 0, 16, 16),
          );
        },
      ),
    );

    // Auto focus on input field
    WidgetsBinding.instance.addPostFrameCallback((_) {
      capacityFocusNode.requestFocus();
      capacityController.selection = TextSelection(
        baseOffset: 0,
        extentOffset: capacityController.text.length,
      );
    });
  }

  void _showPasswordDialog() {
    final TextEditingController pinController = TextEditingController();
    final FocusNode pinFocusNode = FocusNode();

    WidgetsBinding.instance.addPostFrameCallback((_) {
      pinFocusNode.requestFocus();
    });

    showDialog(
      context: context,
      barrierDismissible: false,
      builder: (context) => AlertDialog(
        backgroundColor: Colors.white,
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(20)),
        title: const Text(
          'Xác thực quản trị viên',
          style: TextStyle(
            fontSize: 22,
            fontWeight: FontWeight.bold,
            color: Color(0xFF57B7E7),
          ),
        ),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            const Text(
              'Vui lòng nhập mã PIN 6 số để truy cập cài đặt',
              style: TextStyle(fontSize: 16, height: 1.5),
            ),
            const SizedBox(height: 24),
            TextField(
              controller: pinController,
              focusNode: pinFocusNode,
              decoration: InputDecoration(
                hintText: 'Nhập mã PIN',
                prefixIcon:
                    const Icon(TDIcons.lock_on, color: Color(0xFF57B7E7)),
                border: OutlineInputBorder(
                  borderRadius: BorderRadius.circular(16),
                  borderSide: BorderSide(color: Colors.grey.shade300, width: 1),
                ),
                enabledBorder: OutlineInputBorder(
                  borderRadius: BorderRadius.circular(16),
                  borderSide: BorderSide(color: Colors.grey.shade300, width: 1),
                ),
                focusedBorder: OutlineInputBorder(
                  borderRadius: BorderRadius.circular(16),
                  borderSide:
                      const BorderSide(color: Color(0xFF57B7E7), width: 2),
                ),
                filled: true,
                fillColor: Colors.white,
                contentPadding: const EdgeInsets.symmetric(vertical: 20),
              ),
              keyboardType: TextInputType.number,
              inputFormatters: [
                FilteringTextInputFormatter.digitsOnly,
                LengthLimitingTextInputFormatter(6),
              ],
              obscureText: true,
              textAlign: TextAlign.center,
              style: const TextStyle(
                fontSize: 24,
                letterSpacing: 3,
                fontWeight: FontWeight.bold,
              ),
            ),
          ],
        ),
        actions: [
          TextButton(
            onPressed: () {
              Navigator.of(context).pop();
              Navigator.of(context).pop();
            },
            style: TextButton.styleFrom(
              padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 12),
              shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(14)),
            ),
            child: const Text(
              'Hủy',
              style: TextStyle(
                fontSize: 16,
                color: Colors.grey,
              ),
            ),
          ),
          ElevatedButton(
            onPressed: () {
              if (pinController.text == _adminPassword) {
                setState(() {
                  _isAuthenticated = true;
                });
                Navigator.of(context).pop();

                _loadOrganization();
                _loadKiosk();
              } else {
                CustomToast.showError(
                  context,
                  'Mã PIN không chính xác',
                );
                pinController.clear();
                pinFocusNode.requestFocus();
              }
            },
            style: ElevatedButton.styleFrom(
              backgroundColor: const Color(0xFF57B7E7),
              foregroundColor: Colors.white,
              padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 14),
              shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(14)),
            ),
            child: const Text(
              'Xác nhận',
              style: TextStyle(fontSize: 16),
            ),
          ),
        ],
        actionsPadding: const EdgeInsets.fromLTRB(16, 0, 16, 16),
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
      appBar: _isAuthenticated
          ? AppBar(
              elevation: 0,
              backgroundColor: Colors.white,
              title: const Text(
                'Cài đặt hệ thống',
                style: TextStyle(
                    fontWeight: FontWeight.bold,
                    fontSize: 24,
                    color: Color(0xFF57B7E7)),
              ),
              centerTitle: false,
              leading: IconButton(
                icon: const Icon(TDIcons.chevron_left,
                    size: 28, color: Color(0xFF57B7E7)),
                onPressed: () => Navigator.of(context).pop(),
              ),
              actions: [
                IconButton(
                  icon: const Icon(TDIcons.refresh,
                      size: 24, color: Color(0xFF57B7E7)),
                  onPressed: () {
                    _loadOrganization();
                    _loadKiosk();
                  },
                ),
                const SizedBox(width: 8),
              ],
            )
          : null,
      body: !_isAuthenticated
          ? const Center(
              child: Text(
                'Đang xác thực quyền truy cập...',
                style: TextStyle(fontSize: 20),
              ),
            )
          : MultiBlocListener(
              listeners: [
                // ✅ Listen to OrganizationBloc
                BlocListener<OrganizationBloc, OrganizationState>(
                  listener: (context, state) {
                    if (state is OrganizationLoaded) {
                      setState(() {
                        _organization = state.organization;
                      });
                    } else if (state is OrganizationError) {
                      CustomToast.showError(
                        context,
                        'Lỗi tải thông tin tổ chức: ${state.message}',
                      );
                    }
                  },
                ),
                // ✅ Listen to KioskBloc
                BlocListener<KioskBloc, KioskState>(
                  listener: (context, state) {
                    if (state is KioskLoaded) {
                      setState(() {
                        _kiosk = state.kiosk;
                      });
                    } else if (state is KioskError) {
                      CustomToast.showError(
                        context,
                        'Lỗi tải thông tin kiosk: ${state.message}',
                      );
                    } else if (state is KioskUpdateIngredientLoaded) {
                      if (state.isSuccess) {
                        CustomToast.showSuccess(
                          context,
                          'Cập nhật nguyên liệu thành công!',
                        );
                        _loadKiosk();
                      } else {
                        CustomToast.showError(
                          context,
                          'Cập nhật nguyên liệu thất bại!',
                        );
                      }
                    }
                  },
                ),
              ],
              child: BlocBuilder<KioskBloc, KioskState>(
                builder: (context, kioskState) {
                  final bool isKioskLoading = kioskState is KioskLoading;

                  return BlocBuilder<OrganizationBloc, OrganizationState>(
                    builder: (context, orgState) {
                      final bool isOrgLoading = orgState is OrganizationLoading;
                      final bool isLoading = isKioskLoading || isOrgLoading;

                      return RefreshIndicator(
                        onRefresh: () async {
                          _loadOrganization();
                          _loadKiosk();
                        },
                        color: const Color(0xFF57B7E7),
                        child: Row(
                          children: [
                            // ✅ Left Sidebar
                            Container(
                              width: 250,
                              padding: const EdgeInsets.symmetric(
                                  vertical: 24, horizontal: 16),
                              decoration: BoxDecoration(
                                color: Colors.grey.shade50,
                                border: Border(
                                  right: BorderSide(
                                      color: Colors.grey.shade200, width: 1),
                                ),
                              ),
                              child: Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  // Navigation
                                  Container(
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
                                      crossAxisAlignment:
                                          CrossAxisAlignment.start,
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
                                        _buildNavItem(
                                            'Thông tin cửa hàng',
                                            TDIcons.shop,
                                            _activeSection == 'store',
                                            'store'),
                                        _buildNavItem(
                                            'Thiết bị',
                                            TDIcons.device,
                                            _activeSection == 'devices',
                                            'devices'),
                                      ],
                                    ),
                                  ),
                                  const SizedBox(height: 24),

                                  // ✅ Device Status using kiosk data
                                  Container(
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
                                      crossAxisAlignment:
                                          CrossAxisAlignment.start,
                                      children: [
                                        Row(
                                          children: [
                                            Container(
                                              padding: const EdgeInsets.all(8),
                                              decoration: BoxDecoration(
                                                color: const Color(0xFFE6F5FC),
                                                borderRadius:
                                                    BorderRadius.circular(8),
                                              ),
                                              child: const Icon(
                                                TDIcons.device,
                                                color: Color(0xFF57B7E7),
                                                size: 20,
                                              ),
                                            ),
                                            const SizedBox(width: 12),
                                            const Text(
                                              'Trạng thái kiosk',
                                              style: TextStyle(
                                                fontWeight: FontWeight.bold,
                                                fontSize: 16,
                                                color: Color(0xFF57B7E7),
                                              ),
                                            ),
                                          ],
                                        ),
                                        const SizedBox(height: 16),
                                        const Divider(height: 1),
                                        const SizedBox(height: 16),
                                        _buildDeviceStatusRow(
                                          'Tổng thiết bị',
                                          '${_kiosk?.kioskDevices.length ?? 0}',
                                        ),
                                        _buildDeviceStatusRow(
                                          'Đang hoạt động',
                                          '${_kiosk?.kioskDevices.where((d) => d.status.toLowerCase() == "online").length ?? 0}/${_kiosk?.kioskDevices.length ?? 0}',
                                        ),
                                        _buildDeviceStatusRow(
                                          'Trạng thái kiosk',
                                          _kiosk?.status.toLowerCase() == 'active' ? 'Hoạt động' : 'Offline',
                                          color: _kiosk?.status.toLowerCase() ==
                                                  'active'
                                              ? Colors.green
                                              : Colors.red,
                                        ),
                                      ],
                                    ),
                                  ),
                                  const Spacer(),
                                ],
                              ),
                            ),

                            // ✅ Main content area
                            Expanded(
                              child: isLoading &&
                                      _kiosk == null &&
                                      _organization == null
                                  ? const Center(
                                      child: Column(
                                        mainAxisAlignment:
                                            MainAxisAlignment.center,
                                        children: [
                                          CircularProgressIndicator(
                                              color: Color(0xFF57B7E7)),
                                          SizedBox(height: 16),
                                          Text(
                                            'Đang tải dữ liệu...',
                                            style: TextStyle(
                                                fontSize: 18,
                                                color: Color(0xFF57B7E7)),
                                          ),
                                        ],
                                      ),
                                    )
                                  : ListView(
                                      controller: _scrollController,
                                      padding: const EdgeInsets.all(0),
                                      children: [
                                        SizedBox(
                                            key: _storeInfoKey,
                                            child: _buildStoreInfoSection()),
                                        const SizedBox(height: 32),
                                        SizedBox(
                                            key: _devicesKey,
                                            child: _buildDevicesSection(
                                                isKioskLoading)),
                                      ],
                                    ),
                            ),
                          ],
                        ),
                      );
                    },
                  );
                },
              ),
            ),
    );
  }

  Widget _buildNavItem(
      String title, IconData icon, bool isActive, String section) {
    return Container(
      margin: const EdgeInsets.only(bottom: 8),
      decoration: BoxDecoration(
        color: isActive ? const Color(0xFFE6F5FC) : Colors.transparent,
        borderRadius: BorderRadius.circular(12),
      ),
      child: ListTile(
        leading: Icon(
          icon,
          color: isActive ? const Color(0xFF57B7E7) : Colors.grey,
          size: 20,
        ),
        title: Text(
          title,
          style: TextStyle(
            fontSize: 16,
            fontWeight: isActive ? FontWeight.bold : FontWeight.normal,
            color: isActive ? const Color(0xFF57B7E7) : Colors.black87,
          ),
        ),
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(12),
        ),
        dense: true,
        contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 4),
        onTap: () => _scrollToSection(section),
      ),
    );
  }

  Widget _buildDeviceStatusRow(String label, String value,
      {IconData? icon, Color? color}) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 12),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          Text(
            label,
            style: TextStyle(
              fontSize: 14,
              color: Colors.grey.shade700,
            ),
          ),
          Row(
            children: [
              if (icon != null) ...[
                Icon(icon, size: 16, color: color ?? Colors.black87),
                const SizedBox(width: 6),
              ],
              Text(
                value,
                style: TextStyle(
                  fontSize: 14,
                  fontWeight: FontWeight.w500,
                  color: color ?? Colors.black87,
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }

  Widget _buildStoreInfoSection() {
    return Card(
      elevation: 0,
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(20),
        side: BorderSide(color: Colors.grey.shade200),
      ),
      color: Colors.white,
      child: Padding(
        padding: const EdgeInsets.all(24),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                Container(
                  padding: const EdgeInsets.all(12),
                  decoration: BoxDecoration(
                    color: const Color(0xFFE6F5FC),
                    borderRadius: BorderRadius.circular(12),
                  ),
                  child: const Icon(
                    TDIcons.shop,
                    color: Color(0xFF57B7E7),
                    size: 24,
                  ),
                ),
                const SizedBox(width: 16),
                const Text(
                  'Thông tin tổ chức',
                  style: TextStyle(
                    fontSize: 20,
                    fontWeight: FontWeight.bold,
                    color: Color(0xFF57B7E7),
                  ),
                ),
              ],
            ),
            const SizedBox(height: 24),
            const Divider(height: 1),
            const SizedBox(height: 24),
            if (_organization == null)
              const Center(
                 child: Text(
                    'Không có thông tin tổ chức',
                    style: TextStyle(fontSize: 16, color: Colors.grey),
                  ),
              )
            else
              Column(
                children: [
                  _buildInfoRow('Tên tổ chức:', _organization!.name),
                  _buildInfoRow('Mô tả:', _organization!.description),
                  _buildInfoRow('Email liên hệ:', _organization!.contactEmail),

                  // ✅ Store information
                  if (_organization!.store != null) ...[
                    const SizedBox(height: 16),
                    // Store section divider
                    Row(
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
                    ),
                    const SizedBox(height: 16),

                    _buildInfoRow('Tên cửa hàng:', _organization!.store!.name),
                    if (_organization!.store!.contactPhone != null)
                      _buildInfoRow('Số điện thoại:',
                          _organization!.store!.contactPhone!),
                    _buildInfoRow(
                        'Địa chỉ:', _organization!.store!.locationAddress),
                    _buildInfoRow(
                      'Trạng thái:',
                      _organization!.store!.status.toLowerCase() == 'active'
                          ? 'Hoạt động'
                          : 'Offline',
                      valueColor:
                          _organization!.store!.status.toLowerCase() == 'active'
                              ? Colors.green
                              : Colors.red,
                    ),
                  ] else ...[
                    const SizedBox(height: 16),
                    Container(
                      width: double.infinity,
                      padding: const EdgeInsets.all(16),
                      decoration: BoxDecoration(
                        color: Colors.grey.shade50,
                        borderRadius: BorderRadius.circular(12),
                        border: Border.all(color: Colors.grey.shade200),
                      ),
                      child: const Text(
                        'Chưa có thông tin cửa hàng',
                        style: TextStyle(
                          fontSize: 16,
                          color: Colors.grey,
                          fontStyle: FontStyle.italic,
                        ),
                        textAlign: TextAlign.center,
                      ),
                    ),
                  ],
                ],
              ),
          ],
        ),
      ),
    );
  }

  Widget _buildDevicesSection(bool isLoading) {
    return Card(
      elevation: 0,
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(20),
        side: BorderSide(color: Colors.grey.shade200),
      ),
      color: Colors.white,
      child: Padding(
        padding: const EdgeInsets.all(24),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                Container(
                  padding: const EdgeInsets.all(12),
                  decoration: BoxDecoration(
                    color: const Color(0xFFE6F5FC),
                    borderRadius: BorderRadius.circular(12),
                  ),
                  child: const Icon(
                    TDIcons.device,
                    color: Color(0xFF57B7E7),
                    size: 24,
                  ),
                ),
                const SizedBox(width: 16),
                const Text(
                  'Thiết bị',
                  style: TextStyle(
                    fontSize: 20,
                    fontWeight: FontWeight.bold,
                    color: Color(0xFF57B7E7),
                  ),
                ),
                const Spacer(),
                if (isLoading)
                  const Padding(
                    padding: EdgeInsets.only(left: 16),
                    child: SizedBox(
                      width: 20,
                      height: 20,
                      child: CircularProgressIndicator(
                        strokeWidth: 2,
                        color: Color(0xFF57B7E7),
                      ),
                    ),
                  )
              ],
            ),
            const SizedBox(height: 24),
            const Divider(height: 1),
            const SizedBox(height: 24),
            if (_kiosk?.kioskDevices.isEmpty ?? true)
              const Center(
                child: Padding(
                  padding: EdgeInsets.symmetric(vertical: 32.0),
                  child: Text(
                    'Không có thiết bị nào',
                    style: TextStyle(fontSize: 16, color: Colors.grey),
                  ),
                ),
              )
            else
              ...List.generate(_kiosk!.kioskDevices.length, (index) {
                final deviceMapping = _kiosk!.kioskDevices[index];
                final isLast = index == _kiosk!.kioskDevices.length - 1;

                return Container(
                  margin: EdgeInsets.only(bottom: isLast ? 0 : 24),
                  child: _buildKioskDeviceCard(deviceMapping),
                );
              }),
          ],
        ),
      ),
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
        data: Theme.of(context).copyWith(
          dividerColor: Colors.transparent,
        ),
        child: ExpansionTile(
          tilePadding: const EdgeInsets.fromLTRB(24, 20, 24, 20),
          childrenPadding: const EdgeInsets.fromLTRB(24, 0, 24, 24),
          backgroundColor: Colors.white,
          collapsedBackgroundColor: Colors.white,
          shape: Border.all(color: Colors.transparent),
          leading: Container(
            padding: const EdgeInsets.all(12),
            decoration: BoxDecoration(
              color:
                  isOnline ? const Color(0xFFE7F7ED) : const Color(0xFFFEEFEF),
              borderRadius: BorderRadius.circular(12),
            ),
            child: Icon(
              device.deviceModel.deviceType.isMobileDevice
                  ? TDIcons.mobile
                  : TDIcons.device,
              color: isOnline ? Colors.green : Colors.red,
              size: 24,
            ),
          ),
          title: Text(
            device.name,
            style: const TextStyle(
              fontWeight: FontWeight.bold,
              fontSize: 18,
              color: Colors.black87,
            ),
          ),
          subtitle: Padding(
            padding: const EdgeInsets.only(top: 8),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  'Serial: ${device.serialNumber}',
                  style: TextStyle(
                    color: Colors.grey[700],
                    fontSize: 14,
                  ),
                ),
                const SizedBox(height: 4),
                Text(
                  'Model: ${device.deviceModel.modelName}',
                  style: TextStyle(
                    color: Colors.grey[700],
                    fontSize: 14,
                  ),
                ),
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
          ),
          trailing: const Icon(
            TDIcons.chevron_down,
            color: Color(0xFF57B7E7),
            size: 22,
          ),
          children: [
            const Divider(height: 1),
            const SizedBox(height: 20),
            _buildCombinedDeviceSection(device),
          ],
        ),
      ),
    );
  }

  Widget _buildCombinedDeviceSection(KioskDevice device) {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.grey.shade50,
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: Colors.grey.shade200),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // ✅ Device Info Header with Icon
          Row(
            children: [
              Container(
                padding: const EdgeInsets.all(6),
                decoration: BoxDecoration(
                  color: const Color(0xFF57B7E7).withOpacity(0.1),
                  borderRadius: BorderRadius.circular(8),
                ),
                child: const Icon(
                  TDIcons.info_circle,
                  color: Color(0xFF57B7E7),
                  size: 16,
                ),
              ),
              const SizedBox(width: 8),
              const Text(
                'Thông tin thiết bị',
                style: TextStyle(
                  fontSize: 16,
                  fontWeight: FontWeight.bold,
                  color: Colors.black87,
                ),
              ),
            ],
          ),
          const SizedBox(height: 12),
          _buildInfoRow('Loại thiết bị:', device.deviceModel.deviceType.name),
          _buildInfoRow('Nhà sản xuất:', device.deviceModel.manufacturer),
          _buildInfoRow('Mô tả:', device.description),
          _buildInfoRow('Trạng thái:', device.status.toLowerCase() == "working" ? 'Làm việc' : 'Không làm việc',
              valueColor: device.status.toLowerCase() == 'working'
                  ? Colors.green
                  : Colors.red),

          // ✅ Ingredient Section (if available)
          if (device.deviceIngredientStates.isNotEmpty) ...[
            const SizedBox(height: 24),
            // Styled section divider
            Row(
              children: [
                Container(
                  padding: const EdgeInsets.all(6),
                  decoration: BoxDecoration(
                    color: Colors.orange.withOpacity(0.1),
                    borderRadius: BorderRadius.circular(8),
                  ),
                  child: const Icon(
                    TDIcons.layers,
                    color: Colors.orange,
                    size: 16,
                  ),
                ),
                const SizedBox(width: 8),
                const Text(
                  'Nguyên liệu',
                  style: TextStyle(
                    fontSize: 16,
                    fontWeight: FontWeight.bold,
                    color: Colors.black87,
                  ),
                ),
                const Spacer(),
                // Ingredient count badge
                Container(
                  padding:
                      const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                  decoration: BoxDecoration(
                    color: Colors.orange.withOpacity(0.1),
                    borderRadius: BorderRadius.circular(12),
                    border: Border.all(color: Colors.orange.withOpacity(0.3)),
                  ),
                  child: Text(
                    '${device.deviceIngredientStates.length} loại',
                    style: const TextStyle(
                      fontSize: 12,
                      fontWeight: FontWeight.w600,
                      color: Colors.orange,
                    ),
                  ),
                ),
              ],
            ),
            const SizedBox(height: 16),
            ...device.deviceIngredientStates
                .map((ingredient) => _buildIngredientCard(ingredient)),
          ],
        ],
      ),
    );
  }

  Widget _buildIngredientCard(DeviceIngredientState ingredient) {
    final percentage = ingredient.capacityPercentage;
    final isLow = ingredient.isLowCapacity;

    Color getCapacityColor() {
      if (percentage > 60) return Colors.green;
      if (percentage > 30) return Colors.orange;
      return Colors.red;
    }

    return Container(
      margin: const EdgeInsets.only(bottom: 16),
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(12),
        border: Border.all(
          color: isLow ? Colors.red.shade200 : Colors.grey.shade200,
          width: isLow ? 2 : 1,
        ),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      ingredient.ingredientType,
                      style: const TextStyle(
                        fontSize: 16,
                        fontWeight: FontWeight.bold,
                        color: Colors.black87,
                      ),
                    ),
                    const SizedBox(height: 4),
                    Text(
                      '${ingredient.currentCapacity}/${ingredient.maxCapacity} ${ingredient.unit}',
                      style: TextStyle(
                        fontSize: 14,
                        color: Colors.grey[600],
                      ),
                    ),
                  ],
                ),
              ),
              Column(
                crossAxisAlignment: CrossAxisAlignment.end,
                children: [
                  Text(
                    '${percentage.toStringAsFixed(1)}%',
                    style: TextStyle(
                      fontSize: 18,
                      fontWeight: FontWeight.bold,
                      color: getCapacityColor(),
                    ),
                  ),
                  if (isLow)
                    Container(
                      padding: const EdgeInsets.symmetric(
                          horizontal: 8, vertical: 4),
                      decoration: BoxDecoration(
                        color: Colors.red.shade50,
                        borderRadius: BorderRadius.circular(8),
                        border: Border.all(color: Colors.red.shade200),
                      ),
                      child: const Text(
                        'Cần refill',
                        style: TextStyle(
                          fontSize: 12,
                          color: Colors.red,
                          fontWeight: FontWeight.w500,
                        ),
                      ),
                    ),
                ],
              ),
            ],
          ),
          const SizedBox(height: 12),

          // Progress bar
          Container(
            height: 8,
            decoration: BoxDecoration(
              color: Colors.grey.shade200,
              borderRadius: BorderRadius.circular(4),
            ),
            child: FractionallySizedBox(
              alignment: Alignment.centerLeft,
              widthFactor: percentage / 100,
              child: Container(
                decoration: BoxDecoration(
                  color: getCapacityColor(),
                  borderRadius: BorderRadius.circular(4),
                ),
              ),
            ),
          ),

          const SizedBox(height: 12),

          // Refill button
          SizedBox(
            width: double.infinity,
            child: ElevatedButton.icon(
              onPressed: () => _confirmIngredientRefill(ingredient),
              icon: const Icon(TDIcons.refresh, size: 18),
              label: const Text('Xác nhận đã refill'),
              style: ElevatedButton.styleFrom(
                backgroundColor: const Color(0xFF57B7E7),
                foregroundColor: Colors.white,
                padding: const EdgeInsets.symmetric(vertical: 12),
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(8),
                ),
                elevation: 0,
              ),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildInfoRow(String label, String value, {Color? valueColor}) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 16),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          SizedBox(
            width: 150,
            child: Text(
              label,
              style: TextStyle(
                color: Colors.grey[700],
                fontSize: 16,
                fontWeight: FontWeight.w500,
              ),
            ),
          ),
          Expanded(
            child: Text(
              value,
              style: TextStyle(
                fontSize: 16,
                fontWeight: FontWeight.w500,
                color: valueColor ?? Colors.black87,
              ),
            ),
          ),
        ],
      ),
    );
  }
}
