import 'package:abc_androidapp/app/presentation/widgets/setting_screen/ingredient_card.dart';
import 'package:abc_androidapp/app/presentation/widgets/setting_screen/info_row.dart';
import 'package:flutter/material.dart';
import 'package:tdesign_flutter/tdesign_flutter.dart';
import 'package:abc_androidapp/app/data/models/organization/kiosk.dart';

class DeviceInfoCard extends StatelessWidget {
  final KioskDevice device;
  final Function(DeviceIngredientState) onIngredientRefill;

  const DeviceInfoCard({
    super.key,
    required this.device,
    required this.onIngredientRefill,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.grey.shade50,
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: Colors.grey.shade200),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          _buildDeviceInfoHeader(),
          const SizedBox(height: 12),
          _buildDeviceDetails(),
          if (device.deviceIngredientStates.isNotEmpty) ...[
            const SizedBox(height: 24),
            _buildIngredientsSection(),
          ],
        ],
      ),
    );
  }

  Widget _buildDeviceInfoHeader() {
    return Row(
      children: [
        Container(
          padding: const EdgeInsets.all(6),
          decoration: BoxDecoration(
            color: const Color(0xFF57B7E7).withOpacity(0.1),
            borderRadius: BorderRadius.circular(8),
          ),
          child: const Icon(
            TDIcons.info_circle,
            color: Color(0xFF57B7E7),
            size: 16,
          ),
        ),
        const SizedBox(width: 8),
        const Text(
          'Thông tin thiết bị',
          style: TextStyle(
            fontSize: 16,
            fontWeight: FontWeight.bold,
            color: Colors.black87,
          ),
        ),
      ],
    );
  }

  Widget _buildDeviceDetails() {
    return Column(
      children: [
        InfoRow(label: 'Loại thiết bị:', value: device.deviceModel.deviceType.name),
        InfoRow(label: 'Nhà sản xuất:', value: device.deviceModel.manufacturer),
        InfoRow(label: 'Mô tả:', value: device.description),
        InfoRow(
          label: 'Trạng thái:',
          value: device.status,
          valueColor: device.status.toLowerCase() == 'working' ? Colors.green : Colors.red,
        ),
      ],
    );
  }

  Widget _buildIngredientsSection() {
    return Column(
      children: [
        Row(
          children: [
            Container(
              padding: const EdgeInsets.all(6),
              decoration: BoxDecoration(
                color: Colors.orange.withOpacity(0.1),
                borderRadius: BorderRadius.circular(8),
              ),
              child: const Icon(
                TDIcons.layers,
                color: Colors.orange,
                size: 16,
              ),
            ),
            const SizedBox(width: 8),
            const Text(
              'Nguyên liệu',
              style: TextStyle(
                fontSize: 16,
                fontWeight: FontWeight.bold,
                color: Colors.black87,
              ),
            ),
            const Spacer(),
            Container(
              padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
              decoration: BoxDecoration(
                color: Colors.orange.withOpacity(0.1),
                borderRadius: BorderRadius.circular(12),
                border: Border.all(color: Colors.orange.withOpacity(0.3)),
              ),
              child: Text(
                '${device.deviceIngredientStates.length} loại',
                style: const TextStyle(
                  fontSize: 12,
                  fontWeight: FontWeight.w600,
                  color: Colors.orange,
                ),
              ),
            ),
          ],
        ),
        const SizedBox(height: 16),
        ...device.deviceIngredientStates.map(
          (ingredient) => IngredientCard(
            ingredient: ingredient,
            onRefill: () => onIngredientRefill(ingredient),
          ),
        ),
      ],
    );
  }
}