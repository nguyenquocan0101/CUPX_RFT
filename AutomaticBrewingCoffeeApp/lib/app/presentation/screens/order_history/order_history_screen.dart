import 'package:abc_androidapp/app/core/helpers/price_formatter.dart';
import 'package:abc_androidapp/app/core/helpers/time_formatter.dart';
import 'package:abc_androidapp/app/data/enums/order_status.dart';
import 'package:abc_androidapp/app/data/models/order.dart';
import 'package:abc_androidapp/app/data/models/order_detail.dart';
import 'package:abc_androidapp/app/presentation/blocs/order_history/order_history_bloc.dart';
import 'package:abc_androidapp/app/presentation/screens/order_history/widgets/time_filter.dart';
import 'package:abc_androidapp/app/presentation/widgets/custom_app_bar.dart';
import 'package:abc_androidapp/config/themes/app_color_extension.dart';
import 'package:abc_androidapp/config/themes/app_palette.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:tdesign_flutter/tdesign_flutter.dart';

class OrderHistoryScreen extends StatefulWidget {
  static const String route = "/order-history";
  const OrderHistoryScreen({super.key});

  @override
  State<OrderHistoryScreen> createState() => _OrderHistoryScreenState();
}

class _OrderHistoryScreenState extends State<OrderHistoryScreen> {
  final ValueNotifier<String> _selectedStatus =
      ValueNotifier<String>(OrderStatus.none.value);
  final TextEditingController _searchController = TextEditingController();
  final ScrollController _scrollController = ScrollController();

  int pageIndex = 1;
  final int pageSize = 50;

  late OrderHistoryBloc orderHistoryBloc;
  late DateTime startTime;
  late DateTime endTime;

  String _searchQuery = '';
  List<Order> _allOrder = [];
  List<Order> _orders = [];

  @override
  void initState() {
    super.initState();
    final now = DateTime.now();
    final startTime = DateTime(now.year, now.month, now.day, 0, 0, 0);
    final endTime = DateTime(now.year, now.month, now.day, 23, 59, 59, 999);
    orderHistoryBloc = context.read<OrderHistoryBloc>();
    orderHistoryBloc.add(GetOrderPaginationEvent(
      page: pageIndex,
      size: pageSize,
      startTime: startTime,
      endTime: endTime,
    ));
    _scrollController.addListener(_onScroll);
  }

  Future<void> refresh() async {
    pageIndex = 1;
    orderHistoryBloc
        .add(GetOrderPaginationEvent(page: pageIndex, size: pageSize));
  }

  void _onScroll() {
    if (_scrollController.hasClients) {
      setState(() {}); // Chỉ để rebuild FloatingActionButton
    }
  }

  void search(String value) {
    final query = value.trim().toLowerCase();
    List<Order> filterOrders = _allOrder;
    if (_selectedStatus.value != OrderStatus.none.value) {
      filterOrders = filterOrders
          .where((order) =>
              order.status.name == _selectedStatus.value.toLowerCase())
          .toList();
    }

    // Lọc theo orderId
    var searchedFilterOrders = filterOrders
        .where((x) => x.orderId.toLowerCase().contains(query))
        .toList();

    // Nếu không có kết quả từ orderId, lọc theo productName
    if (filterOrders.isEmpty) {
      filterOrders = searchedFilterOrders.where((order) {
        return order.orderDetails.any(
          (item) => item.productName?.toLowerCase().contains(query) ?? false,
        );
      }).toList();
    }

    setState(() {
      _searchQuery = value;
      _orders = searchedFilterOrders;
    });
  }

