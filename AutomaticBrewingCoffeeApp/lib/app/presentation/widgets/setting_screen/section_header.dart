import 'package:flutter/material.dart';
import 'package:tdesign_flutter/tdesign_flutter.dart';

class SectionHeader extends StatelessWidget {
  final String title;
  final IconData icon;
  final Widget? trailing;

  const SectionHeader({
    super.key,
    required this.title,
    required this.icon,
    this.trailing,
  });

  @override
  Widget build(BuildContext context) {
    return Row(
      children: [
        Container(
          padding: const EdgeInsets.all(12),
          decoration: BoxDecoration(
            color: const Color(0xFFE6F5FC),
            borderRadius: BorderRadius.circular(12),
          ),
          child: Icon(icon, color: const Color(0xFF57B7E7), size: 24),
        ),
        const SizedBox(width: 16),
        Text(
          title,
          style: const TextStyle(
            fontSize: 18,
            fontWeight: FontWeight.bold,
            color: Color(0xFF57B7E7),
          ),
        ),
        const Spacer(),
        if (trailing != null) trailing!,
      ],
    );
  }
}