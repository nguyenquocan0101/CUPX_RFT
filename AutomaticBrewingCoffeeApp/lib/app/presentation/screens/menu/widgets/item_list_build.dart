import 'package:abc_androidapp/app/core/helpers/price_formatter.dart';
import 'package:abc_androidapp/app/data/local_models/cart_item.dart';
import 'package:abc_androidapp/app/presentation/cubits/cart/cart_cubit.dart';
import 'package:abc_androidapp/app/presentation/widgets/common/product_load.dart';
import 'package:abc_androidapp/app/presentation/widgets/common/quantity_selector.dart';
import 'package:abc_androidapp/config/constants/size_config.dart';
import 'package:abc_androidapp/config/themes/app_palette.dart';
import 'package:flutter/material.dart';
import 'package:flutter/widgets.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:flutter_slidable/flutter_slidable.dart';

class ItemListBuild extends StatefulWidget {
  final List<String> chosenItems;
  final Function(String) onChoiceChange;
  const ItemListBuild({
    super.key,
    required this.chosenItems,
    required this.onChoiceChange,
  });

  @override
  State<ItemListBuild> createState() => _ItemListBuildState();
}

class _ItemListBuildState extends State<ItemListBuild> {
  final double fem = SizeConfig.fem;
  final Color primaryColor = AppPalette.blue.primary;

  late CartCubit cart;

  @override
  void initState() {
    super.initState();
    cart = context.read<CartCubit>();
  }

  void onDeleteAll() {
    cart.clear();
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
    //remove in chosen item list too
    if(widget.chosenItems.contains(uniqueKey)){
      widget.chosenItems.remove(uniqueKey);
    }
  }

  @override
  Widget build(BuildContext context) {
    return BlocBuilder<CartCubit, CartState>(
      builder: (context, state) {
        var items = state.itemList;
        return SlidableAutoCloseBehavior(
          closeWhenOpened: true,
          child: ListView.separated(
            padding: EdgeInsets.symmetric(horizontal: 20 * fem),
            itemCount: items.length,
            separatorBuilder: (context, index) => SizedBox(height: 16 * fem),
            itemBuilder: (context, index) {
              final item = items[index];
              return _item(item);
            },
          ),
        );
      },
    );
  }

  Widget _item(CartItem item) {
    bool isChosen = widget.chosenItems.contains(item.uniqueKey);

    return ClipRect(
      child: Slidable(
        key: ValueKey(item.uniqueKey),
        endActionPane: ActionPane(
          motion: const StretchMotion(),
          extentRatio: 0.25,
          children: [
            SlidableAction(
              onPressed: (_) => removeItem(item.uniqueKey),
              backgroundColor: AppPalette.red,
              foregroundColor: AppPalette.white,
              icon: Icons.delete_outline_rounded,
              label: 'Xóa',
              borderRadius:
                  const BorderRadius.horizontal(right: Radius.circular(12)),
            ),
          ],
        ),
        child: Container(
          height: 100 * fem,
          decoration: BoxDecoration(
            color: Colors.grey[50],
            borderRadius: BorderRadius.circular(12 * fem),
          ),
          child: Row(
            children: [
              Checkbox(
                activeColor: AppPalette.blue.primary,
                value: isChosen,
                onChanged: (value){
                  if(value == null) return;
                   setState(() {
                     isChosen = !value;
                   }); 
                   widget.onChoiceChange(item.uniqueKey);          
                },
              ),
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
                    QuantitySelector(
                      quantity: item.quantity,
                      onDecrease: () => decreaseQuantity(item),
                      onIncrease: () => increaseQuantity(item),
                    ),
                    SizedBox(height: 8 * fem),
                    Row(
                      children: [
                        Text("Tổng: "),
                        SizedBox(width: 8 * fem),
                        Text(
                          formatPrice(item.total),
                        ),
                      ],
                    ),
                  ],
                ),
              ),
              // Cột dấu chấm
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
