import 'package:abc_androidapp/app/presentation/widgets/common/product_load.dart';
import 'package:flutter/material.dart';
import 'package:abc_androidapp/config/themes/app_palette.dart';
import 'package:abc_androidapp/config/constants/image_path.dart';

class SideBarItem extends StatelessWidget {
  final VoidCallback onTap;
  final String title;
  final String? imageUrl; // Default image URL
  final String? iconPath;
  final bool isChosen;

  const SideBarItem({
    super.key,
    required this.title,
    required this.imageUrl,
    required this.onTap,
    this.iconPath,
    this.isChosen = false,
  });

  @override
  Widget build(BuildContext context) {
    final Color textColor = isChosen ? AppPalette.white : AppPalette.grey.grey2;

    return GestureDetector(
      onTap: onTap,
      child: Container(
        // width: 140,
        // height: 140,
        padding: const EdgeInsets.all(16.0),
        margin: const EdgeInsets.all(16.0),
        decoration: ShapeDecoration(
          color: isChosen ? AppPalette.blue.blue2 : AppPalette.transparent,
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(20),
          ),
        ),
        child: Column(
          mainAxisSize: MainAxisSize.max,
          mainAxisAlignment: MainAxisAlignment.center,
          crossAxisAlignment: CrossAxisAlignment.center,
          children: [
            SizedBox(
              width: 50,
              height: 50,
              // child: Image.asset(
              //   isChosen
              //       ? ImagePath.whiteCupIcon
              //       : ImagePath.greyOutlineCupIcon,
              //   width: 46,
              //   height: 46,
              //   fit: BoxFit.contain,
              // ),
              child: ImageLoad(imageUrl: imageUrl, defaultImageUrl: isChosen ? ImagePath.whiteCupIcon : ImagePath.greyOutlineCupIcon),
            ),
            const SizedBox(height: 10),
            Text(
              title,
              textAlign: TextAlign.center,
              style: Theme.of(context).textTheme.titleMedium!.copyWith(
                  fontWeight: FontWeight.w400,
                  letterSpacing: 0.5,
                  height: 1.2,
                  color: textColor),
              maxLines: 2,
              overflow: TextOverflow.visible,
            ),
          ],
        ),
      ),
    );
  }
}
