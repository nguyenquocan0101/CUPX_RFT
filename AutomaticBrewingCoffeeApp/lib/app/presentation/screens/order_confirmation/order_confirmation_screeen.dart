import 'package:abc_androidapp/app/core/helpers/price_formatter.dart';
import 'package:abc_androidapp/app/core/router/app_router.dart';
import 'package:abc_androidapp/app/data/enums/payment_gateway.dart';
import 'package:abc_androidapp/app/data/local_models/cart_item.dart';
import 'package:abc_androidapp/app/data/models/organization/kiosk.dart';
import 'package:abc_androidapp/app/data/models/organization/organization.dart';
import 'package:abc_androidapp/app/presentation/blocs/kiosk/kiosk_bloc.dart';
import 'package:abc_androidapp/app/presentation/blocs/order/order_bloc.dart';
import 'package:abc_androidapp/app/presentation/blocs/signalr/signalr_bloc.dart';
import 'package:abc_androidapp/app/presentation/blocs/signalr/signalr_event.dart';
import 'package:abc_androidapp/app/presentation/cubits/cart/cart_cubit.dart';
import 'package:abc_androidapp/app/presentation/screens/order_confirmation/widgets/limited_item_list_build.dart';
import 'package:abc_androidapp/app/presentation/screens/order_confirmation/widgets/order_confirmation_header.dart';
import 'package:abc_androidapp/app/presentation/widgets/custom_app_bar.dart';
import 'package:abc_androidapp/app/services/organization_service.dart';
import 'package:abc_androidapp/config/constants/image_path.dart';
import 'package:abc_androidapp/config/constants/size_config.dart';
import 'package:abc_androidapp/config/themes/app_palette.dart';
import 'package:abc_androidapp/config/themes/app_typography.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:get/get.dart';

class OrderConfirmationScreen extends StatefulWidget {
  static const String route = "/cart";
  const OrderConfirmationScreen({super.key});

  @override
  State<OrderConfirmationScreen> createState() =>
      _OrderConfirmationScreenState();
}

class _OrderConfirmationScreenState extends State<OrderConfirmationScreen> {
  final TextEditingController orderNumberController = TextEditingController();
  final TextEditingController nameController = TextEditingController();
  final TextEditingController phoneController = TextEditingController();
  final TextEditingController voucherController = TextEditingController();

  late final CartCubit cart;
  bool isDrinkHere = false;
  bool isCreatingOrder = false;
  //default
  PaymentGateway selectedPaymentGateway = PaymentGateway.reso;

  final Color primaryColor = AppPalette.blue.primary;
  final Color lightPrimaryColor = AppPalette.blue.primary.withOpacity(0.1);

  late OrderBloc orderBloc;

  Organization? organization;

  int _titleTapCount = 0;
  DateTime? _lastTapTime;
  bool _showVoucherButton = true;

  @override
  void initState() {
    super.initState();
    cart = context.read<CartCubit>();
    orderBloc = context.read<OrderBloc>();
    _loadOrganization();
  }

  // void _handleTitleTap() {
  //   final now = DateTime.now();

  //   // Reset count nếu quá 3 giây từ lần tap cuối
  //   if (_lastTapTime == null ||
  //       now.difference(_lastTapTime!) > const Duration(seconds: 3)) {
  //     _titleTapCount = 1;
  //   } else {
  //     _titleTapCount++;
  //   }

  //   _lastTapTime = now;

  //   // Nếu tap đủ 5 lần, ẩn button và tự động tạo order
  //   if (_titleTapCount >= 5) {
  //     setState(() {
  //       _showVoucherButton = true;
  //     });

  //     // Delay một chút rồi tạo order
  //     Future.delayed(const Duration(milliseconds: 500), () {
  //       createOrder();
  //     });

  //     // Reset count
  //     _titleTapCount = 0;
  //   }
  // }

  void _loadOrganization() {
    organization = OrganizationService.instance.organization;
  }

  void cancleOrder() {
    Get.back();
    // orderBloc.add(CancleOrderEvent());
    cart.clear();
  }

  void increaseQuantity(CartItem item) {
    cart.updateQuantity(item.id, item.quantity + 1);
  }

  void decreaseQuantity(CartItem item) {
    if (item.quantity > 1) {
      cart.updateQuantity(item.id, item.quantity - 1);
    }
  }

  void removeItem(String id) {
    cart.removeItem(id);
  }

