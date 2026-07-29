import 'package:abc_androidapp/config/themes/app_palette.dart';
import 'package:abc_androidapp/config/themes/app_typography.dart';
import 'package:flutter/material.dart';

class CustomAppBar extends StatelessWidget implements PreferredSizeWidget {
  final String? title;
  final TextStyle? titleStyle;
  final Color backgroundColor;
  final Color surfaceTintColor;
  final double elevation;
  final bool centerTitle;
  final Widget? leading;
  final List<Widget>? actions;
  final VoidCallback? onBackPressed;
  final bool automaticallyImplyLeading;
  const CustomAppBar({
    super.key,
    required this.title,
    this.titleStyle,
    this.backgroundColor = AppPalette.transparent,
    this.surfaceTintColor = AppPalette.transparent,
    this.elevation = 0,
    this.centerTitle = true,
    this.leading,
    this.actions,
    this.onBackPressed,
    this.automaticallyImplyLeading = true,
  });
  @override
  Widget build(BuildContext context) {
    return AppBar(
      backgroundColor: backgroundColor,
      surfaceTintColor: surfaceTintColor,
      elevation: elevation,
      centerTitle: centerTitle,
      automaticallyImplyLeading: automaticallyImplyLeading,
      title: title != null
          ? Text(title!, style: titleStyle ?? AppTypography.screenTitle)
          : null,
      leading: leading ??
          (automaticallyImplyLeading
              ? Padding(
                  padding: const EdgeInsets.only(left: 12.0),
                  child: Container(
                    decoration: BoxDecoration(
                      color: AppPalette.grey.grey0.withOpacity(0.3),
                      shape: BoxShape.circle,
                    ),
                    child: IconButton(
                      icon: const  Icon(
                        Icons.arrow_back_ios_new,
                        color: AppPalette.black,
                        size: 20,
                      ),
                      onPressed: onBackPressed ??
                          () {
                            Navigator.of(context).pop();
                          },
                    ),
                  ),
                )
              : null),
      actions: actions,
    );
  }

  @override
  Size get preferredSize => const Size.fromHeight(kToolbarHeight);
}
