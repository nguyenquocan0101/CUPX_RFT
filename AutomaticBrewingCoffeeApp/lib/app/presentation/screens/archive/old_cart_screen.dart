import 'package:abc_androidapp/app/data/models/product/product.dart';
import 'package:flutter_slidable/flutter_slidable.dart';
import 'package:abc_androidapp/app/core/helpers/price_formatter.dart';
import 'package:abc_androidapp/config/themes/app_palette.dart';
import 'package:flutter/material.dart';
import 'package:abc_androidapp/app/presentation/widgets/custom_app_bar.dart';

class OldCartScreen extends StatefulWidget {
  static const String route = "/cart";
  const OldCartScreen({Key? key}) : super(key: key);

  @override
  State<OldCartScreen> createState() => _OldCartScreenState();
}

class _OldCartScreenState extends State<OldCartScreen> {
  // Có thể dùng để lưu số lượng sản phẩm
  int _quantity = 1;

  final products = [];

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: const CustomAppBar(title: "Giỏ hàng"),
      body: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          _buildSectionTitle(context, 'Danh sách món'),
          Expanded(
            child: Padding(
              padding: const EdgeInsets.only(bottom: 16.0),
              child: ListView.builder(
                itemCount: products.length,
                itemBuilder: (context, index) {
                  return _buildCartItem(context, products[index]);
                },
              ),
            ),
          )
        ],
      ),
      bottomNavigationBar: _buildPaymentAndCheckout(),
    );
  }

  Widget _buildSectionTitle(BuildContext context, String title) {
    return Padding(
      padding: const EdgeInsets.all(16),
      child: Text(
        title,
        style: Theme.of(context).textTheme.titleLarge?.copyWith(
              fontWeight: FontWeight.bold,
            ),
      ),
    );
  }

  Widget _buildCartItem(BuildContext context, Product product) {
    return Slidable(
      key: ValueKey(product.productId),
      endActionPane: ActionPane(
        motion: const StretchMotion(),
        extentRatio: 0.25,
        children: [
          SlidableAction(
            onPressed: (context) {
              // Logic xóa item
              debugPrint('Xóa ${product.name}!');
            },
            backgroundColor: AppPalette.red,
            foregroundColor: Colors.white,
            icon: Icons.delete,
            label: 'Xóa',
            borderRadius: BorderRadius.circular(20),
          ),
        ],
      ),
      child: Container(
        margin: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
        padding: const EdgeInsets.all(16),
        decoration: BoxDecoration(
          color: AppPalette.white,
          borderRadius: BorderRadius.circular(20),
          boxShadow: [
            BoxShadow(
              color: Colors.grey.withOpacity(0.1),
              spreadRadius: 1,
              blurRadius: 3,
              offset: const Offset(0, 2),
            ),
          ],
        ),
        child: Row(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            _buildProductImage(
                "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQ0eYrL8jO9bCLLy9Uk3xrqJlBibPRKBeyoDQ&s"),
            const SizedBox(width: 10.0),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  _buildProductDetails(context, product),
                  const SizedBox(height: 8),
                  Align(
                    alignment: Alignment.bottomRight,
                    child: _buildQuantityControls(context),
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildProductImage(String imageUrl) {
    return ClipRRect(
      borderRadius: BorderRadius.circular(16),
      child: Image.network(
        imageUrl,
        width: 90,
        height: 90,
        fit: BoxFit.cover,
        errorBuilder: (context, error, stackTrace) => Container(
          width: 90,
          height: 90,
          decoration: BoxDecoration(
            color: Colors.grey[200],
            borderRadius: BorderRadius.circular(16),
          ),
          child: Icon(
            Icons.image_not_supported,
            color: Colors.grey[400],
            size: 40,
          ),
        ),
      ),
    );
  }

  Widget _buildProductDetails(BuildContext context, Product product) {
    return Row(
      children: [
        Expanded(
          child: Text(
            product.name,
            style: Theme.of(context).textTheme.titleMedium,
            maxLines: 3,
            overflow: TextOverflow.ellipsis,
          ),
        ),
        const SizedBox(width: 8),
        Text(
          formatPrice(product.price),
          style: Theme.of(context).textTheme.titleMedium,
        ),
      ],
    );
  }

  Widget _buildQuantityControls(BuildContext context) {
    return Container(
      alignment: Alignment.bottomRight,
      margin: const EdgeInsets.only(top: 8),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          _buildQuantityButton(
            icon: Icons.remove,
            onTap: () {
              // Giảm số lượng
              if (_quantity > 1) {
                setState(() {
                  _quantity--;
                });
              }
            },
          ),
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 12),
            child: Text(
              '$_quantity',
              style: Theme.of(context).textTheme.bodyLarge?.copyWith(
                    fontWeight: FontWeight.bold,
                  ),
            ),
          ),
          _buildQuantityButton(
            icon: Icons.add,
            onTap: () {
              // Tăng số lượng
              setState(() {
                _quantity++;
              });
            },
          ),
        ],
      ),
    );
  }

  Widget _buildQuantityButton(
      {required IconData icon, required VoidCallback onTap}) {
    return InkWell(
      onTap: onTap,
      borderRadius: BorderRadius.circular(8),
      child: Container(
        width: 28,
        height: 28,
        decoration: BoxDecoration(
          border: Border.all(color: Colors.grey[300]!),
          borderRadius: BorderRadius.circular(8),
        ),
        child: Icon(icon, size: 16, color: Colors.black54),
      ),
    );
  }

  Widget _buildPaymentAndCheckout() {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
      decoration: BoxDecoration(
        color: Colors.white,
        boxShadow: [
          BoxShadow(
            color: Colors.grey.withOpacity(0.1),
            spreadRadius: 1,
            blurRadius: 5,
            offset: const Offset(0, -3),
          ),
        ],
      ),
      child: SafeArea(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                _buildSectionTitle(context, 'Thanh toán'),
                Container(
                  decoration: const BoxDecoration(
                    color: AppPalette.white,
                  ),
                  child: Padding(
                    padding: const EdgeInsets.symmetric(horizontal: 16.0),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Row(
                          mainAxisAlignment: MainAxisAlignment.spaceBetween,
                          children: [
                            Text(
                              'Tổng tiền hàng:',
                              style: Theme.of(context).textTheme.titleMedium,
                            ),
                            Text(
                              '1.500.000đ',
                              style: Theme.of(context).textTheme.titleMedium,
                            ),
                          ],
                        ),
                        const SizedBox(height: 10.0),
                        Row(
                          mainAxisAlignment: MainAxisAlignment.spaceBetween,
                          children: [
                            Text(
                              'Phí vận chuyển:',
                              style: Theme.of(context).textTheme.titleMedium,
                            ),
                            Text(
                              '50.000đ',
                              style: Theme.of(context).textTheme.titleMedium,
                            ),
                          ],
                        ),
                        const Divider(height: 32, thickness: 1),
                        Row(
                          mainAxisAlignment: MainAxisAlignment.spaceBetween,
                          children: [
                            Text(
                              'Tổng cộng:',
                              style: Theme.of(context).textTheme.titleLarge,
                            ),
                            Text(
                              '1.550.000đ',
                              style: Theme.of(context).textTheme.titleLarge!,
                            ),
                          ],
                        ),
                      ],
                    ),
                  ),
                ),
              ],
            ),
            const SizedBox(height: 10.0),
            Padding(
              padding: const EdgeInsets.all(24.0),
              child: ElevatedButton(
                onPressed: () {},
                style: ElevatedButton.styleFrom(
                  backgroundColor: const Color(0xFF4E9EE7),
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(30),
                  ),
                  padding: const EdgeInsets.symmetric(vertical: 16),
                ),
                child: SizedBox(
                  width: double.infinity,
                  child: Text(
                    'Thanh toán',
                    textAlign: TextAlign.center,
                    style: const TextStyle(
                      fontSize: 18,
                      fontWeight: FontWeight.bold,
                      color: Colors.white,
                    ),
                  ),
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildPaymentRow(String label, String value,
      {Color? valueColor, bool isBold = false, double textSize = 14}) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 6),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          Text(
            label,
            style: TextStyle(
              fontSize: textSize,
              fontWeight: isBold ? FontWeight.bold : FontWeight.normal,
              color: Colors.black54,
            ),
          ),
          Text(
            value,
            style: TextStyle(
              fontSize: textSize + (isBold ? 2 : 0),
              fontWeight: isBold ? FontWeight.bold : FontWeight.w500,
              color: valueColor ?? Colors.black87,
            ),
          ),
        ],
      ),
    );
  }
}
