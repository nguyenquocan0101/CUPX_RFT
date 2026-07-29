import 'package:flutter/material.dart';

class ReadOnlyQuantityDisplay extends StatelessWidget {
  final int quantity;
  
  const ReadOnlyQuantityDisplay({
    super.key,
    required this.quantity,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 10),
      decoration: BoxDecoration(
        color: Colors.grey[100],
        borderRadius: BorderRadius.circular(8),
        border: Border.all(color: Colors.grey[300]!),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(
            Icons.production_quantity_limits,
            size: 16,
            color: Colors.grey[600],
          ),
          const SizedBox(width: 6),
          Text(
            '$quantity',
            style: TextStyle(
              fontWeight: FontWeight.w600,
              fontSize: 16,
              color: Colors.grey[700],
            ),
          ),
        ],
      ),
    );
  }
}