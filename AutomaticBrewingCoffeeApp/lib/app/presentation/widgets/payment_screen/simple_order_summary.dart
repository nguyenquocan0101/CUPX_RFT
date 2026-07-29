import 'package:flutter/material.dart';
import 'package:abc_androidapp/app/data/local_models/cart_item.dart';
import 'package:abc_androidapp/config/themes/app_palette.dart';

class SimpleOrderSummary extends StatelessWidget {
  final List<CartItem> items;
  final bool showHeader;
  
  const SimpleOrderSummary({
    super.key,
    required this.items,
    this.showHeader = true,
  });

  @override
  Widget build(BuildContext context) {
    if (items.isEmpty) return const SizedBox.shrink();

    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(16),
        border: Border.all(
          color: AppPalette.blue.primary.withOpacity(0.2),
          width: 1.5,
        ),
        boxShadow: [
          BoxShadow(
            color: AppPalette.blue.primary.withOpacity(0.1),
            blurRadius: 8,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          if (showHeader) ...[
            Row(
              children: [
                Container(
                  padding: const EdgeInsets.all(6),
                  decoration: BoxDecoration(
                    color: AppPalette.blue.primary.withOpacity(0.1),
                    borderRadius: BorderRadius.circular(8),
                  ),
                  child: Icon(
                    Icons.receipt_long,
                    size: 18,
                    color: AppPalette.blue.primary,
                  ),
                ),
                const SizedBox(width: 10),
                Text(
                  'Đơn hàng của bạn',
                  style: TextStyle(
                    fontSize: 16,
                    fontWeight: FontWeight.bold,
                    color: Colors.grey.shade800,
                  ),
                ),
                const Spacer(),
                Container(
                  padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                  decoration: BoxDecoration(
                    color: AppPalette.blue.primary,
                    borderRadius: BorderRadius.circular(12),
                  ),
                  child: Text(
                    '${items.length} món',
                    style: const TextStyle(
                      color: Colors.white,
                      fontSize: 12,
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                ),
              ],
            ),
            const SizedBox(height: 16),
            Container(
              height: 1,
              decoration: BoxDecoration(
                gradient: LinearGradient(
                  colors: [
                    Colors.transparent,
                    AppPalette.blue.primary.withOpacity(0.3),
                    Colors.transparent,
                  ],
                ),
              ),
            ),
            const SizedBox(height: 12),
          ],
          
          ...items.asMap().entries.map((entry) {
            final index = entry.key;
            final item = entry.value;
            return _buildSimpleItem(item, index < items.length - 1);
          }),
        ],
      ),
    );
  }

  Widget _buildSimpleItem(CartItem item, bool showDivider) {
    return Column(
      children: [
        Container(
          padding: const EdgeInsets.symmetric(vertical: 8, horizontal: 4),
          child: Row(
            crossAxisAlignment: CrossAxisAlignment.center,
            children: [
              // Quantity badge với animation-ready design
              Container(
                width: 28,
                height: 28,
                decoration: BoxDecoration(
                  gradient: LinearGradient(
                    colors: [
                      AppPalette.blue.primary,
                      AppPalette.blue.primary.withOpacity(0.8),
                    ],
                    begin: Alignment.topLeft,
                    end: Alignment.bottomRight,
                  ),
                  borderRadius: BorderRadius.circular(14),
                  boxShadow: [
                    BoxShadow(
                      color: AppPalette.blue.primary.withOpacity(0.3),
                      blurRadius: 4,
                      offset: const Offset(0, 2),
                    ),
                  ],
                ),
                child: Center(
                  child: Text(
                    '${item.quantity}',
                    style: const TextStyle(
                      color: Colors.white,
                      fontSize: 12,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                ),
              ),
              
              const SizedBox(width: 12),
              
              // Product info với improved typography
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      item.name,
                      style: TextStyle(
                        fontSize: 14,
                        fontWeight: FontWeight.w600,
                        color: Colors.grey.shade800,
                        height: 1.2,
                      ),
                    ),
                    if (item.description.isNotEmpty) ...[
                      const SizedBox(height: 3),
                      Text(
                        item.description,
                        style: TextStyle(
                          fontSize: 12,
                          color: Colors.grey.shade600,
                          height: 1.3,
                        ),
                        maxLines: 2,
                        overflow: TextOverflow.ellipsis,
                      ),
                    ],
                  ],
                ),
              ),
              
              // Price display (tùy chọn)
              // Container(
              //   padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
              //   decoration: BoxDecoration(
              //     color: Colors.grey.shade100,
              //     borderRadius: BorderRadius.circular(8),
              //   ),
              //   child: Text(
              //     '${(item.price * item.quantity).toStringAsFixed(0)}đ',
              //     style: TextStyle(
              //       fontSize: 12,
              //       fontWeight: FontWeight.w600,
              //       color: Colors.grey.shade700,
              //     ),
              //   ),
              // ),
            ],
          ),
        ),
        
        // Divider giữa các items
        if (showDivider)
          Container(
            margin: const EdgeInsets.symmetric(vertical: 4),
            height: 0.5,
            decoration: BoxDecoration(
              gradient: LinearGradient(
                colors: [
                  Colors.transparent,
                  Colors.grey.shade300,
                  Colors.transparent,
                ],
              ),
            ),
          ),
      ],
    );
  }
}