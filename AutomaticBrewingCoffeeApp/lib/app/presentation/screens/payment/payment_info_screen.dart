import 'package:abc_androidapp/app/core/helpers/price_formatter.dart';
import 'package:abc_androidapp/app/core/router/app_router.dart';
import 'package:abc_androidapp/app/core/dependency_injection/service_location.dart';
import 'package:abc_androidapp/app/data/datasources/order_datasource.dart';
import 'package:abc_androidapp/app/data/models/payment/payment_signal.dart';
import 'package:abc_androidapp/app/presentation/blocs/order/order_bloc.dart';
import 'package:abc_androidapp/app/presentation/blocs/signalr/signalr_bloc.dart';
import 'package:abc_androidapp/app/presentation/blocs/signalr/signalr_event.dart';
import 'package:abc_androidapp/app/presentation/blocs/signalr/signalr_state.dart';
import 'package:abc_androidapp/app/presentation/cubits/cart/cart_cubit.dart';
import 'package:abc_androidapp/app/presentation/screens/order_confirmation/widgets/limited_item_list_build.dart';
import 'package:abc_androidapp/app/presentation/widgets/common/error.dart';
import 'package:abc_androidapp/app/presentation/widgets/common/hint_dialog.dart';
import 'package:abc_androidapp/app/presentation/widgets/common/loading.dart';
import 'package:abc_androidapp/app/presentation/widgets/common/toast.dart';
import 'package:abc_androidapp/app/presentation/widgets/custom_app_bar.dart';
import 'package:abc_androidapp/config/themes/app_palette.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:get/get.dart';

class PaymentInfoScreen extends StatefulWidget {
  static const String route = "/order-info";
  const PaymentInfoScreen({super.key});

  @override
  State<PaymentInfoScreen> createState() => _PaymentInfoScreenState();
}

class _PaymentInfoScreenState extends State<PaymentInfoScreen> {
  double amount = 150000;
  String orderId = "";
  String orderCode = "";
  late CartCubit cart;
  late OrderBloc orderBloc;
  late SignalRBloc signalRBloc;
  @override
  void initState() {
    super.initState();
    cart = context.read<CartCubit>();
    orderBloc = context.read<OrderBloc>();
    signalRBloc = context.read<SignalRBloc>();
    _connectAndSubscribeToSignalR();
  }

  void _connectAndSubscribeToSignalR() {
    // Kiểm tra trạng thái hiện tại của SignalRBloc
    if (signalRBloc.state is! SignalRConnected) {
      // Kết nối đến SignalR hub nếu chưa kết nối
      signalRBloc.add(ConnectEvent());
    }

    // Đăng ký lắng nghe sự kiện "ReceiveTrans"
    signalRBloc.add(SubscribeEvent(eventName: "ReceiveTrans"));
    //signalRBloc.add(UnsubscribeEvent(eventName: "ReceiveOrderState"));
  }

  @override
  void dispose() {
    super.dispose();
  }

