import 'package:abc_androidapp/config/constants/image_path.dart';
import 'package:abc_androidapp/config/themes/app_palette.dart';
import 'package:flutter/material.dart';

class ImageLoad extends StatelessWidget {
  final String? imageUrl;
  final double size;
  final double borderRadius;
  final String? defaultImageUrl;

  const ImageLoad({
    Key? key,
    this.imageUrl,
    this.size = 90,
    this.borderRadius = 10,
    this.defaultImageUrl,
  }) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return ClipRRect(
      borderRadius: BorderRadius.circular(borderRadius),
      child: Image.network(
        imageUrl ?? defaultImageUrl ?? ImagePath.logo,
        width: size,
        height: size,
        fit: BoxFit.cover,
        errorBuilder: (context, error, stackTrace) {
          return Image.asset(
            defaultImageUrl ?? ImagePath.logo,
            width: size,
            height: size,
            fit: BoxFit.cover,
          );
        },
        loadingBuilder: (context, child, loadingProgress) {
          if (loadingProgress == null) return child;
          return Container(
            width: size,
            height: size,
            decoration: BoxDecoration(
              color: AppPalette.transparent,
              borderRadius: BorderRadius.circular(borderRadius),
            ),
            child: Center(
              child: CircularProgressIndicator(
                color: AppPalette.blue.primary,
                value: loadingProgress.expectedTotalBytes != null
                    ? loadingProgress.cumulativeBytesLoaded /
                        loadingProgress.expectedTotalBytes!
                    : null,
              ),
            ),
          );
        },
      ),
    );
  }
}