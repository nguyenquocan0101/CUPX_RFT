import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:tdesign_flutter/tdesign_flutter.dart';

class AdminAuthDialog extends StatefulWidget {
  final String adminPassword;
  final VoidCallback onSuccess;

  const AdminAuthDialog({
    super.key,
    required this.adminPassword,
    required this.onSuccess,
  });

  @override
  State<AdminAuthDialog> createState() => _AdminAuthDialogState();
}

class _AdminAuthDialogState extends State<AdminAuthDialog> {
  final TextEditingController _pinController = TextEditingController();
  final FocusNode _pinFocusNode = FocusNode();

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      _pinFocusNode.requestFocus();
    });
  }

  @override
  void dispose() {
    _pinController.dispose();
    _pinFocusNode.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      backgroundColor: Colors.white,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(20)),
      title: const Text(
        'Xác thực quản trị viên',
        style: TextStyle(
          fontSize: 22,
          fontWeight: FontWeight.bold,
          color: Color(0xFF57B7E7),
        ),
      ),
      content: Column(
        mainAxisSize: MainAxisSize.min,
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Text(
            'Vui lòng nhập mã PIN 6 số để truy cập cài đặt',
            style: TextStyle(fontSize: 16, height: 1.5),
          ),
          const SizedBox(height: 24),
          TextField(
            controller: _pinController,
            focusNode: _pinFocusNode,
            decoration: InputDecoration(
              hintText: 'Nhập mã PIN',
              prefixIcon: const Icon(TDIcons.lock_on, color: Color(0xFF57B7E7)),
              border: OutlineInputBorder(
                borderRadius: BorderRadius.circular(16),
                borderSide: BorderSide(color: Colors.grey.shade300, width: 1),
              ),
              enabledBorder: OutlineInputBorder(
                borderRadius: BorderRadius.circular(16),
                borderSide: BorderSide(color: Colors.grey.shade300, width: 1),
              ),
              focusedBorder: OutlineInputBorder(
                borderRadius: BorderRadius.circular(16),
                borderSide: const BorderSide(color: Color(0xFF57B7E7), width: 2),
              ),
              filled: true,
              fillColor: Colors.white,
              contentPadding: const EdgeInsets.symmetric(vertical: 20),
            ),
            keyboardType: TextInputType.number,
            inputFormatters: [
              FilteringTextInputFormatter.digitsOnly,
              LengthLimitingTextInputFormatter(6),
            ],
            obscureText: true,
            textAlign: TextAlign.center,
            style: const TextStyle(
              fontSize: 24,
              letterSpacing: 3,
              fontWeight: FontWeight.bold,
            ),
          ),
        ],
      ),
      actions: [
        TextButton(
          onPressed: () {
            Navigator.of(context).pop();
            Navigator.of(context).pop();
          },
          style: TextButton.styleFrom(
            padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 12),
            shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(14)),
          ),
          child: const Text(
            'Hủy',
            style: TextStyle(fontSize: 16, color: Colors.grey),
          ),
        ),
        ElevatedButton(
          onPressed: () => _handleAuthentication(context),
          style: ElevatedButton.styleFrom(
            backgroundColor: const Color(0xFF57B7E7),
            foregroundColor: Colors.white,
            padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 14),
            shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(14)),
          ),
          child: const Text('Xác nhận', style: TextStyle(fontSize: 16)),
        ),
      ],
      actionsPadding: const EdgeInsets.fromLTRB(16, 0, 16, 16),
    );
  }

  void _handleAuthentication(BuildContext context) {
    if (_pinController.text == widget.adminPassword) {
      Navigator.of(context).pop();
      widget.onSuccess();
    } else {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Mã PIN không chính xác'),
          backgroundColor: Colors.red,
        ),
      );
      _pinController.clear();
      _pinFocusNode.requestFocus();
    }
  }
}