  Future<bool> _onBackPressed(BuildContext context) async {
    final shouldPop = await showDialog<bool>(
          context: context,
          barrierDismissible: false,
          builder: (dialogContext) {
            return Dialog(
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(16),
              ),
              elevation: 8,
              child: Container(
                padding: const EdgeInsets.all(24),
                decoration: BoxDecoration(
                  color: Colors.white,
                  borderRadius: BorderRadius.circular(16),
                ),
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    // Icon cảnh báo
                    Container(
                      width: 60,
                      height: 60,
                      decoration: BoxDecoration(
                        color: Colors.red.shade50,
                        shape: BoxShape.circle,
                      ),
                      child: Icon(
                        Icons.warning_amber_rounded,
                        color: Colors.red.shade400,
                        size: 32,
                      ),
                    ),

                    const SizedBox(height: 16),

                    // Tiêu đề
                    Text(
                      'Hủy thanh toán?',
                      style: TextStyle(
                        fontSize: 20,
                        fontWeight: FontWeight.bold,
                        color: Colors.grey.shade800,
                      ),
                      textAlign: TextAlign.center,
                    ),

                    const SizedBox(height: 12),

                    LimitedItemListBuild(
                        items: cart.state.items.values.toList(),
                        isAdjustable: false),

                    Text(
                      'Bạn có chắc muốn hủy quá trình thanh toán không?\nĐơn hàng của bạn sẽ không được xử lý.',
                      style: TextStyle(
                        fontSize: 14,
                        color: Colors.grey.shade600,
                        height: 1.5,
                      ),
                      textAlign: TextAlign.center,
                    ),

                    const SizedBox(height: 24),

                    // Buttons
                    Row(
                      children: [
                        // Nút Có
                        Expanded(
                          child: Container(
                            height: 48,
                            decoration: BoxDecoration(
                              borderRadius: BorderRadius.circular(12),
                              gradient: LinearGradient(
                                colors: [
                                  Colors.red.shade400,
                                  Colors.red.shade600,
                                ],
                              ),
                              boxShadow: [
                                BoxShadow(
                                  color: Colors.red.shade200,
                                  blurRadius: 8,
                                  offset: const Offset(0, 4),
                                ),
                              ],
                            ),
                            child: TextButton(
                              onPressed: () {
                                orderBloc.add(CancelOrderEvent(orderId));
                                cart.clear();
                                AppRouter.navigateToMenuAfterPayment();
                              },
                              style: TextButton.styleFrom(
                                backgroundColor: Colors.transparent,
                                foregroundColor: Colors.white,
                                shape: RoundedRectangleBorder(
                                  borderRadius: BorderRadius.circular(12),
                                ),
                              ),
                              child: const Text(
                                'Hủy thanh toán',
                                style: TextStyle(
                                  fontSize: 16,
                                  fontWeight: FontWeight.w600,
                                ),
                              ),
                            ),
                          ),
                        ),

                        const SizedBox(width: 12),
                        // Nút Không
                        Expanded(
                          child: Container(
                            height: 48,
                            decoration: BoxDecoration(
                              border: Border.all(
                                color: AppPalette.blue.primary,
                                width: 1.5,
                              ),
                              borderRadius: BorderRadius.circular(12),
                            ),
                            child: TextButton(
                              onPressed: () {
                                Navigator.of(dialogContext).pop(false);
                              },
                              style: TextButton.styleFrom(
                                backgroundColor: Colors.transparent,
                                foregroundColor: AppPalette.blue.primary,
                                shape: RoundedRectangleBorder(
                                  borderRadius: BorderRadius.circular(12),
                                ),
                              ),
                              child: const Text(
                                'Tiếp tục',
                                style: TextStyle(
                                  fontSize: 16,
                                  fontWeight: FontWeight.w600,
                                ),
                              ),
                            ),
                          ),
                        ),
                      ],
                    ),
                  ],
                ),
              ),
            );
          },
        ) ??
        false;

    if (shouldPop && context.mounted) {
      Navigator.of(context).pop();
      return true;
    }
    return false;
  }

  Future<void> _handleSandboxPaymentSuccess() async {
    if (!mounted || orderId.isEmpty) return;

    final confirmed =
        await sl<OrderDatasource>().markSandboxPaymentSuccess(orderId);
    if (!confirmed) {
      if (mounted) {
        CustomToast.showError(context, 'Không thể xác nhận thanh toán Sandbox');
      }
      return;
    }

    cart.clear();
    signalRBloc.add(UnsubscribeEvent(eventName: "ReceiveTrans"));
    signalRBloc.add(DisconnectEvent());
    AppRouter.navigateToOrderProcessingScreen(orderId, orderCode);

    Future.delayed(const Duration(milliseconds: 500), () {
      if (Get.context != null) {
        HintDialog.show(
          Get.context!,
          'Thanh toán thành công',
          'Sandbox đã xác nhận thanh toán. Đơn hàng đang được xử lý!',
        );
      }
    });
  }

  Widget _buildSandboxPayment() {
    return Center(
      child: SingleChildScrollView(
        padding: const EdgeInsets.all(24),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(
              Icons.payment_outlined,
              color: AppPalette.blue.primary,
              size: 72,
            ),
            const SizedBox(height: 20),
            const Text(
              'Sandbox payment',
              style: TextStyle(fontSize: 24, fontWeight: FontWeight.w700),
            ),
            const SizedBox(height: 8),
            Text(
              'Thanh toán thật sẽ được tích hợp sau.',
              textAlign: TextAlign.center,
              style: TextStyle(color: Colors.grey.shade600, fontSize: 15),
            ),
            const SizedBox(height: 24),
            Text(
              '${formatPrice(amount)} đ',
              style: TextStyle(
                color: Colors.grey.shade800,
                fontSize: 30,
                fontWeight: FontWeight.w900,
              ),
            ),
            const SizedBox(height: 32),
            SizedBox(
              width: double.infinity,
              child: ElevatedButton.icon(
                onPressed: _handleSandboxPaymentSuccess,
                icon: const Icon(Icons.check_circle_outline),
                label: const Text('Thanh toán thành công'),
                style: ElevatedButton.styleFrom(
                  backgroundColor: AppPalette.blue.primary,
                  foregroundColor: Colors.white,
                  padding: const EdgeInsets.symmetric(vertical: 15),
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(8),
                  ),
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return PopScope<Object?>(
      canPop: false,
      onPopInvokedWithResult: (bool didPop, Object? result) async {
        if (!didPop) {
          _onBackPressed(context);
        }
      },
      child: Scaffold(
        backgroundColor: Colors.white,
        appBar: CustomAppBar(
          title: "Thanh toán",
          automaticallyImplyLeading: true,
          onBackPressed: () async {
            await _onBackPressed(context);
          },
        ),
        body: BlocListener<SignalRBloc, SignalRState>(
          listener: (context, signalRState) async {
            if (signalRState is SignalREventReceived &&
                signalRState.eventName == "ReceiveTrans") {
              final dynamic paymentData = signalRState.data;
              try {
                if (paymentData is Map<String, dynamic>) {
                  final paymentSignal = PaymentSignal.fromJson(paymentData);
                  if (paymentSignal.orderId != orderId) {
                    return;
                  }
                  bool isSuccess = paymentSignal.isSuccess;
                  // Debug thông tin nhận được
                  print("Payment signal: $paymentSignal");
                  print("Payment status: ${paymentSignal.paymentStatus}");
                  cart.clear();
                  signalRBloc.add(UnsubscribeEvent(eventName: "ReceiveTrans"));
                  signalRBloc.add(DisconnectEvent());
                  if (!isSuccess) {
                    AppRouter.navigateToMenuAfterPayment();
                    Future.delayed(const Duration(milliseconds: 500), () {
                      if (Get.context != null) {
                        CustomToast.showError(
                          Get.context!,
                          'Thanh toán thất bại',
                        );
                      }
                    });
                  } else {
                    AppRouter.navigateToOrderProcessingScreen(
                        orderId, orderCode);

                    Future.delayed(const Duration(milliseconds: 500), () {
                      if (Get.context != null) {
                        HintDialog.show(
                          Get.context!,
                          'Thanh toán thành công',
                          'Đơn hàng đang được xử lý!',
                        );
                      }
                    });
                  }
                }
              } catch (e) {
                print("Error parsing payment data: $e");
              }
            }
          },
          child: BlocBuilder<OrderBloc, OrderState>(
            builder: (context, state) {
              if (state is OrderLoading) {
                return const Center(child: CustomLoading());
              }

              if (state is OrderError) {
                var errorMessage = state.message;
                cart.clear();
                return CustomError(
                  title: "Có lỗi xảy ra",
                  subTitle: errorMessage,
                  onback: () {
                    AppRouter.navigateToMenuAfterPayment();
                  },
                );
              }

              if (state is CreateOrderDone) {
                orderId = state.orderPreparedInfo.orderId;
                orderCode = state.orderPreparedInfo.orderCode;
                amount = state.orderPreparedInfo.finalAmount;

                return _buildSandboxPayment();
              }

              return const Center(child: CustomLoading());
            },
          ),
        ),
      ),
    );
  }
}
