import 'package:flutter/material.dart';

class SettingCard extends StatelessWidget {
  final Widget child;
  final EdgeInsetsGeometry? padding;

  const SettingCard({
    super.key,
    required this.child,
    this.padding = const EdgeInsets.all(24),
  });

  @override
  Widget build(BuildContext context) {
    return Card(
      elevation: 0,
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(20),
        side: BorderSide(color: Colors.grey.shade200),
      ),
      color: Colors.white,
      child: Padding(
        padding: padding!,
        child: child,
      ),
    );
  }
}