part of 'product_bloc.dart';

@immutable
abstract class ProductEvent extends Equatable {
  const ProductEvent();
}

class GetProductsEvent extends ProductEvent {
  final String? search;
  final String? sortBy;
  final bool isAsc;
  final String? productStatus;
  final String? productSize;
  final String? productType;
  final int page;
  final int size;

  const GetProductsEvent(
      this.isAsc, this.productStatus, this.productSize, this.productType,
      {this.search, this.sortBy, required this.page, required this.size});

  @override
  List<Object?> get props => [search, sortBy, isAsc, page, size];
}

class GetProductDetailEvent extends ProductEvent {
  final String productId;

  const GetProductDetailEvent({required this.productId});

  @override
  List<Object?> get props => [productId];
}
