import 'dart:async';
import 'package:flutter/material.dart';

class BannerAppBar extends StatefulWidget implements PreferredSizeWidget {
  final List<String> imagePaths;
  final double aspectRatio;
  final Duration autoPlayDuration;

  const BannerAppBar({
    super.key,
    required this.imagePaths,
    this.aspectRatio = 16 / 10,
    this.autoPlayDuration = const Duration(seconds: 3),
  });

  @override
  State<BannerAppBar> createState() => _BannerAppBarState();

    @override
  Size get preferredSize {
    return const Size.fromHeight(230.0);
  }

}

class _BannerAppBarState extends State<BannerAppBar> {
  late final PageController _pageController;
  Timer? _timer;
  int _currentPage = 0;

  @override
  void initState() {
    super.initState();
    _pageController = PageController();
    if (widget.imagePaths.length > 1) {
      _startTimer();
    }
  }

  @override
  void dispose() {
    _timer?.cancel();
    _pageController.dispose();
    super.dispose();
  }

  void _startTimer() {
    _timer = Timer.periodic(widget.autoPlayDuration, (timer) {
      if (widget.imagePaths.isNotEmpty) {
        _currentPage = (_currentPage + 1) % widget.imagePaths.length;
        _pageController.animateToPage(
          _currentPage,
          duration: const Duration(milliseconds: 3000),
          curve: Curves.easeInOut,
        );
      }
    });
  }

  @override
  Widget build(BuildContext context) {
    return AppBar(
      backgroundColor: Colors.transparent,
      elevation: 0,
      flexibleSpace: AspectRatio(
        aspectRatio: widget.aspectRatio,
        child: PageView.builder(
          controller: _pageController,
          itemCount: widget.imagePaths.length,
          onPageChanged: (index) {
            setState(() {
              _currentPage = index;
            });
          },
          itemBuilder: (context, index) {
            return Image.asset(
              widget.imagePaths[index],
              fit: BoxFit.cover,
            );
          },
        ),
      ),
    );
  }
}