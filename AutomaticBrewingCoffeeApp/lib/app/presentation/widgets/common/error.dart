import 'package:abc_androidapp/config/constants/animation_path.dart';
import 'package:abc_androidapp/config/themes/app_palette.dart';
import 'package:flutter/material.dart';
import 'package:flutter/widgets.dart';
import 'package:get/get.dart';
import 'package:lottie/lottie.dart';

class CustomError extends StatefulWidget {
  final String title;
  final String subTitle;
  final VoidCallback? onback;
  const CustomError({super.key, required this.title, required this.subTitle, this.onback});

  @override
  State<CustomError> createState() => _CustomErrorState();
}

class _CustomErrorState extends State<CustomError> {
  @override
  Widget build(BuildContext context) {
    return Center(
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Lottie.asset(
            AnimationPath.error,
            width: 200,
            height: 200,
            repeat: false,
            frameRate: FrameRate.max,
          ),
          const SizedBox(height: 24),
          Text(
            widget.title,
            style: const TextStyle(
              fontSize: 20,
              fontWeight: FontWeight.w600,
              color: AppPalette.black,
            ),
          ),
          const SizedBox(height: 12),
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 40),
            child: Text(
              widget.subTitle,
              textAlign: TextAlign.center,
              style: TextStyle(
                color: Colors.grey.shade600,
                fontSize: 15,
              ),
            ),
          ),
          const SizedBox(height: 32),
          ElevatedButton(
            onPressed: widget.onback ?? () {
              Get.back();
            },
            style: ElevatedButton.styleFrom(
              backgroundColor: const Color(0xFF57B7E7),
              padding: const EdgeInsets.symmetric(
                horizontal: 32,
                vertical: 12,
              ),
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(8),
              ),
            ),
            child: const Text(
              'Quay lại',
              style: TextStyle(fontSize: 16, color: AppPalette.white),
            ),
          ),
        ],
      ),
    );
  }
}