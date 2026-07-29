import 'package:abc_androidapp/app/core/helpers/price_formatter.dart';
import 'package:abc_androidapp/app/core/router/app_router.dart';
import 'package:abc_androidapp/app/data/enums/payment_gateway.dart';
import 'package:abc_androidapp/app/data/models/payment/payment_signal.dart';
import 'package:abc_androidapp/app/presentation/blocs/order/order_bloc.dart';
import 'package:abc_androidapp/app/presentation/blocs/signalr/signalr_bloc.dart';
import 'package:abc_androidapp/app/presentation/blocs/signalr/signalr_event.dart';
import 'package:abc_androidapp/app/presentation/blocs/signalr/signalr_state.dart';
import 'package:abc_androidapp/app/presentation/cubits/cart/cart_cubit.dart';
import 'package:abc_androidapp/app/presentation/screens/menu/menu_screen.dart';
import 'package:abc_androidapp/app/presentation/screens/order_confirmation/widgets/limited_item_list_build.dart';
import 'package:abc_androidapp/app/presentation/widgets/common/dialog.dart';
import 'package:abc_androidapp/app/presentation/widgets/common/error.dart';
import 'package:abc_androidapp/app/presentation/widgets/common/hint_dialog.dart';
import 'package:abc_androidapp/app/presentation/widgets/common/loading.dart';
import 'package:abc_androidapp/app/presentation/widgets/common/toast.dart';
import 'package:abc_androidapp/app/presentation/widgets/custom_app_bar.dart';
import 'package:abc_androidapp/app/presentation/widgets/payment_screen/countdown_timer.dart';
import 'package:abc_androidapp/app/presentation/widgets/payment_screen/simple_order_summary.dart';
import 'package:abc_androidapp/config/themes/app_palette.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:qr_flutter/qr_flutter.dart';
import 'package:get/get.dart';
import 'package:webview_flutter/webview_flutter.dart';

class PaymentInfoScreen extends StatefulWidget {
  static const String route = "/order-info";
  const PaymentInfoScreen({super.key});

  @override
  State<PaymentInfoScreen> createState() => _PaymentInfoScreenState();
}

class _PaymentInfoScreenState extends State<PaymentInfoScreen> {
  double amount = 150000;
  String qrData = "";
  String paymentUrl = "";
  String orderId = "";
  String orderCode = "";
  DateTime? expiredDate;
  late CartCubit cart;
  late OrderBloc orderBloc;
  late SignalRBloc signalRBloc;
  bool _isProcessingExpiry = false;
  final GlobalKey<State<CountdownTimer>> _countdownTimerKey = GlobalKey();

  late final WebViewController _webViewController;

