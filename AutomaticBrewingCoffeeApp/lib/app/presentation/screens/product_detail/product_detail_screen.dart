import 'package:abc_androidapp/app/core/helpers/price_formatter.dart';
import 'package:abc_androidapp/app/data/local_models/cart_item.dart';
import 'package:abc_androidapp/app/data/models/product/product.dart';
import 'package:abc_androidapp/app/data/models/product/product_attribute.dart';
import 'package:abc_androidapp/app/data/models/product/product_attribute_selection.dart';
import 'package:abc_androidapp/app/presentation/blocs/product/product_bloc.dart';
import 'package:abc_androidapp/app/presentation/cubits/cart/cart_cubit.dart';
import 'package:abc_androidapp/app/presentation/widgets/common/hint_dialog.dart';
import 'package:abc_androidapp/app/presentation/widgets/common/loading.dart';
import 'package:abc_androidapp/app/presentation/widgets/common/product_load.dart';
import 'package:abc_androidapp/app/presentation/widgets/common/quantity_selector.dart';
import 'package:abc_androidapp/app/presentation/widgets/custom_app_bar.dart';
import 'package:abc_androidapp/app/presentation/widgets/product_option_selector.dart';
import 'package:abc_androidapp/config/constants/animation_path.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:get/get.dart';
import 'package:get/get_core/src/get_main.dart';
import 'package:lottie/lottie.dart';

class ProductDetailScreen extends StatefulWidget {
  static const String route = "/product-detail";
  final Product product;

  const ProductDetailScreen({Key? key, required this.product})
      : super(key: key);

  @override
  State<ProductDetailScreen> createState() => _ProductDetailScreenState();
}

class _ProductDetailScreenState extends State<ProductDetailScreen> {
  int quantity = 1;
  Map<String, AttributeOption> selectedOptions = {};

  late CartCubit cart;
  final Color primaryColor = const Color(0xFF57B7E7);

  @override
  void initState() {
    super.initState();

    cart = context.read<CartCubit>();
  }

  void _initializeDefaultOptions(List<ProductAttribute> attributes) {
    selectedOptions.clear();
    attributes.sort((a, b) => a.displayOrder.compareTo(b.displayOrder));
    for (var attribute in attributes) {
      if (attribute.attributeOptions != null &&
          attribute.attributeOptions!.isNotEmpty) {
        final options = attribute.attributeOptions!;
        final int defaultIndex =
            options.indexWhere((opt) => opt.isDefault == true);
        final selectedOption = options[defaultIndex != -1 ? defaultIndex : 0];

        selectedOptions[attribute.productAttributeId] = selectedOption;
      }
    }
  }

  void onAddToCart(Product p, int quantity) {
    final selectedAttributes = selectedOptions.entries
        .map((e) => ProductAttributeSelection(
              productAttributeId: e.key,
              attributeOptionId: e.value.attributeOptionId,
            ))
        .toList();
    final optionDescriptions = selectedOptions.entries.map((entry) {
      final option = entry.value;
      return '${option.name}';
    }).join(', ');

    var item = CartItem(
        id: p.productId,
        name: p.name,
        price: p.price,
        description: optionDescriptions.isNotEmpty
            ? optionDescriptions
            : '',
        picUrl: p.imageUrl ?? '',
        quantity: quantity,
        selectedAttributes: selectedAttributes);
    final alreadyInCart = cart.state.items.containsKey(item.uniqueKey);
    cart.addItem(item);
    //Get back
    Get.back();
    if (!alreadyInCart) {
      HintDialog.show(
        context,
        'Đã thêm vào giỏ hàng!',
        'Bạn có thể xem lại trong giỏ hàng.',
      );
    }
  }

  void _onOptionSelected(String attributeId, AttributeOption option) {
    setState(() {
      selectedOptions[attributeId] = option;
    });
  }

  List<ProductOptionSelector> _buildProductOptionSelectors(
      List<ProductAttribute> attributes) {
    return attributes
        .map((attribute) {
          if (attribute.attributeOptions == null ||
              attribute.attributeOptions!.isEmpty) {
            return [];
          }

          //sort
          attribute.attributeOptions!
              .sort((a, b) => a.displayOrder.compareTo(b.displayOrder));

          final optionNames =
              attribute.attributeOptions!.map((option) => option.name).toList();

          final defaultOption = attribute.attributeOptions!
              .firstWhere((option) => option.name == selectedOptions[attribute.productAttributeId]?.name,
                  orElse: () => attribute.attributeOptions!.first);
          return ProductOptionSelector(
            title: attribute.label,
            items: optionNames,
            onSelect: (selectedName) {
              // Find the corresponding AttributeOption
              final selectedOption = attribute.attributeOptions!
                  .firstWhere((option) => option.name == selectedName);

              _onOptionSelected(attribute.productAttributeId, selectedOption);
            },
            selectedIndex: attribute.attributeOptions!.indexOf(attribute.attributeOptions!
                .firstWhere((option) => option.name == selectedOptions[attribute.productAttributeId]?.name,
                    orElse: () => defaultOption)),
          );
        })
        .where((widget) => widget is ProductOptionSelector)
        .cast<ProductOptionSelector>()
        .toList();
  }

