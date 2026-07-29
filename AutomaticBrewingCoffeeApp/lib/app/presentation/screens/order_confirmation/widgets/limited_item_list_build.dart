import 'package:abc_androidapp/app/core/helpers/price_formatter.dart';
import 'package:abc_androidapp/app/data/local_models/cart_item.dart';
import 'package:abc_androidapp/app/presentation/cubits/cart/cart_cubit.dart';
import 'package:abc_androidapp/app/presentation/widgets/common/product_load.dart';
import 'package:abc_androidapp/app/presentation/widgets/common/quantity_selector.dart';
import 'package:abc_androidapp/app/presentation/widgets/common/read_only_quantity_display.dart';
import 'package:abc_androidapp/config/constants/animation_path.dart';
import 'package:abc_androidapp/config/constants/size_config.dart';
import 'package:abc_androidapp/config/themes/app_palette.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:flutter_slidable/flutter_slidable.dart';
import 'package:lottie/lottie.dart';

class LimitedItemListBuild extends StatefulWidget {
  final List<CartItem> items;
  final bool isAdjustable;

  const LimitedItemListBuild({
    super.key,
    required this.items,
    this.isAdjustable = true,
  });

  @override
  State<LimitedItemListBuild> createState() => _LimitedItemListBuildState();
}

class _LimitedItemListBuildState extends State<LimitedItemListBuild> {
  final double fem = SizeConfig.fem;
  final Color primaryColor = AppPalette.blue.primary;
  late bool _isExpanded;

  late CartCubit cart;

  @override
  void initState() {
    super.initState();
    cart = context.read<CartCubit>();
    _isExpanded = !(widget.items.length > 2);
  }

  void increaseQuantity(CartItem item) {
    cart.updateQuantity(item.uniqueKey, item.quantity + 1);
  }

   void decreaseQuantity(CartItem item) {
    // if (item.quantity > 1) {
    //   cart.updateQuantity(item.id, item.quantity - 1);
    // }
     cart.updateQuantity(item.uniqueKey, item.quantity - 1);
  }

  void removeItem(String uniqueKey) {
    cart.removeItem(uniqueKey);
  }

  Widget _buildExpandButton() {
    return Align(
      alignment: Alignment.centerLeft,
      child: TextButton(
        onPressed: () => setState(() => _isExpanded = !_isExpanded),
        child: Text(
          _isExpanded ? 'Thu gọn' : 'Xem thêm',
          style: TextStyle(
            color: primaryColor,
            fontWeight: FontWeight.w500,
          ),
        ),
      ),
    );
  }

  

  @override
  Widget build(BuildContext context) {
    if (widget.items.isEmpty) {
      return Center(
        child: Column(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                Lottie.asset(
                  AnimationPath.noFound,
                  width: 200,
                  height: 200,
                  repeat: false,
                  frameRate: FrameRate.max,
                ),
                SizedBox(height: 16 * fem),
                Text(
                  'Giỏ hàng trống',
                  style: TextStyle(
                    fontSize: 16 * fem,
                    color: Colors.grey[600],
                    fontWeight: FontWeight.w500,
                  ),
                ),
                SizedBox(height: 8 * fem),
                Text(
                  'Hãy thêm sản phẩm vào giỏ hàng',
                  style: TextStyle(
                    fontSize: 14 * fem,
                    color: Colors.grey[500],
                  ),
                ),
                SizedBox(height: 16 * fem),
              ],
            ),
      );
    }

