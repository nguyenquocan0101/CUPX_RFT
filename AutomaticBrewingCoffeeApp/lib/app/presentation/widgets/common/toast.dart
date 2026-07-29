import 'package:flutter/material.dart';

enum ToastType {
  success,
  error,
  warning,
  info,
}

class CustomToast {
  static OverlayEntry? _currentToast;
  
  static void show({
    required BuildContext context,
    required String message,
    ToastType type = ToastType.info,
    Duration duration = const Duration(seconds: 3),
    VoidCallback? onDismiss,
  }) {
    _currentToast?.remove();
    _currentToast = null;
    
    final Color backgroundColor;
    final Color textColor;
    final IconData icon;

    switch (type) {
      case ToastType.success:
        backgroundColor = const Color(0xFFE7F7ED);
        textColor = const Color(0xFF2E7D32);
        icon = Icons.check_circle;
        break;
      case ToastType.error:
        backgroundColor = const Color(0xFFFEEFEF);
        textColor = Colors.red.shade700;
        icon = Icons.error;
        break;
      case ToastType.warning:
        backgroundColor = const Color(0xFFFFF9E5);
        textColor = const Color(0xFFEEA23D);
        icon = Icons.warning;
        break;
      case ToastType.info:
        backgroundColor = const Color(0xFFE6F5FC);
        textColor = const Color(0xFF57B7E7);
        icon = Icons.info;
        break;
    }

    // Tạo overlay để hiển thị toast
    final overlay = Overlay.of(context);
    final toast = OverlayEntry(
      builder: (context) => _ToastOverlay(
        message: message,
        backgroundColor: backgroundColor,
        textColor: textColor,
        icon: icon,
        duration: duration,
        onDismiss: () {
          _currentToast?.remove();
          _currentToast = null;
          if (onDismiss != null) onDismiss();
        },
      ),
    );
    
    _currentToast = toast;
    overlay.insert(toast);
  }

  static void showSuccess(BuildContext context, String message) {
    show(context: context, message: message, type: ToastType.success);
  }
  
  static void showError(BuildContext context, String message) {
    show(context: context, message: message, type: ToastType.error);
  }
  
  static void showWarning(BuildContext context, String message) {
    show(context: context, message: message, type: ToastType.warning);
  }
  
  static void showInfo(BuildContext context, String message) {
    show(context: context, message: message, type: ToastType.info);
  }
}

class _ToastOverlay extends StatefulWidget {
  final String message;
  final Color backgroundColor;
  final Color textColor;
  final IconData icon;
  final Duration duration;
  final VoidCallback onDismiss;

  const _ToastOverlay({
    required this.message,
    required this.backgroundColor,
    required this.textColor,
    required this.icon,
    required this.duration,
    required this.onDismiss,
  });

  @override
  State<_ToastOverlay> createState() => _ToastOverlayState();
}

class _ToastOverlayState extends State<_ToastOverlay> with SingleTickerProviderStateMixin {
  late AnimationController _controller;
  late Animation<double> _animation;

  @override
  void initState() {
    super.initState();
    
    _controller = AnimationController(
      vsync: this,
      duration: const Duration(milliseconds: 300),
    );
    
    _animation = CurvedAnimation(
      parent: _controller,
      curve: Curves.easeInOut,
    );
    
    _controller.forward();
    
    Future.delayed(widget.duration, () {
      if (mounted) {
        _dismissToast();
      }
    });
  }

  void _dismissToast() {
    _controller.reverse().then((_) {
      widget.onDismiss();
    });
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Positioned(
      top: 16, // Khoảng cách từ top
      right: 16, // Khoảng cách từ right
      child: Material(
        color: Colors.transparent,
        child: FadeTransition(
          opacity: _animation,
          child: SlideTransition(
            position: Tween<Offset>(
              begin: const Offset(1.0, 0.0), // Trượt từ phải sang
              end: Offset.zero,
            ).animate(_animation),
            child: Container(
              constraints: BoxConstraints(
                maxWidth: MediaQuery.of(context).size.width * 0.4, // Giới hạn chiều rộng tối đa
              ),
              decoration: BoxDecoration(
                color: widget.backgroundColor,
                borderRadius: BorderRadius.circular(12),
                boxShadow: [
                  BoxShadow(
                    color: Colors.black.withOpacity(0.1),
                    blurRadius: 10,
                    offset: const Offset(0, 4),
                  ),
                ],
              ),
              padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
              child: IntrinsicWidth(
                child: Row(
                  children: [
                    Icon(widget.icon, color: widget.textColor, size: 24),
                    const SizedBox(width: 16),
                    Expanded(
                      child: Text(
                        widget.message,
                        style: TextStyle(
                          color: widget.textColor,
                          fontSize: 20,
                          fontWeight: FontWeight.w500,
                        ),
                      ),
                    ),
                    const SizedBox(width: 8),
                    GestureDetector(
                      onTap: _dismissToast,
                      child: Icon(
                        Icons.close,
                        color: widget.textColor,
                        size: 20,
                      ),
                    ),
                  ],
                ),
              ),
            ),
          ),
        ),
      ),
    );
  }
}