import 'package:abc_androidapp/config/themes/app_palette.dart';
import 'package:flutter/material.dart';

class QuantitySelector extends StatelessWidget {
  final int quantity;
  final VoidCallback onIncrease;
  final VoidCallback onDecrease;

  const QuantitySelector({
    super.key,
    required this.quantity,
    required this.onIncrease,
    required this.onDecrease,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(4),
      decoration: BoxDecoration(
        color: Colors.grey[100],
        borderRadius: BorderRadius.circular(8),
        border: Border.all(color: Colors.grey[300]!),
      ),
      child: Row(
        children: [
          InkWell(
            onTap: onDecrease,
            child: Container(
              padding: const EdgeInsets.all(8),
              decoration: BoxDecoration(
                color: Colors.white,
                borderRadius: BorderRadius.circular(6),
              ),
              child: const Icon(
                Icons.remove,
                size: 18,
                color: Colors.black87,
              ),
            ),
          ),
          Container(
            alignment: Alignment.center,
            width: 36,
            child: Text(
              '$quantity',
              style:  
                  const TextStyle(
                    color: Colors.black87,
                    fontWeight: FontWeight.w600,
                    fontSize: 16,
                  ),
            ),
          ),
          InkWell(
            onTap: onIncrease,
            child: Container(
              padding: const EdgeInsets.all(8),
              decoration: BoxDecoration(
                color: AppPalette.blue.primary,
                borderRadius: BorderRadius.circular(6),
              ),
              child: const Icon(
                Icons.add,
                size: 18,
                color: Colors.white,
              ),
            ),
          ),
        ],
      ),
    );
  }
}