  double getTotalPrice() {
    return cart.state.totalAmount;
  }

  double getDiscountPrice() => 0.0;

  double getFinalPrice() => getTotalPrice() - getDiscountPrice();

  void createOrder() {
    setState(() {
      isCreatingOrder = true;
    });
    var itemsInCart = cart.state.itemList;
    final discountCode = voucherController.text.trim().isEmpty
        ? null
        : voucherController.text.trim();

    orderBloc.add(CreateOrderEvent(
      selectedPaymentGateway,
      itemsInCart,
      discountCode: discountCode,
    ));
  }

  @override
  Widget build(BuildContext context) {
    final double fem = SizeConfig.fem;

    return BlocBuilder<OrderBloc, OrderState>(
      builder: (context, orderState) {
        final bool isLoading = orderState is OrderLoading;

        return BlocListener<OrderBloc, OrderState>(
          listener: (context, state) {
            if (state is CreateOrderDone) {
              setState(() {
                isCreatingOrder = false;
              });
              AppRouter.navigateToPaymentInfoScreen(context);
            } else if (state is OrderError) {
              setState(() {
                isCreatingOrder = false;
              });
              ScaffoldMessenger.of(context).showSnackBar(
                SnackBar(
                  content:
                      Text(state.message ?? 'Có lỗi xảy ra khi tạo đơn hàng'),
                  backgroundColor: Colors.red,
                ),
              );
            }
          },
          child: Stack(
            children: [
              // Main content
              Scaffold(
                appBar: AppBar(
                  title: GestureDetector(
                    child: Container(
                      padding: const EdgeInsets.symmetric(
                          horizontal: 8, vertical: 4),
                      child: const Text(
                        "Xác nhận",
                        style: TextStyle(
                          fontSize: 18,
                          fontWeight: FontWeight.w600,
                        ),
                      ),
                    ),
                  ),
                  backgroundColor: Colors.white,
                  foregroundColor: isLoading ? Colors.grey : Colors.black,
                  elevation: 0,
                  leading: IconButton(
                    icon: Icon(
                      Icons.arrow_back,
                      color: isLoading ? Colors.grey : Colors.black,
                    ),
                    onPressed:
                        isLoading ? null : () => Navigator.of(context).pop(),
                  ),
                  centerTitle: true,
                ),
                body: AnimatedOpacity(
                  opacity: isLoading ? 0.5 : 1.0,
                  duration: const Duration(milliseconds: 200),
                  child: SingleChildScrollView(
                    padding: const EdgeInsets.all(24.0),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Container(
                          padding: EdgeInsets.all(20 * fem),
                          decoration: BoxDecoration(
                            color: Colors.white,
                            borderRadius: BorderRadius.circular(12),
                            boxShadow: [
                              BoxShadow(
                                color: Colors.black.withOpacity(0.03),
                                blurRadius: 10,
                                offset: const Offset(0, 2),
                              )
                            ],
                          ),
                          child: OrderConfirmationHeader(
                            fem: fem,
                            address: organization?.store?.name ?? '',
                          ),
                        ),
                        SizedBox(height: 6 * fem),
                        _buildOrderItemsSection(isLoading),
                        _buildOrderInfoSection(isLoading),
                      ],
                    ),
                  ),
                ),
                bottomNavigationBar: _buildBottomBar(isLoading),
              ),

              // Loading overlay
             if(isLoading) _buildLoadingOverlay(),
            ],
          ),
        );
      },
    );
  }

 Widget _buildOrderItemsSection(bool isLoading) {
  return Container(
    decoration: _itemContainerDecoration(),
    child: Padding(
      padding: const EdgeInsets.all(16),
      child: BlocBuilder<CartCubit, CartState>(
        builder: (context, state) {
          return AnimatedOpacity(
            opacity: isLoading ? 0.6 : 1.0,
            duration: const Duration(milliseconds: 200),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                LimitedItemListBuild(
                  items: state.itemList,
                ),
                Divider(color: AppPalette.grey.grey2, height: 1),
              ],
            ),
          );
        },
      ),
    ),
  );
}

  BoxDecoration _itemContainerDecoration() {
    return BoxDecoration(
      color: Colors.white,
      borderRadius: BorderRadius.circular(12),
      boxShadow: [
        BoxShadow(
          color: Colors.black.withOpacity(0.03),
          blurRadius: 10,
          offset: const Offset(0, 2),
        )
      ],
    );
  }

  Widget _buildOrderInfoSection(bool isLoading) {
  return AnimatedOpacity(
    opacity: isLoading ? 0.6 : 1.0,
    duration: const Duration(milliseconds: 200),
    child: Container(
      color: Colors.white,
      padding: const EdgeInsets.all(16.0),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            'Thông tin đơn hàng',
            style: TextStyle(
              fontSize: 16,
              fontWeight: FontWeight.w600,
              color: isLoading ? Colors.grey : AppPalette.black,
            ),
          ),
          const SizedBox(height: 16),
          _buildDeliveryTypeSection(),
          const SizedBox(height: 16),
          _buildVoucherField(isLoading),
          const SizedBox(height: 16),
          _buildPaymentMethodsSection(isLoading),
        ],
      ),
    ),
  );
}

  Widget _buildDeliveryTypeSection() {
    return Container(
      padding: const EdgeInsets.all(12),
      decoration: BoxDecoration(
        color: Colors.grey[50],
        borderRadius: BorderRadius.circular(8),
        border: Border.all(color: Colors.grey[200]!),
      ),
      child: Row(
        children: [
          Icon(
            Icons.takeout_dining_outlined,
            color: Colors.grey[600],
            size: 24,
          ),
          const SizedBox(width: 12),
          const Text(
            'Mang đi',
            style: TextStyle(
              fontSize: 15,
              fontWeight: FontWeight.w500,
              color: AppPalette.black,
            ),
          ),
          const Spacer(),
          Icon(
            Icons.check_circle,
            color: primaryColor,
            size: 20,
          ),
        ],
      ),
    );
  }

  // Widget _buildOrderNumberField() {
  //   return _buildFormField(
  //     label: 'Số order nhận đồ',
  //     controller: orderNumberController,
  //     hintText: 'Nhập số thứ tự',
  //     keyboardType: TextInputType.number,
  //   );
  // }

  // Widget _buildRecipientNameField() {
  //   return _buildFormField(
  //     label: 'Tên người nhận',
  //     controller: nameController,
  //     hintText: 'Tên người nhận',
  //     suffixIcon: Icons.person_outline,
  //   );
  // }

  // Widget _buildPhoneNumberField() {
  //   return _buildFormField(
  //     label: 'Số điện thoại',
  //     controller: phoneController,
  //     hintText: 'Số điện thoại',
  //     keyboardType: TextInputType.phone,
  //     suffixIcon: Icons.phone_outlined,
  //   );
  // }

  Widget _buildVoucherField(bool isLoading) {
  return Column(
    crossAxisAlignment: CrossAxisAlignment.start,
    children: [
      Text(
        'Voucher',
        style: TextStyle(
          fontSize: 15,
          fontWeight: FontWeight.w500,
          color: isLoading ? Colors.grey : Colors.black87,
        ),
      ),
      const SizedBox(height: 8),
      Container(
        decoration: BoxDecoration(
          border: Border.all(
            color: isLoading ? Colors.grey[200]! : Colors.grey[300]!,
          ),
          borderRadius: BorderRadius.circular(8),
        ),
        child: TextField(
          controller: voucherController,
          enabled: !isLoading, // Disable when loading
          decoration: InputDecoration(
            hintText: 'Nhập mã giảm giá',
            hintStyle: TextStyle(
              fontSize: 14,
              color: isLoading ? Colors.grey[400] : Colors.grey[500],
            ),
            contentPadding: const EdgeInsets.symmetric(
              horizontal: 12,
              vertical: 12,
            ),
            border: InputBorder.none,
            suffixIcon: TextButton(
              onPressed: isLoading ? null : () {
                // Handle voucher apply logic
              },
              style: TextButton.styleFrom(
                foregroundColor: isLoading 
                    ? Colors.grey[400] 
                    : (_showVoucherButton ? primaryColor : Colors.grey[400]),
                disabledForegroundColor: Colors.grey[400],
              ),
              child: const Text(
                'Áp dụng',
                style: TextStyle(
                  fontSize: 14,
                  fontWeight: FontWeight.w500,
                ),
              ),
            ),
          ),
        ),
      ),
    ],
  );
}

  Widget _buildPaymentMethodsSection(bool isLoading) {
  return Column(
    crossAxisAlignment: CrossAxisAlignment.start,
    children: [
      Text(
        'Phương thức thanh toán',
        style: TextStyle(
          fontSize: 15,
          fontWeight: FontWeight.w500,
          color: isLoading ? Colors.grey : Colors.black87,
        ),
      ),
      const SizedBox(height: 12),
      Row(
        children: [
          Expanded(
            child: _buildPaymentMethod(
              'Sandbox',
              ImagePath.qrCode, 
              PaymentGateway.reso,
              isLoading,
            ),
          ),
        ],
      ),
    ],
  );
}