    return SlidableAutoCloseBehavior(
      closeWhenOpened: true,
      child: Column(
        children: [
          ...List.generate(_buildCartItemsList().length, (index) {
            return Padding(
              padding: EdgeInsets.only(bottom: 8 * fem),
              child: _buildCartItemsList()[index],
            );
          }),
          if (widget.items.length > 3) _buildExpandButton(),
        ],
      ),
    );

    
  }
  List<Widget> _buildCartItemsList() {
    return (_isExpanded ? widget.items : widget.items.take(3))
        .map((item) => _buildCartItem(
              item: item,
              decrease: () => decreaseQuantity(item),
              increase: () => increaseQuantity(item),
              onDelete: () => removeItem(item.uniqueKey),
            ))
        .toList();
  }

  Widget _buildCartItem({
    required CartItem item,
    required VoidCallback increase,
    required VoidCallback decrease,
    required VoidCallback onDelete,
  }) {
    return ClipRect(
      child: Slidable(
        key: ValueKey(item.uniqueKey),
       endActionPane: widget.isAdjustable ? ActionPane(
        motion: const StretchMotion(),
        extentRatio: 0.25,
        children: [
          SlidableAction(
            onPressed: (_) => onDelete(),
            backgroundColor: AppPalette.red,
            foregroundColor: AppPalette.white,
            icon: Icons.delete_outline_rounded,
            label: 'Xóa',
            borderRadius: const BorderRadius.horizontal(
              right: Radius.circular(12),
            ),
          ),
        ],
      ) : null, 
        child: Container(
          height: 100 * fem,
          decoration: BoxDecoration(
            color: Colors.grey[50],
            borderRadius: BorderRadius.circular(12 * fem),
          ),
          child: Row(
            children: [
              ClipRRect(
                borderRadius: BorderRadius.only(
                  topLeft: Radius.circular(12 * fem),
                  bottomLeft: Radius.circular(12 * fem),
                ),
                child: ImageLoad(imageUrl: item.picUrl),
              ),
              Expanded(
                child: Padding(
                  padding: EdgeInsets.all(12 * fem),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    mainAxisAlignment: MainAxisAlignment.center,
                    children: [
                      Text(
                        item.name,
                        style: TextStyle(
                          fontWeight: FontWeight.w600,
                          fontSize: 16 * fem,
                        ),
                        maxLines: 2,
                        overflow: TextOverflow.ellipsis,
                      ),
                      SizedBox(height: 4 * fem),
                       if (item.description.isNotEmpty)
                      Text(
                        item.description,
                        style: TextStyle(
                          fontSize: 13 * fem,
                          color: Colors.grey[600],
                          height: 1.2,
                        ),
                        maxLines: 2,
                        overflow: TextOverflow.ellipsis,
                      ),
                    
                    const Spacer(),
                      Text(
                        formatPrice(item.price),
                        style: TextStyle(
                          fontWeight: FontWeight.w600,
                          color: primaryColor,
                          fontSize: 16 * fem,
                        ),
                      ),
                    ],
                  ),
                ),
              ),
              Padding(
                padding: EdgeInsets.only(right: 12 * fem),
                child: Column(
                  mainAxisAlignment: MainAxisAlignment.center,
                  crossAxisAlignment: CrossAxisAlignment.end,
                  children: [
                    if (widget.isAdjustable)
                      QuantitySelector(
                        quantity: item.quantity,
                        onDecrease: decrease,
                        onIncrease: increase,
                      )
                    else
                      ReadOnlyQuantityDisplay(quantity: item.quantity),
                    SizedBox(height: 8 * fem),
                    Row(
                      children: [
                        const Text("Tổng: "),
                        SizedBox(width: 8 * fem),
                        Text(
                          formatPrice(item.total),
                        ),
                      ],
                    ),
                  ],
                ),
              ),
              const SizedBox(width: 12),
              // Column(
              //   mainAxisAlignment: MainAxisAlignment.center,
              //   children: List.generate(
              //     4,
              //     (_) => Padding(
              //       padding: const EdgeInsets.symmetric(vertical: 2),
              //       child: Container(
              //         width: 4,
              //         height: 4,
              //         decoration: const BoxDecoration(
              //           color: Colors.black26,
              //           shape: BoxShape.circle,
              //         ),
              //       ),
              //     ),
              //   ),
              // ),
              //const SizedBox(width: 12),
            ],
          ),
        ),
      ),
    );
  }
}
