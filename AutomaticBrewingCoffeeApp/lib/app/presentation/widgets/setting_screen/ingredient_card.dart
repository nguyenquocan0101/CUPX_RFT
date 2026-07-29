import 'package:flutter/material.dart';
import 'package:tdesign_flutter/tdesign_flutter.dart';
import 'package:abc_androidapp/app/data/models/organization/kiosk.dart';

class IngredientCard extends StatelessWidget {
  final DeviceIngredientState ingredient;
  final VoidCallback onRefill;

  const IngredientCard({
    super.key,
    required this.ingredient,
    required this.onRefill,
  });

  @override
  Widget build(BuildContext context) {
    final percentage = ingredient.capacityPercentage;
    final isLow = ingredient.isLowCapacity;

    return Container(
      margin: const EdgeInsets.only(bottom: 16),
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(12),
        border: Border.all(
          color: isLow ? Colors.red.shade200 : Colors.grey.shade200,
          width: isLow ? 2 : 1,
        ),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          _buildHeader(percentage, isLow),
          const SizedBox(height: 12),
          _buildProgressBar(percentage),
          const SizedBox(height: 12),
          _buildRefillButton(),
        ],
      ),
    );
  }

  Widget _buildHeader(double percentage, bool isLow) {
    return Row(
      mainAxisAlignment: MainAxisAlignment.spaceBetween,
      children: [
        Expanded(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                ingredient.ingredientType,
                style: const TextStyle(
                  fontSize: 16,
                  fontWeight: FontWeight.bold,
                  color: Colors.black87,
                ),
              ),
              const SizedBox(height: 4),
              Text(
                '${ingredient.currentCapacity}/${ingredient.maxCapacity} ${ingredient.unit}',
                style: TextStyle(
                  fontSize: 14,
                  color: Colors.grey[600],
                ),
              ),
            ],
          ),
        ),
        Column(
          crossAxisAlignment: CrossAxisAlignment.end,
          children: [
            Text(
              '${percentage.toStringAsFixed(1)}%',
              style: TextStyle(
                fontSize: 18,
                fontWeight: FontWeight.bold,
                color: _getCapacityColor(percentage),
              ),
            ),
            if (isLow) _buildLowCapacityBadge(),
          ],
        ),
      ],
    );
  }

  Widget _buildLowCapacityBadge() {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
      decoration: BoxDecoration(
        color: Colors.red.shade50,
        borderRadius: BorderRadius.circular(8),
        border: Border.all(color: Colors.red.shade200),
      ),
      child: const Text(
        'Cần refill',
        style: TextStyle(
          fontSize: 12,
          color: Colors.red,
          fontWeight: FontWeight.w500,
        ),
      ),
    );
  }

  Widget _buildProgressBar(double percentage) {
    return Container(
      height: 8,
      decoration: BoxDecoration(
        color: Colors.grey.shade200,
        borderRadius: BorderRadius.circular(4),
      ),
      child: FractionallySizedBox(
        alignment: Alignment.centerLeft,
        widthFactor: percentage / 100,
        child: Container(
          decoration: BoxDecoration(
            color: _getCapacityColor(percentage),
            borderRadius: BorderRadius.circular(4),
          ),
        ),
      ),
    );
  }

  Widget _buildRefillButton() {
    return SizedBox(
      width: double.infinity,
      child: ElevatedButton.icon(
        onPressed: onRefill,
        icon: const Icon(TDIcons.refresh, size: 18),
        label: const Text('Xác nhận đã refill'),
        style: ElevatedButton.styleFrom(
          backgroundColor: const Color(0xFF57B7E7),
          foregroundColor: Colors.white,
          padding: const EdgeInsets.symmetric(vertical: 12),
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(8),
          ),
          elevation: 0,
        ),
      ),
    );
  }

  Color _getCapacityColor(double percentage) {
    if (percentage > 60) return Colors.green;
    if (percentage > 30) return Colors.orange;
    return Colors.red;
  }
}