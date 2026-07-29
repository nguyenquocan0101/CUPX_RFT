import 'package:flutter/material.dart';

class InfoRow extends StatelessWidget {
  final String label;
  final String value;
  final Color? valueColor;
  final IconData? icon;

  const InfoRow({
    super.key,
    required this.label,
    required this.value,
    this.valueColor,
    this.icon,
  });

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 16),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          SizedBox(
            width: 150,
            child: Text(
              label,
              style: TextStyle(
                color: Colors.grey[700],
                fontSize: 16,
                fontWeight: FontWeight.w500,
              ),
            ),
          ),
          Expanded(
            child: Row(
              children: [
                if (icon != null) ...[
                  Icon(icon!, size: 16, color: valueColor ?? Colors.black87),
                  const SizedBox(width: 6),
                ],
                Expanded(
                  child: Text(
                    value,
                    style: TextStyle(
                      fontSize: 16,
                      fontWeight: FontWeight.w500,
                      color: valueColor ?? Colors.black87,
                    ),
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}