import 'package:abc_androidapp/app/data/local_models/cart_item.dart';
import 'package:bloc/bloc.dart';

part 'cart_state.dart';

class CartCubit extends Cubit<CartState> {
  CartCubit() : super(CartState(items: {}));

  void addItem(CartItem item) {
    final newItems = Map<String, CartItem>.from(state.items);
    final uniqueKey = item.uniqueKey;
    
    if (newItems.containsKey(uniqueKey)) {
      final existingItem = newItems[uniqueKey]!;
      newItems[uniqueKey] = existingItem.copyWith(
        quantity: existingItem.quantity + item.quantity
      );
    } else {
      newItems[uniqueKey] = item;
    }
    
    emit(state.copyWith(items: newItems));
  }

    void removeItem(String uniqueKey) {
    final newItems = Map<String, CartItem>.from(state.items);
    newItems.remove(uniqueKey);
    emit(state.copyWith(items: newItems));
  }

  void updateQuantity(String uniqueKey, int quantity) {
    final newItems = Map<String, CartItem>.from(state.items);
    if (quantity <= 0) {
      newItems.remove(uniqueKey);
    } else if (newItems.containsKey(uniqueKey)) {
      newItems[uniqueKey] = newItems[uniqueKey]!.copyWith(quantity: quantity);
    }
    emit(state.copyWith(items: newItems));
  }

  void clear() {
    emit(CartState(items: {}));
  }
}
