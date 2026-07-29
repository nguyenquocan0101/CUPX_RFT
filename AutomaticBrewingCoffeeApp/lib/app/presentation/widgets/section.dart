import 'package:abc_androidapp/config/themes/app_palette.dart';
import 'package:flutter/material.dart';

class Section extends StatelessWidget {
  final String title;
  final Widget detail;
  static const double sectionHeight = 55.0;
  
  const Section({
    super.key, 
    required this.title, 
    required this.detail,
  });

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Padding(
          padding: const EdgeInsets.symmetric(
            horizontal: 16.0,
            vertical: 12.0,
          ),
          child: Row(
            children: [
              Text(
                title.toUpperCase(),
                style: Theme.of(context).textTheme.titleMedium?.copyWith(
                  fontWeight: FontWeight.w600,
                  color: AppPalette.blue.primary, 
                  letterSpacing: 0.5, 
                ),
              ),
              const SizedBox(width: 8),
              Expanded(
                child: Container(
                  height: 1,
                  color: Colors.grey.withOpacity(0.2),
                ),
              ),
            ],
          ),
        ),
        const SizedBox(height: 8.0), 
        detail,
      ],
    );
  }
}