  @override
  void dispose() {
    _searchController.dispose();
    _scrollController.dispose();
    _selectedStatus.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: CustomAppBar(
        title: "Lịch sử đặt nước",
        actions: [
          IconButton(
            icon: const Icon(TDIcons.filter, size: 24),
            onPressed: () {
              _showFilterBottomSheet(context);
            },
          ),
        ],
        backgroundColor: AppPalette.white,
      ),
      body: BlocListener<OrderHistoryBloc, OrderHistoryState>(
        listener: (context, state) {
          if (state is OrderPaginationLoaded) {
            _allOrder = state.orderPagiantion.items;
            setState(() {
              _orders = _allOrder;
            });
          }
        },
        child: Column(
          children: [
            Padding(
              padding: const EdgeInsets.all(16.0),
              child: Container(
                decoration: BoxDecoration(
                  color: Colors.white,
                  borderRadius: BorderRadius.circular(12),
                  boxShadow: [
                    BoxShadow(
                      color: Colors.black.withOpacity(0.05),
                      blurRadius: 10,
                      offset: const Offset(0, 2),
                    ),
                  ],
                ),
                child: TextField(
                  controller: _searchController,
                  decoration: InputDecoration(
                    hintText: 'Tìm kiếm đơn hàng...',
                    prefixIcon: const Icon(TDIcons.search, color: Colors.grey),
                    suffixIcon: _searchQuery.isNotEmpty
                        ? IconButton(
                            icon: const Icon(TDIcons.close_circle,
                                color: Colors.grey),
                            onPressed: () {
                              _searchController.clear();
                              _searchQuery = '';
                              search(_searchQuery);
                            },
                          )
                        : null,
                    border: InputBorder.none,
                    contentPadding: const EdgeInsets.symmetric(vertical: 16),
                  ),
                  onChanged: search,
                ),
              ),
            ),
            _buildStatusFilterChips(),
            Expanded(
              child: BlocBuilder<OrderHistoryBloc, OrderHistoryState>(
                builder: (context, state) {
                  if (state is OrderHistoryLoading) {
                    return const Center(
                      child: TDLoading(
                        size: TDLoadingSize.large,
                        text: 'Đang tải dữ liệu...',
                      ),
                    );
                  }

                  if (_orders.isEmpty) {
                    return const Center(
                      child: Column(
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: [
                          Icon(TDIcons.info_circle,
                              size: 48, color: Colors.grey),
                          SizedBox(height: 16),
                          Text(
                            'Không có đơn hàng nào',
                            style: TextStyle(fontSize: 18, color: Colors.grey),
                          ),
                        ],
                      ),
                    );
                  }

                  return RefreshIndicator(
                    onRefresh: refresh,
                    child: ListView.separated(
                      controller: _scrollController,
                      padding: const EdgeInsets.all(16),
                      itemCount: _orders.length,
                      itemBuilder: (context, index) {
                        final order = _orders[index];
                        return _buildOrderCard(order);
                      },
                      separatorBuilder: (context, index) =>
                          const SizedBox(height: 16),
                    ),
                  );
                },
              ),
            ),
          ],
        ),
      ),
      floatingActionButton:
          _scrollController.hasClients && _scrollController.offset > 200
              ? FloatingActionButton(
                  backgroundColor: const Color(0xFF2E6EDF),
                  child: const Icon(TDIcons.filter, color: Colors.white),
                  onPressed: () {
                    _scrollController.animateTo(
                      0,
                      duration: const Duration(milliseconds: 500),
                      curve: Curves.easeInOut,
                    );
                  },
                )
              : null,
    );
  }

  // Widget lọc trạng thái dạng chip
  Widget _buildStatusFilterChips() {
    return ValueListenableBuilder<String>(
      valueListenable: _selectedStatus,
      builder: (context, selectedStatus, _) {
        return SingleChildScrollView(
          scrollDirection: Axis.horizontal,
          child: Row(
            children: [
              _buildStatusChip(OrderStatus.none.value, OrderStatus.none.vnValue,
                  selectedStatus),
              const SizedBox(width: 12),
              _buildStatusChip(OrderStatus.pending.value,
                  OrderStatus.pending.vnValue, selectedStatus),
              const SizedBox(width: 12),
              _buildStatusChip(OrderStatus.preparing.value,
                  OrderStatus.preparing.vnValue, selectedStatus),
              const SizedBox(width: 12),
              _buildStatusChip(OrderStatus.completed.value,
                  OrderStatus.completed.vnValue, selectedStatus),
              const SizedBox(width: 12),
              _buildStatusChip(OrderStatus.cancelled.value,
                  OrderStatus.cancelled.vnValue, selectedStatus),
              const SizedBox(width: 12),
              _buildStatusChip(OrderStatus.failed.value,
                  OrderStatus.failed.vnValue, selectedStatus),
            ],
          ),
        );
      },
    );
  }

  // Chip trạng thái đơn hàng
  Widget _buildStatusChip(String value, String label, String selectedValue) {
    final bool isSelected = value == selectedValue;

    return Material(
      color: Colors.transparent,
      child: InkWell(
        onTap: () {
          _selectedStatus.value = value;
          search(_searchQuery);
        },
        borderRadius: BorderRadius.circular(16),
        child: AnimatedContainer(
          duration: const Duration(milliseconds: 200),
          padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 12),
          decoration: BoxDecoration(
            color: isSelected ? AppPalette.blue.primary : AppPalette.white,
            borderRadius: BorderRadius.circular(16),
            border: Border.all(
              color:
                  isSelected ? AppPalette.blue.primary : AppPalette.grey.grey2,
              width: 1.5,
            ),
            boxShadow: isSelected
                ? [
                    BoxShadow(
                      color: AppPalette.blue.primary.withOpacity(0.15),
                      blurRadius: 12,
                      offset: const Offset(0, 4),
                    )
                  ]
                : null,
          ),
          child: Text(
            label,
            style: TextStyle(
              color: isSelected ? AppPalette.white : AppPalette.grey.grey2,
              fontSize: 15,
              fontWeight: isSelected ? FontWeight.w600 : FontWeight.w500,
              letterSpacing: -0.3,
            ),
          ),
        ),
      ),
    );
  }

  Widget _buildOrderCard(Order order) {
    return Card(
      elevation: 0,
      color: AppPalette.white,
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(20),
      ),
      margin: EdgeInsets.zero,
      child: Container(
        decoration: BoxDecoration(
          borderRadius: BorderRadius.circular(20),
          border: Border.all(color: Colors.grey[100]!),
          boxShadow: [
            BoxShadow(
              color: AppPalette.black.withOpacity(0.04),
              blurRadius: 12,
              offset: const Offset(0, 4),
            ),
          ],
        ),
        child: InkWell(
          onTap: () => _showOrderDetails(order),
          borderRadius: BorderRadius.circular(20),
          child: Padding(
            padding: const EdgeInsets.all(24),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    Container(
                      padding: const EdgeInsets.symmetric(
                          horizontal: 16, vertical: 8),
                      decoration: BoxDecoration(
                        color: AppPalette.blue.primary.withOpacity(0.1),
                        borderRadius: BorderRadius.circular(12),
                      ),
                      child: Text(
                        order.orderId,
                        style: TextStyle(
                          color: AppPalette.blue.primary,
                          fontSize: 18,
                          fontWeight: FontWeight.w600,
                          letterSpacing: -0.5,
                        ),
                      ),
                    ),
                    _buildStatusBadge(order.status.value),
                  ],
                ),
                const SizedBox(height: 20),
                Row(
                  children: [
                    Icon(
                      TDIcons.time,
                      size: 22,
                      color: AppPalette.grey.grey3,
                    ),
                    const SizedBox(width: 12),
                    Text(
                      formatTime(order.createdAt!.toIso8601String()),
                      style: TextStyle(
                        color: AppPalette.grey.grey3,
                        fontSize: 16,
                        fontWeight: FontWeight.w500,
                      ),
                    ),
                  ],
                ),
                Padding(
                  padding: const EdgeInsets.symmetric(vertical: 20),
                  child: Divider(
                    height: 1,
                    color: Colors.grey[200],
                  ),
                ),
                ...order.orderDetails.take(2).map(
                      (item) => Padding(
                        padding: const EdgeInsets.only(bottom: 16),
                        child: _buildOrderItemPreview(item),
                      ),
                    ),
                if (order.orderDetails.length > 2)
                  Padding(
                    padding: const EdgeInsets.only(bottom: 16),
                    child: Text(
                      '+ ${order.orderDetails.length - 2} món khác',
                      style: TextStyle(
                        color: Colors.grey[500],
                        fontSize: 15,
                        fontWeight: FontWeight.w500,
                      ),
                    ),
                  ),
                Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    Text(
                      'Tổng cộng:',
                      style: TextStyle(
                        fontSize: 17,
                        fontWeight: FontWeight.w600,
                        color: Colors.grey[800],
                      ),
                    ),
                    Column(
                      crossAxisAlignment: CrossAxisAlignment.end,
                      children: [
                        if (order.orderData != null &&
                            order.orderData!.totalAmount != null &&
                            order.orderData!.discount != null)
                          Text(
                            formatPrice(order.orderData?.totalAmount ?? 0.0),
                            style: TextStyle(
                              fontSize: 15,
                              decoration: TextDecoration.lineThrough,
                              color: Colors.grey[500],
                            ),
                          ),
                        const SizedBox(height: 4),
                        Text(
                          formatPrice(order.orderData?.totalAmount ?? 0.0),
                          style: TextStyle(
                            fontSize: 20,
                            fontWeight: FontWeight.w600,
                            color: AppPalette.blue.primary,
                            letterSpacing: -0.5,
                          ),
                        ),
                      ],
                    ),
                  ],
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }

  Widget _buildOrderItemPreview(OrderDetail item) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 8),
      child: Row(
        children: [
          ClipRRect(
            borderRadius: BorderRadius.circular(12),
            child: Image.network(
              // item.imageUrl,
              "",
              width: 70,
              height: 70,
              fit: BoxFit.cover,
              errorBuilder: (context, error, stackTrace) {
                return Container(
                  width: 70,
                  height: 70,
                  color: Colors.grey[300],
                  child:
                      const Icon(TDIcons.image, color: Colors.grey, size: 24),
                );
              },
            ),
          ),
          const SizedBox(width: 16),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  item.productName!,
                  style: Theme.of(context).textTheme.bodyLarge,
                ),
                const SizedBox(height: 6),
                // Text(
                //   item.options.entries
                //       .map((e) => '${e.key}: ${e.value}')
                //       .join(', '),
                //   style: Theme.of(context).textTheme.bodyMedium,
                //   maxLines: 1,
                //   overflow: TextOverflow.ellipsis,
                // ),
              ],
            ),
          ),
          const SizedBox(width: 12),
          Column(
            crossAxisAlignment: CrossAxisAlignment.end,
            children: [
              Text(
                'x${item.quantity}',
                style: const TextStyle(
                  fontWeight: FontWeight.bold,
                  fontSize: 16,
                ),
              ),
              const SizedBox(height: 6),
              Text(
                formatPrice(item.sellingPrice),
                style: TextStyle(
                  fontSize: 15,
                  color: Theme.of(context)
                      .extension<AppColorsExtension>()!
                      .primary,
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }

  void _showOrderDetails(Order order) {
    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      backgroundColor: Colors.transparent,
      builder: (context) => DraggableScrollableSheet(
        initialChildSize: 0.8, // Tăng kích thước ban đầu
        maxChildSize: 0.95,
        minChildSize: 0.5,
        builder: (_, controller) => Container(
          decoration: const BoxDecoration(
            color: Colors.white,
            borderRadius: BorderRadius.vertical(top: Radius.circular(24)),
          ),
          padding: const EdgeInsets.all(24),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Center(
                child: Container(
                  width: 60,
                  height: 6,
                  decoration: BoxDecoration(
                    color: Colors.grey[300],
                    borderRadius: BorderRadius.circular(10),
                  ),
                ),
              ),
              const SizedBox(height: 24),
              Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  Expanded(
                    child: Text(
                      'Chi tiết đơn ${order.orderId}',
                      style: const TextStyle(
                        fontSize: 22,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                  ),
                  _buildStatusBadge(order.status.value),
                ],
              ),
              const SizedBox(height: 16),
              Row(
                children: [
                  const Icon(
                    TDIcons.time,
                    size: 20,
                    color: Colors.grey,
                  ),
                  const SizedBox(width: 8),
                  Text(
                    formatTime(order.createdAt!.toIso8601String()),
                    style: const TextStyle(
                      color: Colors.grey,
                      fontSize: 16,
                    ),
                  ),
                ],
              ),
              const SizedBox(height: 40),

              // Hiển thị trạng thái đơn hàng
              _buildOrderStatusTracker(order.status.value),

              const SizedBox(height: 24),
              const Divider(height: 1),
              Expanded(
                child: ListView.separated(
                  controller: controller,
                  itemCount: order.orderDetails.length,
                  separatorBuilder: (context, index) =>
                      const Divider(height: 32),
                  itemBuilder: (context, index) {
                    final item = order.orderDetails[index];
                    return _buildOrderItemDetail(item);
                  },
                ),
              ),
              const Divider(height: 32),

              // Hiển thị thông tin thanh toán
              if (order.orderData != null) ...[
                Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    const Text(
                      'Tạm tính:',
                      style: TextStyle(
                        color: Colors.grey,
                        fontSize: 16,
                      ),
                    ),
                    Text(
                      formatPrice(order.orderData?.totalAmount ?? 0),
                      style: const TextStyle(
                        color: Colors.grey,
                        fontSize: 16,
                      ),
                    ),
                  ],
                ),
                const SizedBox(height: 12),
                Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    const Text(
                      'Giảm giá:',
                      style: TextStyle(
                        color: Colors.red,
                        fontSize: 16,
                      ),
                    ),
                    Text(
                      '-${formatPrice(order.orderData?.discount ?? 0.0)}',
                      style: const TextStyle(
                        color: Colors.red,
                        fontSize: 16,
                      ),
                    ),
                  ],
                ),
                const SizedBox(height: 12),
              ],

              Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  const Text(
                    'Tổng cộng:',
                    style: TextStyle(
                      fontSize: 20,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                  Text(
                    formatPrice(order.orderData?.finalAmount ?? 0.0),
                    style: const TextStyle(
                      fontSize: 20,
                      fontWeight: FontWeight.bold,
                      color: Color(0xFF2E6EDF),
                    ),
                  ),
                ],
              ),
              const SizedBox(height: 24),
              if (order.status == 'waiting')
                SizedBox(
                  width: double.infinity,
                  height: 56,
                  child: ElevatedButton.icon(
                    onPressed: () {
                      Navigator.pop(context); // Đóng bottom sheet
                      showDialog(
                        context: context,
                        builder: (context) => AlertDialog(
                          title: const Text('Xác nhận hủy'),
                          content: const Text(
                              'Bạn có chắc chắn muốn hủy đơn hàng này không?'),
                          actions: [
                            TextButton(
                              onPressed: () {
                                Navigator.pop(context); // Đóng dialog
                              },
                              child: const Text('Quay lại'),
                            ),
                            TextButton(
                              onPressed: () {
                                Navigator.pop(context);
                                TDToast.showSuccess(
                                  'Đã hủy đơn hàng ${order.orderId}',
                                  context: context,
                                );
                              },
                              child: const Text('Hủy đơn'),
                            ),
                          ],
                        ),
                      );
                    },
                    icon: const Icon(TDIcons.close_circle),
                    label: const Text(
                      'Hủy đơn hàng',
                      style: TextStyle(fontSize: 18),
                    ),
                    style: ElevatedButton.styleFrom(
                      backgroundColor: Colors.red,
                      foregroundColor: Colors.white,
                      shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(12),
                      ),
                    ),
                  ),
                ),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildOrderStatusTracker(String status) {
    final List<Map<String, dynamic>> steps = [
      {'label': 'Chờ Xác Nhận', 'completed': true},
      {'label': 'Đang xử lý', 'completed': status != 'waiting'},
      {'label': 'Hoàn thành', 'completed': status == 'completed'},
      {'label': 'Đã nhận món', 'completed': false},
    ];

    return Column(
      children: [
        Row(
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          children: steps.map((step) {
            return Expanded(
              child: Column(
                children: [
                  Container(
                    width: 36,
                    height: 36,
                    decoration: BoxDecoration(
                      color: step['completed']
                          ? const Color(0xFF2E6EDF)
                          : Colors.grey[300],
                      shape: BoxShape.circle,
                    ),
                    child: Center(
                      child: step['completed']
                          ? const Icon(TDIcons.check,
                              color: Colors.white, size: 20)
                          : null,
                    ),
                  ),
                  const SizedBox(height: 8),
                  Text(
                    step['label'],
                    style: TextStyle(
                      fontSize: 14,
                      color: step['completed']
                          ? const Color(0xFF2E6EDF)
                          : Colors.grey,
                      fontWeight: step['completed']
                          ? FontWeight.bold
                          : FontWeight.normal,
                    ),
                    textAlign: TextAlign.center,
                  ),
                ],
              ),
            );
          }).toList(),
        ),
        const SizedBox(height: 12),
        Stack(
          children: [
            Container(
              height: 6,
              decoration: BoxDecoration(
                color: Colors.grey[300],
                borderRadius: BorderRadius.circular(3),
              ),
            ),
            Container(
              height: 6,
              width: status == 'waiting'
                  ? MediaQuery.of(context).size.width * 0.2
                  : (status == 'processing'
                      ? MediaQuery.of(context).size.width * 0.5
                      : MediaQuery.of(context).size.width * 0.75),
              decoration: BoxDecoration(
                color: const Color(0xFF2E6EDF),
                borderRadius: BorderRadius.circular(3),
              ),
            ),
          ],
        ),
      ],
    );
  }

  Widget _buildStatusBadge(String statusStr) {
    OrderStatus status;
    try {
      status = OrderStatusExtension.fromValueStr(statusStr);
    } catch (_) {
      status = OrderStatus.none;
    }

    Color backgroundColor;
    Color textColor;
    String text;
    IconData icon;

    switch (status) {
      case OrderStatus.pending:
        backgroundColor = AppPalette.white;
        textColor = Colors.orange;
        text = OrderStatus.pending.vnValue;
        icon = TDIcons.time;
        break;
      case OrderStatus.preparing:
        backgroundColor = AppPalette.white;
        textColor = Colors.blue;
        text = OrderStatus.preparing.vnValue;
        icon = TDIcons.play_circle;
        break;
      case OrderStatus.completed:
        backgroundColor = AppPalette.white;
        textColor = Colors.green;
        text = OrderStatus.completed.vnValue;
        icon = TDIcons.check_circle;
        break;
      case OrderStatus.cancelled:
        backgroundColor = AppPalette.white;
        textColor = Colors.red.withOpacity(0.5);
        text = OrderStatus.cancelled.vnValue;
        icon = TDIcons.check_circle;
        break;
      case OrderStatus.failed:
        backgroundColor = Colors.grey.shade200;
        textColor = Colors.grey;
        text = OrderStatus.failed.vnValue;
        icon = TDIcons.help_circle;
        break;
      default:
        backgroundColor = Colors.grey.shade200;
        textColor = Colors.grey;
        text = 'Không xác định';
        icon = TDIcons.help_circle;
    }

    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
      decoration: BoxDecoration(
        color: backgroundColor,
        borderRadius: BorderRadius.circular(12),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(icon, color: textColor, size: 16),
          const SizedBox(width: 4),
          Text(
            text,
            style: TextStyle(
              color: textColor,
              fontWeight: FontWeight.bold,
              fontSize: 12,
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildOrderItemDetail(OrderDetail item) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 12),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          ClipRRect(
            borderRadius: BorderRadius.circular(16),
            child: Image.network(
              // item.imageUrl,
              "",
              width: 100,
              height: 100,
              fit: BoxFit.cover,
              errorBuilder: (context, error, stackTrace) {
                return Container(
                  width: 100,
                  height: 100,
                  color: Colors.grey[300],
                  child:
                      const Icon(TDIcons.image, color: Colors.grey, size: 36),
                );
              },
            ),
          ),
          const SizedBox(width: 20),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  item.productName!,
                  style: Theme.of(context).textTheme.bodyLarge,
                ),
                const SizedBox(height: 12),
                // ...item.options.entries.map((entry) => Padding(
                //       padding: const EdgeInsets.only(bottom: 8),
                //       child: Row(
                //         children: [
                //           Text(
                //             '${entry.key}:',
                //             style: TextStyle(
                //               color: Colors.grey[600],
                //               fontSize: 16,
                //             ),
                //           ),
                //           const SizedBox(width: 6),
                //           Text(
                //             entry.value,
                //             style: const TextStyle(
                //               fontWeight: FontWeight.w500,
                //               fontSize: 16,
                //             ),
                //           ),
                //         ],
                //       ),
                //     )),
              ],
            ),
          ),
          Column(
            crossAxisAlignment: CrossAxisAlignment.end,
            children: [
              Text(
                'x${item.quantity}',
                style: const TextStyle(
                  fontWeight: FontWeight.bold,
                  fontSize: 18,
                ),
              ),
              const SizedBox(height: 12),
              Text(
                formatPrice(item.sellingPrice),
                style: TextStyle(
                  fontWeight: FontWeight.bold,
                  color: AppPalette.blue.blue4,
                  fontSize: 16,
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }

  void _showFilterBottomSheet(BuildContext context) {
    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(32)),
      ),
      builder: (context) {
        return TimeFilter(
          pageIndex: pageIndex,
          pageSize: pageSize,
          onApply: (start, end) {
            orderHistoryBloc.add(GetOrderPaginationEvent(
              page: pageIndex,
              size: pageSize,
              startTime: start,
              endTime: end,
            ));
          },
          onReset: () {
            final now = DateTime.now();
            final startTime = DateTime(now.year, now.month, now.day, 0, 0, 0);
            final endTime =
                DateTime(now.year, now.month, now.day, 23, 59, 59, 999);
            orderHistoryBloc.add(GetOrderPaginationEvent(
              page: pageIndex,
              size: pageSize,
              startTime: startTime,
              endTime: endTime,
            ));
          },
        );
      },
    );
  }
}