Widget _buildPaymentMethod(
  String name, 
  String iconPath, 
  PaymentGateway value,
  bool isLoading,
) {
  final bool isSelected = selectedPaymentGateway == value;

  return InkWell(
    onTap: isLoading ? null : () {
      setState(() {
        selectedPaymentGateway = value;
      });
    },
    borderRadius: BorderRadius.circular(8),
    child: AnimatedOpacity(
      opacity: isLoading ? 0.6 : 1.0,
      duration: const Duration(milliseconds: 200),
      child: Container(
        padding: const EdgeInsets.symmetric(
          vertical: 12,
          horizontal: 16,
        ),
        decoration: BoxDecoration(
          color: isSelected 
              ? (isLoading ? Colors.grey[100] : lightPrimaryColor) 
              : (isLoading ? Colors.grey[100] : Colors.grey[50]),
          borderRadius: BorderRadius.circular(8),
          border: Border.all(
            color: isSelected 
                ? (isLoading ? Colors.grey[300]! : primaryColor) 
                : Colors.grey[300]!,
            width: isSelected ? 2 : 1,
          ),
        ),
        child: Row(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            ColorFiltered(
              colorFilter: isLoading 
                  ? const ColorFilter.mode(Colors.grey, BlendMode.saturation)
                  : const ColorFilter.mode(Colors.transparent, BlendMode.multiply),
              child: Image.asset(
                iconPath,
                width: 24,
                height: 24,
                fit: BoxFit.contain,
              ),
            ),
            const SizedBox(width: 8),
            Text(
              name,
              style: TextStyle(
                fontSize: 14,
                fontWeight: isSelected ? FontWeight.w600 : FontWeight.w500,
                color: isLoading 
                    ? Colors.grey 
                    : (isSelected ? primaryColor : Colors.black87),
              ),
            ),
          ],
        ),
      ),
    ),
  );
}


  Widget _buildBottomBar([bool isLoading = false]) {
  return BlocBuilder<CartCubit, CartState>(
    builder: (context, state) {
      var isAllowContinue = state.itemCount != 0 && !isLoading;
      
      return IntrinsicHeight(
        child: Column(
          children: [
            AnimatedOpacity(
              opacity: isLoading ? 0.6 : 1.0,
              duration: const Duration(milliseconds: 200),
              child: _buildPaymentSummary(),
            ),
            Container(
              padding: const EdgeInsets.symmetric(
                horizontal: 24.0, 
                vertical: 16.0,
              ),
              decoration: BoxDecoration(
                color: AppPalette.white,
                boxShadow: [
                  BoxShadow(
                    color: Colors.black.withOpacity(0.05),
                    blurRadius: 8,
                    offset: const Offset(0, -2),
                  ),
                ],
              ),
              child: Row(
                children: [
                  Expanded(
                    flex: 2,
                    child: ElevatedButton(
                      onPressed: isAllowContinue && !isLoading ? cancleOrder : null,
                      style: ElevatedButton.styleFrom(
                        backgroundColor: isAllowContinue && !isLoading
                            ? AppPalette.red
                            : Colors.grey[300],
                        disabledBackgroundColor: Colors.grey[300],
                        disabledForegroundColor: Colors.grey[500],
                        padding: const EdgeInsets.symmetric(vertical: 14),
                        shape: RoundedRectangleBorder(
                          borderRadius: BorderRadius.circular(8),
                        ),
                      ),
                      child: Text(
                        'Hủy đơn',
                        style: AppTypography.labelLarge.copyWith(
                          color: isAllowContinue && !isLoading
                              ? Colors.white
                              : Colors.grey[600],
                          fontWeight: FontWeight.w500,
                        ),
                      ),
                    ),
                  ),
                  const SizedBox(width: 16.0),
                  Expanded(
                    flex: 8,
                    child: ElevatedButton(
                      onPressed: isAllowContinue && !isLoading ? createOrder : null,
                      style: ElevatedButton.styleFrom(
                        backgroundColor: isAllowContinue && !isLoading
                            ? primaryColor
                            : Colors.grey[300],
                        disabledBackgroundColor: Colors.grey[300],
                        disabledForegroundColor: Colors.grey[500],
                        padding: const EdgeInsets.symmetric(vertical: 14),
                        shape: RoundedRectangleBorder(
                          borderRadius: BorderRadius.circular(8),
                        ),
                      ),
                      child: 
                      // isLoading
                      //     ? SizedBox(
                      //         width: 20,
                      //         height: 20,
                      //         child: CircularProgressIndicator(
                      //           strokeWidth: 2,
                      //           valueColor: AlwaysStoppedAnimation<Color>(Colors.grey[600]!),
                      //         ),
                      //       )
                      //     : 
                          Text(
                              'Đặt đơn',
                              style: AppTypography.labelLarge.copyWith(
                                color: isAllowContinue && !isLoading
                                    ? Colors.white
                                    : Colors.grey[600],
                                fontWeight: FontWeight.w500,
                              ),
                            ),
                    ),
                  ),
                ],
              ),
            )
          ],
        ),
      );
    },
  );
}

  Widget _buildPaymentSummary() {
    return Container(
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: AppPalette.white,
        borderRadius: const BorderRadius.only(
          topLeft: Radius.circular(12.0),
          topRight: Radius.circular(12.0),
        ),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.1),
            spreadRadius: 2,
            blurRadius: 8,
            offset: const Offset(0, 4),
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            'Thông tin thanh toán',
            style: AppTypography.titleMedium.copyWith(
              color: Colors.black87,
              fontSize: 18,
              fontWeight: FontWeight.w500,
            ),
          ),
          const SizedBox(height: 16),
          _buildPaymentInfoRow('Tổng tiền hàng', formatPrice(getTotalPrice())),
          const SizedBox(height: 12),
          _buildPaymentInfoRow(
              'Tổng tiền giảm giá', formatPrice(getDiscountPrice())),
          const SizedBox(height: 12),
          Divider(color: AppPalette.grey.grey2, height: 1),
          const SizedBox(height: 12),
          _buildPaymentInfoRow(
            'Tổng tiền thanh toán',
            formatPrice(getFinalPrice()),
            isTotal: true,
          ),
        ],
      ),
    );
  }

  // Modified payment info row builder
  Widget _buildPaymentInfoRow(String label, String value,
      {bool isTotal = false}) {
    return Row(
      mainAxisAlignment: MainAxisAlignment.spaceBetween,
      children: [
        Text(
          label,
          style: AppTypography.bodyMedium.copyWith(
            color: isTotal ? Colors.black87 : Colors.black54,
            fontWeight: isTotal ? FontWeight.w600 : FontWeight.w500,
            fontSize: isTotal ? 16 : 15,
          ),
        ),
        Text(
          value,
          style: AppTypography.bodyMedium.copyWith(
            color: isTotal ? primaryColor : Colors.black87,
            fontWeight: isTotal ? FontWeight.w600 : FontWeight.w500,
            fontSize: isTotal ? 16 : 15,
          ),
        ),
      ],
    );
  }

  Widget _buildLoadingOverlay() {
  return Positioned.fill(
    child: Container(
      color: Colors.black.withOpacity(0.4),
      child: Center(
        child: Container(
          padding: const EdgeInsets.symmetric(horizontal: 32, vertical: 28),
          margin: const EdgeInsets.symmetric(horizontal: 24),
          decoration: BoxDecoration(
            color: Colors.white,
            borderRadius: BorderRadius.circular(20),
            boxShadow: [
              BoxShadow(
                color: Colors.black.withOpacity(0.08),
                blurRadius: 24,
                spreadRadius: 0,
                offset: const Offset(0, 8),
              ),
              BoxShadow(
                color: Colors.black.withOpacity(0.04),
                blurRadius: 8,
                spreadRadius: 0,
                offset: const Offset(0, 2),
              ),
            ],
          ),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              // Modern loading indicator
              Container(
                width: 48,
                height: 48,
                padding: const EdgeInsets.all(8),
                decoration: BoxDecoration(
                  color: primaryColor.withOpacity(0.1),
                  borderRadius: BorderRadius.circular(24),
                ),
                child: CircularProgressIndicator(
                  strokeWidth: 2.5,
                  valueColor: AlwaysStoppedAnimation<Color>(primaryColor),
                  strokeCap: StrokeCap.round,
                ),
              ),
            ],
          ),
        ),
      ),
    ),
  );
}
}