  Widget _buildPaymentMethodChip(String title) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 8),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(20),
        border: Border.all(color: Colors.grey.shade300),
      ),
      child: Text(
        title,
        style: TextStyle(
          color: Colors.grey.shade700,
          fontSize: 14,
          fontWeight: FontWeight.w500,
        ),
      ),
    );
  }

  @override
  void initState() {
    super.initState();
    cart = context.read<CartCubit>();
    orderBloc = context.read<OrderBloc>();
    signalRBloc = context.read<SignalRBloc>();
    _webViewController = WebViewController()
          ..setJavaScriptMode(JavaScriptMode.unrestricted)
        // ..setNavigationDelegate(
        //   NavigationDelegate(
        //     onPageFinished: (String url) {
        //       // Xử lý khi trang web hoàn tất tải
        //       if (url.contains('payment_success') ||
        //           url.contains('success=true')) {
        //         // Thanh toán thành công qua URL callback
        //         cart.clear();
        //         AppRouter.navigateToMenuAfterPayment();
        //         HintDialog.showSuccess(
        //           context,
        //           'Thanh toán thành công',
        //           'Đơn hàng sẽ được xử lý!',
        //         );
        //       } else if (url.contains('payment_failed') ||
        //           url.contains('success=false')) {
        //         // Thanh toán thất bại
        //         AppRouter.navigateToMenuAfterPayment();
        //         CustomToast.showError(
        //           context,
        //           'Thanh toán thất bại',
        //         );
        //       }
        //     },
        //   ),
        ;
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

  void _handlePaymentExpired() {
    // Prevent multiple calls
    if (_isProcessingExpiry || !mounted) return;

    _isProcessingExpiry = true;

    // Cancel order và cleanup
    _cleanupAndNavigate();

    // Show notification
    _showExpiryDialog();
  }

  void _cleanupAndNavigate() {
    // Cancel order if exists
    if (orderId.isNotEmpty) {
      orderBloc.add(CancelOrderEvent(orderId));
    }

    // Clear cart
    cart.clear();

    // Cleanup SignalR
    signalRBloc.add(UnsubscribeEvent(eventName: "ReceiveTrans"));
    signalRBloc.add(DisconnectEvent());

    // Navigate back
    AppRouter.navigateToMenuAfterPayment();
  }

  void _showExpiryDialog() {
    Future.delayed(const Duration(milliseconds: 500), () {
      if (Get.context != null) {
        HintDialog.show(Get.context!, 'Hết hạn thanh toán đơn hàng',
            'Vui lòng tạo lại đơn mới',
            isSuccess: false);
      }
    });
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
                   final timerState = _countdownTimerKey.currentState;
                    if (timerState != null && timerState is CountdownTimerController) {
                      (timerState as CountdownTimerController).setEnableCallback(false); // Ép kiểu rõ ràng
                    }
                    AppRouter.navigateToOrderProcessingScreen(
                        orderId, orderCode);

                    // ✅ Show dialog with new context after navigation
                    Future.delayed(const Duration(milliseconds: 500), () {
                      if (Get.context != null) {
                        HintDialog.show(
                          Get.context!, // Context của OrderProcessingScreen
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
                qrData = state.orderPreparedInfo.paymentQr;
                paymentUrl = state.orderPreparedInfo.paymentUrl;
                orderId = state.orderPreparedInfo.orderId;
                orderCode = state.orderPreparedInfo.orderCode;
                amount = state.orderPreparedInfo.finalAmount;
                expiredDate = state.orderPreparedInfo.expiredDate;

                // Xác định phương thức thanh toán từ response
                final orderBloc = context.read<OrderBloc>();
                if (state.orderPreparedInfo.paymentGateway ==
                    PaymentGateway.vnpay) {
                  // Hiển thị WebView cho VNPAY
                  return _buildWebView(paymentUrl);
                } else {
                  return _buildQRPayment();
                }
              }

              return const Center(child: CustomLoading());
            },
          ),
        ),
      ),
    );
  }

  Widget _buildQRPayment() {
    return Center(
      child: SingleChildScrollView(
        child: Padding(
          padding: const EdgeInsets.symmetric(horizontal: 24),
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            crossAxisAlignment: CrossAxisAlignment.center,
            children: [
              const SizedBox(height: 24),
              Text(
                'Vui lòng quét mã QR bằng ứng dụng ngân hàng hoặc ví điện tử để hoàn tất thanh toán',
                textAlign: TextAlign.center,
                style: TextStyle(
                  fontSize: 15,
                  color: Colors.grey.shade600,
                  height: 1.5,
                ),
              ),
              const SizedBox(height: 32),
              if (expiredDate != null)
                CountdownTimer(
                  key: _countdownTimerKey,
                  expiredDate: expiredDate!,
                  onExpired: _handlePaymentExpired,
                ),
              Container(
                padding: const EdgeInsets.all(16),
                decoration: BoxDecoration(
                  color: Colors.white,
                  borderRadius: BorderRadius.circular(16),
                  border: Border.all(
                    color: AppPalette.blue.primary,
                    width: 2,
                  ),
                ),
                child: QrImageView(
                  data: qrData,
                  version: QrVersions.auto,
                  size: 400,
                  eyeStyle: const QrEyeStyle(
                    eyeShape: QrEyeShape.square,
                    color: AppPalette.black,
                  ),
                  dataModuleStyle: const QrDataModuleStyle(
                    dataModuleShape: QrDataModuleShape.square,
                    color: AppPalette.black,
                  ),
                ),
              ),
              const SizedBox(height: 10),
              Text(
                formatPrice(amount) + " đ",
                style: TextStyle(
                  fontSize: 30,
                  color: Colors.grey.shade800,
                  fontWeight: FontWeight.w900,
                ),
              ),
              const SizedBox(height: 20),
              Row(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  _buildStepCircle("1", 'Mở ứng dụng\nngân hàng'),
                  _buildStepDivider(),
                  _buildStepCircle("2", 'Quét mã\nQR'),
                  _buildStepDivider(),
                  _buildStepCircle("3", 'Xác nhận\nthanh toán'),
                ],
              ),
              const SizedBox(height: 16),
              LimitedItemListBuild(
                  items: cart.state.items.values.toList(), isAdjustable: false),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildWebView(String url) {
    _webViewController.loadRequest(Uri.parse(url));

    return WebViewWidget(
      controller: _webViewController,
    );
  }

  Widget _buildStepCircle(String number, String text) {
    return Column(
      children: [
        Container(
          width: 40,
          height: 40,
          decoration: BoxDecoration(
            color: Colors.grey.shade100,
            shape: BoxShape.circle,
          ),
          child: Center(
            child: Text(
              number,
              style: TextStyle(
                color: Colors.grey.shade600,
                fontSize: 16,
                fontWeight: FontWeight.w600,
              ),
            ),
          ),
        ),
        const SizedBox(height: 8),
        Text(
          text,
          textAlign: TextAlign.center,
          style: const TextStyle(fontSize: 13),
        ),
      ],
    );
  }

  Widget _buildStepDivider() {
    return Container(
      width: 40,
      height: 2,
      color: Colors.grey.shade300,
      margin: const EdgeInsets.symmetric(horizontal: 12),
    );
  }
}
