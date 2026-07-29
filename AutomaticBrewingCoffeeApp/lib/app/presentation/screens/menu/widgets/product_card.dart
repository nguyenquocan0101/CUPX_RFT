import 'package:abc_androidapp/app/core/helpers/price_formatter.dart';
import 'package:abc_androidapp/app/data/local_models/cart_item.dart';
import 'package:abc_androidapp/app/data/models/product/product.dart';
import 'package:abc_androidapp/app/data/models/product/product_attribute.dart';
import 'package:abc_androidapp/app/data/models/product/product_attribute_selection.dart';
import 'package:abc_androidapp/app/presentation/cubits/cart/cart_cubit.dart';
import 'package:abc_androidapp/app/presentation/widgets/common/hint_dialog.dart';
import 'package:abc_androidapp/app/presentation/widgets/common/product_load.dart';
import 'package:abc_androidapp/config/constants/animation_path.dart';
import 'package:abc_androidapp/config/constants/image_path.dart';
import 'package:abc_androidapp/config/themes/app_palette.dart';
import 'package:abc_androidapp/config/themes/app_typography.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:lottie/lottie.dart';

class ProductCard extends StatelessWidget {
  final Product product;
  final VoidCallback onTap;
  final bool isAvailable;
  const ProductCard({super.key, required this.product, required this.onTap, required this.isAvailable});

  Map<String, AttributeOption> _getDefaultOptions(
      List<ProductAttribute> attributes) {
    final Map<String, AttributeOption> defaultOptions = {};

    attributes.sort((a, b) => a.displayOrder.compareTo(b.displayOrder));
    for (var attribute in attributes) {
      if (attribute.attributeOptions != null &&
          attribute.attributeOptions!.isNotEmpty) {
        final options = attribute.attributeOptions!;
        final int defaultIndex =
            options.indexWhere((opt) => opt.isDefault == true);
        final selectedOption = options[defaultIndex != -1 ? defaultIndex : 0];

        defaultOptions[attribute.productAttributeId] = selectedOption;
      }
    }
    return defaultOptions;
  }

  @override
  Widget build(BuildContext context) {
    var cart = context.read<CartCubit>();

    void addToCart(Product p) {
      final attributes = p.getAttributes();
      final selectedOptions = _getDefaultOptions(attributes);

      // Convert to ProductAttributeSelection
      final selectedAttributes = selectedOptions.entries
          .map((e) => ProductAttributeSelection(
                productAttributeId: e.key,
                attributeOptionId: e.value.attributeOptionId,
              ))
          .toList();

      // Generate description
      final optionDescriptions = selectedOptions.entries.map((entry) {
        final attributeId = entry.key;
        final option = entry.value;
        final attribute = p
            .getAttributes()
            .firstWhere((attr) => attr.productAttributeId == attributeId);
        return '${option.name}';
      }).join(', ');

      var item = CartItem(
        id: p.productId,
        picUrl: p.imageUrl,
        description: optionDescriptions,
        name: p.name,
        price: p.price,
        selectedAttributes: selectedAttributes,
      );
      final alreadyInCart = cart.state.items.containsKey(item.uniqueKey);
      cart.addItem(item);
      if (!alreadyInCart) {
        HintDialog.show(
          context,
          'Đã thêm vào giỏ hàng!',
          'Bạn có thể xem lại trong giỏ hàng.',
        );
      }
    }

    return Opacity(
      opacity: isAvailable ? 1.0 : 0.5,
      child: Card(
      color: AppPalette.white,
      elevation: 0,
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(8),
        side: const BorderSide(color: AppPalette.white),
      ),
      child: InkWell(
        onTap: isAvailable ? onTap : null,
        borderRadius: BorderRadius.circular(16),
        child: Container(
          height: 140,
          padding: const EdgeInsets.all(12),
          child: Row(
            children: [
              Container(
                width: 116,
                height: 116,
                decoration: BoxDecoration(
                  borderRadius: BorderRadius.circular(12),
                  boxShadow: [
                    BoxShadow(
                      color: Colors.black.withOpacity(0.1),
                      blurRadius: 8,
                      offset: const Offset(0, 2),
                    ),
                  ],
                ),
                child: ImageLoad(
                    imageUrl: product.imageUrl,
                    defaultImageUrl: ImagePath.logo),
              ),
              const SizedBox(width: 16),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    Text(
                      product.name,
                      style: AppTypography.screenTitle.copyWith(
                        fontSize: 18,
                        fontWeight: FontWeight.w600,
                        color: Colors.black87,
                      ),
                      maxLines: 2,
                      overflow: TextOverflow.ellipsis,
                    ),
                    const SizedBox(height: 8),
                    Text(
                      product.description ?? "",
                      style: AppTypography.productDescription.copyWith(
                        fontSize: 14,
                        color: Colors.black54,
                        height: 1.2,
                      ),
                      maxLines: 2,
                      overflow: TextOverflow.ellipsis,
                    ),
                    const Spacer(),
                    Row(
                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                      children: [
                        Text(
                          formatPrice(product.price),
                          style: AppTypography.productPrice.copyWith(
                            fontSize: 16,
                            fontWeight: FontWeight.w700,
                            color: AppPalette.blue.primary,
                          ),
                        ),
                        InkWell(
                          onTap: isAvailable ? () => addToCart(product) : null,
                          borderRadius: BorderRadius.circular(8),
                          child: Container(
                            width: 36,
                            height: 36,
                            decoration: BoxDecoration(
                              color: isAvailable
                                    ? AppPalette.blue.primary
                                    : Colors.grey,
                              borderRadius: BorderRadius.circular(8),
                            ),
                            child: const Icon(Icons.add,
                                color: AppPalette.white, size: 20),
                          ),
                        ),
                      ],
                    ),
                  ],
                ),
              ),
            ],
          ),
        ),
      ),
    ),
    );
  }
}