  @override
  Widget build(BuildContext context) {
    return BlocBuilder<ProductBloc, ProductState>(
      builder: (context, state) {
        final product = widget.product!;
          final totalPrice = product.price * quantity;

          final attributes = product.getAttributes();

          if (selectedOptions.isEmpty && attributes.isNotEmpty) {
            _initializeDefaultOptions(attributes);
          }

          final optionSelectors = _buildProductOptionSelectors(attributes);

          return Scaffold(
            backgroundColor: Colors.white,
            appBar: const CustomAppBar(
              title: "",
            ),
            body: Container(
              padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 16),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      ImageLoad(imageUrl: product.imageUrl, size: 150),
                      const SizedBox(width: 12),
                      Expanded(
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text(
                              product.name,
                              style: Theme.of(context).textTheme.titleLarge,
                              softWrap: true,
                            ),
                            const SizedBox(height: 12),
                            if (product.description != null &&
                                product.description!.isNotEmpty)
                              Container(
                                margin: const EdgeInsets.only(bottom: 24),
                                child: Text(
                                  product.description!,
                                  style: Theme.of(context).textTheme.bodyLarge,
                                  softWrap: true,
                                ),
                              ),
                          ],
                        ),
                      ),
                    ],
                  ),

                  const SizedBox(height: 16),

                  // Quantity Selector with Price
                  Container(
                    padding: const EdgeInsets.all(24),
                    decoration: BoxDecoration(
                      color: Colors.grey[50],
                      borderRadius: BorderRadius.circular(16),
                    ),
                    child: Row(
                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                      children: [
                        Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text(
                              'Giá tiền',
                              style: TextStyle(
                                fontSize: 16,
                                fontWeight: FontWeight.w500,
                                color: Colors.black.withOpacity(0.6),
                              ),
                            ),
                            const SizedBox(height: 4),
                            Text(
                              formatPrice(product.price),
                              style: TextStyle(
                                fontSize: 20,
                                fontWeight: FontWeight.w600,
                                color: primaryColor,
                              ),
                            ),
                          ],
                        ),
                        QuantitySelector(
                          quantity: quantity,
                          onDecrease: () {
                            if (quantity > 1) {
                              setState(() {
                                quantity--;
                              });
                            }
                          },
                          onIncrease: () {
                            setState(() {
                              quantity++;
                            });
                          },
                        ),
                      ],
                    ),
                  ),

                  const SizedBox(height: 32),

                  // Options
                  if (optionSelectors.isNotEmpty)
                    Expanded(
                      child: SingleChildScrollView(
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            ...optionSelectors
                                .map((selector) => Padding(
                                      padding:
                                          const EdgeInsets.only(bottom: 24),
                                      child: selector,
                                    ))
                                .toList(),
                          ],
                        ),
                      ),
                    )
                  else
                    const Expanded(
                      child: Center(
                        child: Text(
                          'Không có tùy chọn nào cho sản phẩm này',
                          style: TextStyle(
                            fontSize: 16,
                            color: Colors.grey,
                          ),
                        ),
                      ),
                    ),

                  const SizedBox(height: 24),

                  // Add to Cart Button
                  SizedBox(
                    width: double.infinity,
                    height: 56,
                    child: ElevatedButton(
                      onPressed: () {
                        onAddToCart(product, quantity);
                      },
                      style: ElevatedButton.styleFrom(
                        backgroundColor: primaryColor,
                        foregroundColor: Colors.white,
                        elevation: 0,
                        shape: RoundedRectangleBorder(
                          borderRadius: BorderRadius.circular(8),
                        ),
                      ),
                      child: Row(
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: [
                          const Icon(Icons.shopping_bag_outlined),
                          const SizedBox(width: 12),
                          Text(
                            'Thêm vào giỏ - ${formatPrice(totalPrice)}',
                            style: const TextStyle(
                              fontSize: 18,
                              fontWeight: FontWeight.w600,
                              letterSpacing: 0.2,
                            ),
                          ),
                        ],
                      ),
                    ),
                  ),
                ],
              ),
            ),
          );

      },
    );
  }
}
