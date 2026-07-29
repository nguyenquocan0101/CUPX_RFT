
enum OrderStatus {
  pending,
  preparing,
  completed,
  cancelled,
  failed,
  //for all
  none
}

extension OrderStatusExtension on OrderStatus {
  String get value {
    switch (this) {
      case OrderStatus.pending:
        return 'Pending';
      case OrderStatus.preparing:
        return 'Preparing';
      case OrderStatus.completed:
        return 'Completed';
      case OrderStatus.cancelled:
        return 'Cancelled';
      case OrderStatus.failed:
        return 'Failed';
      case OrderStatus.none:
        return 'All';
    }
  }

  String get vnValue {
    switch (this) {
      case OrderStatus.pending:
        return 'Đang đợi';
      case OrderStatus.preparing:
        return 'Đang chuẩn bị';
      case OrderStatus.completed:
        return 'Hoàn thành';
      case OrderStatus.cancelled:
        return 'Hủy';
      case OrderStatus.failed:
        return 'Thất bại';
      case OrderStatus.none:
        return 'Tất cả';
    }
  }

  static List<String> get allValues =>
      OrderStatus.values.map((e) => e.value).toList();

  static OrderStatus fromValueStr(String value) {
    return OrderStatus.values.firstWhere((e) => e.value == value);
  }
}
