import 'package:abc_androidapp/app/core/router/app_router.dart';
import 'package:abc_androidapp/app/data/models/organization/organization.dart';
import 'package:abc_androidapp/app/presentation/blocs/order/order_bloc.dart';
import 'package:abc_androidapp/app/presentation/blocs/signalr/signalr_bloc.dart';
import 'package:abc_androidapp/app/presentation/blocs/signalr/signalr_event.dart';
import 'package:abc_androidapp/app/presentation/blocs/signalr/signalr_state.dart';
import 'package:abc_androidapp/app/presentation/cubits/cart/cart_cubit.dart';
import 'package:abc_androidapp/app/presentation/widgets/common/hint_dialog.dart';
import 'package:abc_androidapp/app/services/organization_service.dart';
import 'package:abc_androidapp/config/themes/app_palette.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:get/get.dart';
import 'package:get/instance_manager.dart';

class OrderProcessingScreen extends StatefulWidget {
  static const String route = "/orderprocessing";
  final String orderId;
  final String orderCode;

  const OrderProcessingScreen({
    super.key,
    required this.orderId,
    required this.orderCode,
  });

  @override
  State<OrderProcessingScreen> createState() => _OrderProcessingScreenState();
}

class _OrderProcessingScreenState extends State<OrderProcessingScreen>
    with TickerProviderStateMixin {
  late SignalRBloc signalRBloc;
  late CartCubit cart;
  late AnimationController _pulseController;
  late AnimationController _progressController;
  late AnimationController _waveController;
  late Animation<double> _pulseAnimation;
  late Animation<double> _progressAnimation;
  late Animation<double> _waveAnimation;
  late Organization? organization;
  late OrderBloc orderBloc;

  DateTime? _longPressStartTime;
  static const Duration _requiredLongPressDuration = Duration(seconds: 3);

  @override
  void initState() {
    super.initState();
    signalRBloc = context.read<SignalRBloc>();
    orderBloc = context.read<OrderBloc>();
    cart = context.read<CartCubit>();

    // Setup animations
    _pulseController = AnimationController(
      duration: const Duration(seconds: 2),
      vsync: this,
    )..repeat(reverse: true);

    _progressController = AnimationController(
      duration: const Duration(seconds: 3),
      vsync: this,
    )..repeat();

    _pulseAnimation = Tween<double>(
      begin: 0.8,
      end: 1.2,
    ).animate(CurvedAnimation(
      parent: _pulseController,
      curve: Curves.easeInOut,
    ));

    _waveController = AnimationController(
      duration: const Duration(milliseconds: 2000),
      vsync: this,
    )..repeat();

    _progressAnimation = Tween<double>(
      begin: 0.0,
      end: 1.0,
    ).animate(CurvedAnimation(
      parent: _progressController,
      curve: Curves.linear,
    ));

    _waveAnimation = Tween<double>(
      begin: 0.0,
      end: 1.0,
    ).animate(CurvedAnimation(
      parent: _waveController,
      curve: Curves.easeOut,
    ));

    _subscribeToOrderUpdates();
    _loadOrganization();
  }

  void _loadOrganization() {
    organization = OrganizationService.instance.organization;
  }

  void _subscribeToOrderUpdates() {
    // Subscribe to order status updates
    signalRBloc.add(SubscribeEvent(eventName: "ReceiveOrderState"));
  }

  @override
  void dispose() {
    _pulseController.dispose();
    _progressController.dispose();
    _waveController.dispose();
    signalRBloc.add(UnsubscribeEvent(eventName: "ReceiveOrderState"));
    signalRBloc.add(DisconnectEvent());
    super.dispose();
  }

  void _handleOrderCompleted() {
    // Stop animations
    _pulseController.stop();
    _progressController.stop();

    AppRouter.navigateToMenuAfterPayment();
    Future.delayed(const Duration(milliseconds: 500), () {
      if (Get.context != null) {
        HintDialog.show(Get.context!, 'Pha chế thành công',
            'Đơn hàng ${widget.orderCode} đã hoàn thành.\nLiên hệ **${organization?.contactPhone}** nếu cần được hỗ trợ.',
            isSuccess: true);
      }
    });
  }

  void _handleOrderFailed() {
    // Stop animations
    _pulseController.stop();
    _progressController.stop();
    orderBloc.add(CancelOrderEvent(widget.orderId));
    AppRouter.navigateToMenuAfterPayment();
    Future.delayed(const Duration(milliseconds: 500), () {
      if (Get.context != null) {
        HintDialog.show(
          Get.context!,
          'Pha chế thất bại',
          'Đơn hàng ${widget.orderCode} đã thất bại.\nLiên hệ nhân viên tại quầy hoặc liên hệ qua số điện thoại **${organization?.contactPhone}** để được hỗ trợ.\nCUPX sẽ sớm hoàn tiền cho bạn!',
          isSuccess: false,
        );
      }
    });
  }

  void _handleLongPressStart() {
    _longPressStartTime = DateTime.now();
    print('Long press started at: $_longPressStartTime');
  }

  void _handleLongPressEnd() {
    if (_longPressStartTime != null) {
      final pressDuration = DateTime.now().difference(_longPressStartTime!);
      print('Long press duration: ${pressDuration.inSeconds} seconds');

      if (pressDuration >= _requiredLongPressDuration) {
        print('Long press requirement met - exiting');
        _handleLongPressExit();
      } else {
        print(
            'Long press too short - need ${_requiredLongPressDuration.inSeconds} seconds');
      }
    }
    _longPressStartTime = null;
  }

  void _handleLongPressExit() {
    _pulseController.stop();
    _progressController.stop();
    AppRouter.navigateToMenuAfterPayment();
  }

  Widget _buildWaveRing(double animationValue, double delay, double maxRadius) {
    final adjustedValue = (animationValue - delay).clamp(0.0, 1.0);
    final opacity = (1.0 - adjustedValue).clamp(0.0, 1.0);
    final radius = adjustedValue * maxRadius;

    return Container(
      width: radius * 2,
      height: radius * 2,
      decoration: BoxDecoration(
        shape: BoxShape.circle,
        border: Border.all(
          color: AppPalette.blue.primary.withOpacity(opacity * 0.3),
          width: 2.0,
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return PopScope<Object?>(
      canPop: false, // Prevent back navigation
      child: Scaffold(
        backgroundColor: Colors.white,
        body: BlocListener<SignalRBloc, SignalRState>(
          listener: (context, signalRState) {
            if (signalRState is SignalREventReceived &&
                signalRState.eventName == "ReceiveOrderState") {
              final dynamic orderData = signalRState.data;

              try {
                if (orderData is Map<String, dynamic>) {
                  final orderId = orderData['orderId'] as String?;
                  final orderStatus = orderData['orderStatus'] as String?;

                  print("Order update received: $orderData");
                  print("Order ID: $orderId, Status: $orderStatus");

                  // Check if this update is for our order and order is completed
                  if (orderId == widget.orderId && orderStatus == "Completed") {
                    _handleOrderCompleted();
                  } else if (orderId == widget.orderId &&
                      orderStatus != "Completed") {
                    _handleOrderFailed();
                  }
                }
              } catch (e) {
                print("Error parsing order update data: $e");
              }
            }
          },
          child: SafeArea(
            child: Padding(
              padding: const EdgeInsets.all(24),
              child: Column(
                children: [
                  // Header - Order Information
                  Container(
                    padding: const EdgeInsets.symmetric(vertical: 20),
                    child: Row(
                      children: [
                        Container(
                          padding: const EdgeInsets.all(12),
                          decoration: BoxDecoration(
                            color: AppPalette.blue.primary.withOpacity(0.1),
                            borderRadius: BorderRadius.circular(16),
                          ),
                          child: Icon(
                            Icons.coffee_maker_outlined,
                            color: AppPalette.blue.primary,
                            size: 28,
                          ),
                        ),
                        const SizedBox(width: 16),
                        Expanded(
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Text(
                                'Đơn hàng #${widget.orderCode}',
                                style: const TextStyle(
                                  fontSize: 18,
                                  fontWeight: FontWeight.w600,
                                  color: Colors.black87,
                                ),
                                overflow: TextOverflow.ellipsis,
                                maxLines: 1,
                              ),
                              const SizedBox(height: 4),
                            ],
                          ),
                        ),
                      ],
                    ),
                  ),

                  const Spacer(flex: 1),

                  // Main Animation Section
                  SizedBox(
                    width: 300,
                    height: 300,
                    child: Stack(
                      alignment: Alignment.center,
                      children: [
                        // ✅ Wave rings
                        AnimatedBuilder(
                          animation: _waveAnimation,
                          builder: (context, child) {
                            return Stack(
                              alignment: Alignment.center,
                              children: [
                                // Wave ring 1
                                _buildWaveRing(_waveAnimation.value, 0.0, 150),

                                // Wave ring 2 (delayed)
                                _buildWaveRing(_waveAnimation.value, 0.2, 150),

                                // Wave ring 3 (more delayed)
                                _buildWaveRing(_waveAnimation.value, 0.4, 150),

                                // Wave ring 4 (most delayed)
                                _buildWaveRing(_waveAnimation.value, 0.6, 150),
                              ],
                            );
                          },
                        ),

                        // ✅ Main pulsing circle (existing)
                        AnimatedBuilder(
                          animation: _pulseAnimation,
                          builder: (context, child) {
                            return Transform.scale(
                              scale: _pulseAnimation.value,
                              child: Container(
                                width: 180,
                                height: 180,
                                decoration: BoxDecoration(
                                  shape: BoxShape.circle,
                                  gradient: RadialGradient(
                                    colors: [
                                      AppPalette.blue.primary.withOpacity(0.2),
                                      AppPalette.blue.primary.withOpacity(0.1),
                                      AppPalette.blue.primary.withOpacity(0.05),
                                      Colors.transparent,
                                    ],
                                  ),
                                ),
                                child: Center(
                                  child: GestureDetector(
                                    onLongPressStart: (_) =>
                                        _handleLongPressStart(),
                                    onLongPressEnd: (_) =>
                                        _handleLongPressEnd(),
                                    onLongPressCancel: () =>
                                        _longPressStartTime = null,
                                    child: Container(
                                      width: 100,
                                      height: 100,
                                      decoration: BoxDecoration(
                                        shape: BoxShape.circle,
                                        color: AppPalette.blue.primary,
                                        boxShadow: [
                                          BoxShadow(
                                            color: AppPalette.blue.primary
                                                .withOpacity(0.3),
                                            blurRadius: 20,
                                            spreadRadius: 5,
                                          ),
                                        ],
                                      ),
                                      child: const Icon(
                                        Icons.coffee,
                                        color: Colors.white,
                                        size: 40,
                                      ),
                                    ),
                                  ),
                                ),
                              ),
                            );
                          },
                        ),
                      ],
                    ),
                  ),

                  const SizedBox(height: 32),

                  // Status Text
                  Text(
                    'Đơn hàng đang được thực hiện',
                    style: TextStyle(
                      fontSize: 22,
                      fontWeight: FontWeight.bold,
                      color: Colors.grey.shade800,
                    ),
                    textAlign: TextAlign.center,
                  ),

                  const SizedBox(height: 12),

                  Text(
                    'Kiosk đang pha chế đồ uống cho bạn\nVui lòng chờ trong giây lát',
                    style: TextStyle(
                      fontSize: 16,
                      color: Colors.grey.shade600,
                      height: 1.4,
                    ),
                    textAlign: TextAlign.center,
                  ),

                  const SizedBox(height: 32),

                  // Progress Bar
                  Container(
                    width: double.infinity,
                    height: 6,
                    decoration: BoxDecoration(
                      color: Colors.grey.shade200,
                      borderRadius: BorderRadius.circular(3),
                    ),
                    child: AnimatedBuilder(
                      animation: _progressAnimation,
                      builder: (context, child) {
                        return FractionallySizedBox(
                          alignment: Alignment.centerLeft,
                          widthFactor: _progressAnimation.value,
                          child: Container(
                            decoration: BoxDecoration(
                              gradient: LinearGradient(
                                colors: [
                                  AppPalette.blue.primary,
                                  AppPalette.blue.primary.withOpacity(0.8),
                                ],
                              ),
                              borderRadius: BorderRadius.circular(3),
                            ),
                          ),
                        );
                      },
                    ),
                  ),

                  const SizedBox(height: 16),

                  Text(
                    'Đang xử lý...',
                    style: TextStyle(
                      fontSize: 14,
                      color: AppPalette.blue.primary,
                      fontWeight: FontWeight.w500,
                    ),
                  ),

                  const Spacer(flex: 2),

                  // Information Cards
                  Container(
                    padding: const EdgeInsets.all(20),
                    decoration: BoxDecoration(
                      color: Colors.grey.shade50,
                      borderRadius: BorderRadius.circular(16),
                      border: Border.all(color: Colors.grey.shade200),
                    ),
                    child: Column(
                      children: [
                        _buildInfoRow(
                          Icons.notifications_active_outlined,
                          'Thông báo',
                          'Tự động khi hoàn thành',
                          AppPalette.blue.primary,
                        ),
                        const SizedBox(height: 16),
                        _buildInfoRow(
                          Icons.coffee_maker_outlined,
                          'Trạng thái',
                          'Đang pha chế',
                          Colors.green.shade600,
                        ),
                      ],
                    ),
                  ),

                  const SizedBox(height: 32),

                  // Bottom Note
                  Container(
                    padding: const EdgeInsets.all(16),
                    decoration: BoxDecoration(
                      color: AppPalette.blue.primary.withOpacity(0.1),
                      borderRadius: BorderRadius.circular(12),
                      border: Border.all(
                        color: AppPalette.blue.primary.withOpacity(0.2),
                      ),
                    ),
                    child: Row(
                      children: [
                        Icon(
                          Icons.info_outline_rounded,
                          color: AppPalette.blue.primary,
                          size: 20,
                        ),
                        const SizedBox(width: 12),
                        Expanded(
                          child: Text(
                            'Màn hình sẽ được chuyển về menu khi đơn hàng hoàn thành',
                            style: TextStyle(
                              fontSize: 13,
                              color: AppPalette.blue.primary,
                              fontWeight: FontWeight.w500,
                            ),
                          ),
                        ),
                      ],
                    ),
                  ),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }

  Widget _buildInfoRow(IconData icon, String title, String value, Color color) {
    return Row(
      children: [
        Container(
          padding: const EdgeInsets.all(8),
          decoration: BoxDecoration(
            color: color.withOpacity(0.1),
            borderRadius: BorderRadius.circular(8),
          ),
          child: Icon(
            icon,
            color: color,
            size: 18,
          ),
        ),
        const SizedBox(width: 12),
        Expanded(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                title,
                style: TextStyle(
                  fontSize: 13,
                  color: Colors.grey.shade600,
                  fontWeight: FontWeight.w500,
                ),
              ),
              Text(
                value,
                style: TextStyle(
                  fontSize: 14,
                  color: Colors.grey.shade800,
                  fontWeight: FontWeight.w600,
                ),
              ),
            ],
          ),
        ),
      ],
    );
  }
}
