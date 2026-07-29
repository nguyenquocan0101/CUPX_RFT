part of 'product_bloc.dart';

abstract class ProductState extends Equatable {
  const ProductState();
}

final class ProductInitial extends ProductState {
  @override
  List<Object?> get props => [];
}

final class ProductLoading extends ProductState {
  @override
  List<Object?> get props => [];
}

//Get Products Event Done Event
class ProductPaginationLoaded extends ProductState {
  final Pagination<Product> productPagination;

  const ProductPaginationLoaded({
    required this.productPagination,
  });

  @override
  List<Object?> get props => [productPagination];
}

//Get Product Event Done Event
class OneProductLoaded extends ProductState {
  final Product? product;

  const OneProductLoaded({required this.product});

  @override
  List<Object?> get props => [product];
}

class ProductError extends ProductState {
  final String message;

  const ProductError({required this.message});

  @override
  List<Object?> get props => [message];
}
