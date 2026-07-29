import 'package:abc_androidapp/config/themes/app_palette.dart';
import 'package:flutter/material.dart';

class OptionButton extends StatefulWidget {
  final String label;
  final bool isSelected;
  // final VoidCallback onPressed;

  const OptionButton({
    super.key,
    required this.label,
    this.isSelected = false,
    // required this.onPressed,
  });

  @override
  _OptionButtonState createState() => _OptionButtonState();
}

class _OptionButtonState extends State<OptionButton> {
  @override
  void initState() {
    super.initState();
  }

  @override
  Widget build(BuildContext context) {
    return Column(
      mainAxisSize: MainAxisSize.min,
      children: [
        Container(
          width: 165,
          height: 55,
          decoration: ShapeDecoration(
            color: widget.isSelected
                ? AppPalette.blue.blue1
                : AppPalette.grey.grey1,
            shape: RoundedRectangleBorder(
              side: BorderSide(
                width: 2,
                color: widget.isSelected
                    ? AppPalette.blue.primary
                    : AppPalette.transparent,
              ),
              borderRadius: BorderRadius.circular(60),
            ),
          ),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            mainAxisAlignment: MainAxisAlignment.center,
            crossAxisAlignment: CrossAxisAlignment.center,
            children: [
              SizedBox(
                child: Text(
                  widget.label,
                  textAlign: TextAlign.center,
                  style: Theme.of(context).textTheme.titleMedium,
                  maxLines: 1,
                ),
              ),
            ],
          ),
        ),
      ],
    );
  }
}
