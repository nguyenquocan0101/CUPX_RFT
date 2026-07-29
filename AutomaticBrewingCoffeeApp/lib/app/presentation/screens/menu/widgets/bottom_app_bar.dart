import 'package:abc_androidapp/app/core/helpers/price_formatter.dart';
import 'package:abc_androidapp/app/presentation/cubits/cart/cart_cubit.dart';
import 'package:abc_androidapp/app/presentation/screens/menu/widgets/cart.dart';
import 'package:abc_androidapp/config/constants/size_config.dart';
import 'package:abc_androidapp/config/themes/app_palette.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

class CartBottomBar extends StatelessWidget {
  final VoidCallback onCheckout;

  const CartBottomBar({
    super.key,
    required this.onCheckout,
  });

  Widget _buildCartIcon(int itemCount, VoidCallback onCartTap) {
    return InkWell(
      onTap: onCartTap,
      child: Stack(
        clipBehavior: Clip.none,
        children: [
          Icon(
            Icons.shopping_cart_rounded,
            color: AppPalette.blue.primary,
            size: 32,
          ),
          if (itemCount > 0)
            Positioned(
              right: -8,
              top: -8,
              child: Container(
                padding: const EdgeInsets.all(4),
                decoration: const BoxDecoration(
                  color: Colors.red,
                  shape: BoxShape.circle,
                ),
                constraints: const BoxConstraints(
                  minWidth: 16,
                  minHeight: 16,
                ),
                child: Text(
                  itemCount.toString(),
                  style: const TextStyle(
                    color: Colors.white,
                    fontSize: 10,
                    fontWeight: FontWeight.bold,
                  ),
                  textAlign: TextAlign.center,
                ),
              ),
            ),
        ],
      ),
    );
  }

  Widget _buildTotalPrice(double total) {
    return Column(
      mainAxisSize: MainAxisSize.min,
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        const Text(
          'Tổng thanh toán',
          style: TextStyle(
            color: Colors.black54,
            fontSize: 13,
          ),
        ),
        const SizedBox(height: 4),
        Text(
          formatPrice(total),
          style: const TextStyle(
            color: Color(0xFF57B7E7),
            fontSize: 18,
            fontWeight: FontWeight.w700,
          ),
        ),
      ],
    );
  }

  Widget _buildCheckoutButton() {
    return ElevatedButton(
      onPressed: onCheckout,
      style: ElevatedButton.styleFrom(
        backgroundColor: const Color(0xFF57B7E7),
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(8),
        ),
        padding: const EdgeInsets.symmetric(
          horizontal: 32,
          vertical: 12,
        ),
        elevation: 0,
      ),
      child: const Text(
        'Xác nhận',
        style: TextStyle(
          color: Colors.white,
          fontSize: 16,
          fontWeight: FontWeight.w600,
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {

        final double fem = SizeConfig.fem;

    void showCart() {
      showModalBottomSheet(
        context: context,
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.vertical(top: Radius.circular(16 * fem)),
        ),
        builder: (context) {
          return const Cart();
        },
      );
    }

    return Container(
      decoration: BoxDecoration(
        color: Colors.white,
        boxShadow: [
          BoxShadow(
            color: Colors.grey.withOpacity(0.1),
            blurRadius: 10,
            offset: const Offset(0, -5),
          ),
        ],
      ),
      padding: EdgeInsets.only(
        left: 16,
        right: 16,
        bottom: MediaQuery.of(context).padding.bottom + 16,
        top: 16,
      ),
      child: BlocBuilder<CartCubit, CartState>(
        builder: (context, state) {
          return Row(
            children: [
              _buildCartIcon(state.itemQuantity, showCart),
              const SizedBox(width: 16),
              _buildTotalPrice(state.totalAmount),
              const Spacer(),
              _buildCheckoutButton(),
            ],
          );
        },
      ),
    );
  }
}
