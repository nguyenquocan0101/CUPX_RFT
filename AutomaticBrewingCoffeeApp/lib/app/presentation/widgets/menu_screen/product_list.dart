// import 'package:abc_androidapp/app/core/router/app_router.dart';
// import 'package:abc_androidapp/app/data/models/product/product.dart';
// import 'package:abc_androidapp/app/presentation/blocs/product/product_bloc.dart';
// import 'package:abc_androidapp/app/presentation/screens/menu/widgets/product_card.dart';
// import 'package:abc_androidapp/app/presentation/widgets/section.dart';
// import 'package:flutter/material.dart';
// import 'package:flutter/widgets.dart';
// import 'package:flutter_bloc/flutter_bloc.dart';

// class ProductList extends StatefulWidget {
//   ProductList({super.key});

//   @override
//   State<ProductList> createState() => _ProductListState();
// }

// class _ProductListState extends State<ProductList> {
//   final ScrollController _scrollController = ScrollController();

//   @override
//   Widget build(BuildContext context) {
//     return BlocBuilder<ProductBloc, ProductState>(
//       builder: (context, state) {
//         if (state is ProductLoading) {
//           return const Center(child: CircularProgressIndicator());
//         } else if (state is ProductError) {
//           return Center(child: Text('Error: ${state.message}'));
//         } else if (state is ProductPaginationLoaded) {
//           final products = state.productPagination.items;

//           // Nhóm sản phẩm theo category (ở đây dùng productParentName làm category)
//           final groupedByCategory = <String, List<Product>>{};
//           for (final product in products) {
//             final category = 'Uncategorized';
//             groupedByCategory.putIfAbsent(category, () => []).add(product);
//           }

//           return Expanded(
//             child: Padding(
//               padding: const EdgeInsets.only(right: 8.0),
//               child: CustomScrollView(
//                 controller: _scrollController,
//                 physics: const BouncingScrollPhysics(),
//                 slivers: [
//                   SliverList(
//                     delegate: SliverChildBuilderDelegate(
//                       (context, index) {
//                         final entry = groupedByCategory.entries.toList()[index];
//                         final categoryId = entry.key;
//                         final productsInCategory = entry.value;
//                         return Section(
//                           title: categoryId,
//                           detail: Column(
//                             children: productsInCategory.map((product) {
//                               return ProductCard(
//                                 product: product,
//                                 onTap: () {
//                                   AppRouter.navigateToProductDetailScreen(
//                                       product.productId);
//                                 },
//                               );
//                             }).toList(),
//                           ),
//                         );
//                       },
//                       childCount: groupedByCategory.length,
//                     ),
//                   ),
//                 ],
//               ),
//             ),
//           );
//         }

//         return const SizedBox(); 
//       },
//     );
//   }
// }
