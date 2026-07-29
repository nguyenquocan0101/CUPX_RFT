part of 'cart_cubit.dart';

class CartState {
  final Map<String, CartItem> items;

  CartState({required this.items});

  double get totalAmount =>
      items.values.fold(0, (sum, item) => sum + item.total);


  int get itemCount => items.length;

  int get itemQuantity =>
      items.values.fold(0, (sum, item) => sum + item.quantity);

  List<CartItem> get itemList => items.values.toList();

  CartState copyWith({Map<String, CartItem>? items}) {
    return CartState(items: items ?? this.items);
  }
}
