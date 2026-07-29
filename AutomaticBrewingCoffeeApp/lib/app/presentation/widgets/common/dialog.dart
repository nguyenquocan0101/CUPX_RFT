import 'package:abc_androidapp/config/constants/size_config.dart';
import 'package:flutter/material.dart';


enum DialogType {
  error,
  success,
  warning,
  info,
}

class DialogConfig {
  final String title;
  final Color backgroundColor;
  final Color iconColor;
  final Color buttonColor;
  final IconData icon;

  DialogConfig({
    required this.title,
    required this.backgroundColor,
    required this.iconColor,
    required this.buttonColor,
    required this.icon,
  });
}

void showCustomDialog({
  required BuildContext context,
  required String message,
  DialogType type = DialogType.error,
  VoidCallback? onConfirm,
  String? confirmButtonText,
  VoidCallback? onCancel,
  String? cancelButtonText,
  bool barrierDismissible = true,
}) {
  // Define configurations for different dialog types
  final Map<DialogType, DialogConfig> configs = {
    DialogType.error: DialogConfig(
      title: 'Lỗi',
      backgroundColor: Colors.red.shade50,
      iconColor: Colors.red.shade700,
      buttonColor: Colors.red.shade700,
      icon: Icons.error_outline,
    ),
    DialogType.success: DialogConfig(
      title: 'Thành công',
      backgroundColor: Colors.green.shade50,
      iconColor: Colors.green.shade700,
      buttonColor: Colors.green.shade700,
      icon: Icons.check_circle_outline,
    ),
    DialogType.warning: DialogConfig(
      title: 'Cảnh báo',
      backgroundColor: Colors.orange.shade50,
      iconColor: Colors.orange.shade700,
      buttonColor: Colors.orange.shade700,
      icon: Icons.warning_amber_outlined,
    ),
    DialogType.info: DialogConfig(
      title: 'Thông tin',
      backgroundColor: Colors.lightBlue.shade50,
      iconColor: Colors.lightBlue.shade700,
      buttonColor: Colors.lightBlue.shade700,
      icon: Icons.info_outline,
    ),
  };

  showDialog(
    context: context,
    barrierDismissible: barrierDismissible,
    builder: (context) => CustomDialog(
      message: message,
      config: configs[type]!,
      onConfirm: onConfirm,
      confirmButtonText: confirmButtonText ?? 'Đóng',
      onCancel: onCancel,
      cancelButtonText: cancelButtonText,
    ),
  );
}

class CustomDialog extends StatelessWidget {
  final String message;
  final DialogConfig config;
  final VoidCallback? onConfirm;
  final String confirmButtonText;
  final VoidCallback? onCancel;
  final String? cancelButtonText;

  const CustomDialog({
    Key? key,
    required this.message,
    required this.config,
    this.onConfirm,
    required this.confirmButtonText,
    this.onCancel,
    this.cancelButtonText,
  }) : super(key: key);

  @override
  Widget build(BuildContext context) {
    SizeConfig.init(context);
    return Dialog(
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(16 * SizeConfig.fem),
      ),
      elevation: 0,
      backgroundColor: Colors.transparent,
      child: ContentBox(
        message: message,
        config: config,
        onConfirm: onConfirm,
        confirmButtonText: confirmButtonText,
        onCancel: onCancel,
        cancelButtonText: cancelButtonText,
      ),
    );
  }
}

class ContentBox extends StatelessWidget {
  final String message;
  final DialogConfig config;
  final VoidCallback? onConfirm;
  final String confirmButtonText;
  final VoidCallback? onCancel;
  final String? cancelButtonText;

  const ContentBox({
    Key? key,
    required this.message,
    required this.config,
    this.onConfirm,
    required this.confirmButtonText,
    this.onCancel,
    this.cancelButtonText,
  }) : super(key: key);

  @override
  Widget build(BuildContext context) {
    final screenWidth = MediaQuery.of(context).size.width;
    final fem = SizeConfig.fem;
    final hem = SizeConfig.hem;
    final ffem = SizeConfig.ffem;

    return Container(
      width: screenWidth * 0.7,
      padding: EdgeInsets.all(20 * fem),
      decoration: BoxDecoration(
        shape: BoxShape.rectangle,
        color: Colors.white,
        borderRadius: BorderRadius.circular(16 * fem),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.2),
            offset: Offset(0, 10 * hem),
            blurRadius: 10 * fem,
          ),
        ],
      ),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: <Widget>[
          Row(
            children: [
              Container(
                padding: EdgeInsets.all(8 * fem),
                decoration: BoxDecoration(
                  color: config.backgroundColor,
                  shape: BoxShape.circle,
                ),
                child: Icon(
                  config.icon,
                  color: config.iconColor,
                  size: 32 * fem,
                ),
              ),
              SizedBox(width: 16 * fem),
              Text(
                config.title,
                style: TextStyle(
                  fontSize: 20 * ffem,
                  fontWeight: FontWeight.w600,
                  color: config.iconColor,
                ),
              ),
            ],
          ),
          SizedBox(height: 20 * hem),
          Text(
            message,
            style: TextStyle(
              fontSize: 16 * ffem,
              color: Colors.black87,
            ),
            textAlign: TextAlign.center,
          ),
          SizedBox(height: 24 * hem),
          Row(
            mainAxisAlignment: onCancel != null
                ? MainAxisAlignment.spaceBetween
                : MainAxisAlignment.end,
            children: [

// Sửa phần nút Cancel trong widget ContentBox
              if (onCancel != null)
                TextButton(
                  onPressed: () {
                    // Chỉ đóng dialog, không gọi hàm onCancel
                    Navigator.pop(context);
                  },
                  child: Text(
                    cancelButtonText ?? 'Hủy',
                    style: TextStyle(
                      fontSize: 16 * ffem,
                      color: Colors.grey.shade700,
                    ),
                  ),
                ),
              ElevatedButton(
                style: ElevatedButton.styleFrom(
                  backgroundColor: config.buttonColor,
                  foregroundColor: Colors.white,
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(30 * fem),
                  ),
                  padding: EdgeInsets.symmetric(
                    horizontal: 20 * fem,
                    vertical: 10 * hem,
                  ),
                ),
                onPressed: () {
                  Navigator.pop(context);
                  onConfirm?.call();
                },
                child: Text(
                  confirmButtonText,
                  style: TextStyle(fontSize: 16 * ffem),
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }
}