import 'package:flutter/material.dart';
import 'package:tdesign_flutter/tdesign_flutter.dart';

class CustomLoading extends StatelessWidget {
  final TDLoadingSize size;
  final String? text;

  const CustomLoading({
    Key? key,
    this.size = TDLoadingSize.large,
    this.text,
  }) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Center(
      child: TDLoading(
        size: size,
        text: text,
      ),
    );
  }
}
