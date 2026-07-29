import 'package:abc_androidapp/app/core/base_models/base_pagination.dart';
import 'package:abc_androidapp/app/data/models/product/product.dart';
import 'package:abc_androidapp/app/domain/usecases/product/get_product_usecase.dart';
import 'package:abc_androidapp/app/domain/usecases/product/get_selling_products_usecase.dart';
import 'package:bloc/bloc.dart';
import 'package:equatable/equatable.dart';
import 'package:meta/meta.dart';

part 'product_event.dart';
part 'product_state.dart';

class ProductBloc extends Bloc<ProductEvent, ProductState> {
  final GetProductsUseCase getProductsUseCase;
  final GetProductUseCase getProductUseCase;

  ProductBloc({
    required this.getProductsUseCase,
    required this.getProductUseCase,
  }) : super(ProductInitial()) {
    on<ProductEvent>((event, emit) {
      // TODO: implement event handler
    });
    on<GetProductDetailEvent>(_onGetProductDetail);
  }

  Future<void> _onGetProducts(
    GetProductsEvent event,
    Emitter<ProductState> emit,
  ) async {
    emit(ProductLoading());
    final response = await getProductsUseCase.execute(
      event.search,
      "Name",
      event.productStatus,
      event.productSize,
      event.productType,
      event.sortBy,
      event.isAsc,
      pageIndex: event.page,
      pageSize: event.size,
    );
    response.fold(
      (failure) => emit(ProductError(message: failure.message)),
      (data) => emit(ProductPaginationLoaded(
        productPagination: data,
      )),
    );
  }

  Future<void> _onGetProductDetail(
    GetProductDetailEvent event,
    Emitter<ProductState> emit,
  ) async {
    emit(ProductLoading());
    final response = await getProductUseCase.execute(event.productId);
    response.fold(
      (failure) => emit(ProductError(message: failure.message)),
      (data) => emit(OneProductLoaded(product: data)),
    );
  }
}
