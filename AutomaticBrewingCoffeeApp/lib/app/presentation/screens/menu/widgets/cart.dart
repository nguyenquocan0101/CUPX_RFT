import 'package:abc_androidapp/app/presentation/cubits/cart/cart_cubit.dart';
import 'package:abc_androidapp/app/presentation/screens/menu/widgets/item_list_build.dart';
import 'package:abc_androidapp/app/presentation/widgets/common/price_row.dart';
import 'package:abc_androidapp/config/constants/animation_path.dart';
import 'package:abc_androidapp/config/constants/size_config.dart';
import 'package:abc_androidapp/config/themes/app_palette.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:lottie/lottie.dart';

class Cart extends StatefulWidget {
  const Cart({super.key});

  @override
  State<Cart> createState() => _CartState();
}

class _CartState extends State<Cart> {
  final double fem = SizeConfig.fem;
  final Color primaryColor = AppPalette.blue.primary;

  late CartCubit cart;
  //* many item delete logic
  bool isMultipleChoice = false;
  List<String> chosenItems = [];

  @override
  void initState() {
    super.initState();
    cart = context.read<CartCubit>();
  }

  void onDeleteMany() {
    if (chosenItems.isEmpty) return;

    for (final id in chosenItems) {
      cart.removeItem(id);
    }

    setState(() {
      chosenItems.clear();
      isMultipleChoice = false;
    });
  }

  void handleChooseAll(bool? isChooseAll) {
    if (isChooseAll == null) return;

    setState(() {
      isMultipleChoice = isChooseAll;
      chosenItems =
          isChooseAll ? cart.state.itemList.map((x) => x.uniqueKey).toList() : [];
    });
  }

  void itemChangeChoice(String id) {
    setState(() {
      if (chosenItems.contains(id)) {
        chosenItems.remove(id);

        if (chosenItems.isEmpty) {
          isMultipleChoice = false;
        }
      } else {
        chosenItems.add(id);
      }
    });
  }

  @override
  Widget build(BuildContext context) {
    final Size screenSize = MediaQuery.of(context).size;
    final bool isTablet = screenSize.width >= 600;

    return BlocBuilder<CartCubit, CartState>(
      builder: (context, state) {
        var total = state.totalAmount;
        var anyItem = state.itemCount != 0;
        // Thêm check để reset state khi không còn items trong cart
        if (!anyItem && (isMultipleChoice || chosenItems.isNotEmpty)) {
          WidgetsBinding.instance.addPostFrameCallback((_) {
            setState(() {
              isMultipleChoice = false;
              chosenItems.clear();
            });
          });
        }

        if (state.itemCount == 0) {
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
              ],
            ),
          );
        }

        return Container(
          height: isTablet ? screenSize.height * 0.7 : screenSize.height * 0.85,
          decoration: BoxDecoration(
            color: Colors.white,
            borderRadius: BorderRadius.vertical(top: Radius.circular(24 * fem)),
            boxShadow: [
              BoxShadow(
                color: Colors.black.withOpacity(0.1),
                blurRadius: 10 * fem,
                offset: Offset(0, -2 * fem),
              ),
            ],
          ),
          padding: const EdgeInsets.symmetric(vertical: 16.0),
          child: Column(
            children: [
              _buildHeader(
                  anyItem: anyItem,
                  isMultipleChoice: isMultipleChoice,
                  onChange: handleChooseAll),
              Expanded(
                child: ItemListBuild(
                    chosenItems: chosenItems, onChoiceChange: itemChangeChoice),
              ),
              Divider(height: 1 * fem),
              Padding(
                padding: EdgeInsets.all(20 * fem),
                child: Column(
                  children: [
                    // _buildPriceRow('Tạm tính:', subTotal),
                    // SizedBox(height: 8 * fem),
                    // _buildPriceRow('Giảm giá:', discount),
                    // SizedBox(height: 8 * fem),
                    PriceRow(
                      label: 'Tổng cộng:',
                      amount: total,
                      fem: fem,
                      isTotal: true,
                      primaryColor: AppPalette.blue.primary,
                    ),
                  ],
                ),
              ),
            ],
          ),
        );
      },
    );
  }

  Widget _buildHeader(
      {required bool anyItem,
      required bool isMultipleChoice,
      required Function(bool?) onChange}) {
    return Container(
      padding: EdgeInsets.symmetric(horizontal: 20 * fem, vertical: 16 * fem),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          Row(
            children: [
              if (anyItem)
                Checkbox(
                  value: isMultipleChoice && chosenItems.isNotEmpty,
                  onChanged: onChange,
                  activeColor: AppPalette.blue.primary,
                ),
              SizedBox(width: 6 * fem),
              Text(
                'Giỏ hàng',
                style: TextStyle(
                  fontSize: 18 * fem,
                  fontWeight: FontWeight.w600,
                ),
              ),
            ],
          ),
          if (anyItem && chosenItems.isNotEmpty)
            TextButton.icon(
              onPressed: onDeleteMany,
              icon: Icon(
                Icons.delete_outline,
                color: AppPalette.red,
                size: 20 * fem,
              ),
              label: Text(
                'Xóa đã chọn (${chosenItems.length})', // Thêm số lượng items được chọn
                style: TextStyle(color: AppPalette.red, fontSize: 14 * fem),
              ),
              style: TextButton.styleFrom(
                padding: EdgeInsets.symmetric(horizontal: 12 * fem),
                minimumSize: Size(0, 36 * fem),
              ),
            ),
        ],
      ),
    );
  }
}
