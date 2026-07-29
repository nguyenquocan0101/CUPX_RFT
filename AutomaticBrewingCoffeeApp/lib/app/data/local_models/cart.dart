import 'package:abc_androidapp/app/data/local_models/cart_item.dart';

class Cart {
  final Map<String, CartItem> _items = {};

  List<CartItem> get items => _items.values.toList();

  int get itemCount => _items.length;

  double get totalAmount =>
      _items.values.fold(0, (sum, item) => sum + item.total);

  void addItem(CartItem item) {
    if (_items.containsKey(item.id)) {
      _items[item.id]!.quantity += item.quantity;
    } else {
      _items[item.id] = item;
    }
  }

  void removeItem(String uniqueKey) {
    _items.remove(uniqueKey);
  }

  void clear() {
    _items.clear();
  }

  void updateQuantity(String uniqueKey, int quantity) {
    if (_items.containsKey(uniqueKey)) {
      _items[uniqueKey]!.quantity = quantity;
    }
  }
}
