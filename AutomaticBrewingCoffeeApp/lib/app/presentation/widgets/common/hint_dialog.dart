import 'package:abc_androidapp/config/constants/animation_path.dart';
import 'package:flutter/material.dart';
import 'package:lottie/lottie.dart';

class HintDialog {
  static void show(
    BuildContext context,
    String title,
    String message, {
    bool isSuccess = true,
  }) {
    showDialog(
      context: context,
      barrierDismissible: true,
      builder: (BuildContext dialogContext) {
        return _AutoDismissDialog(
          title: title,
          message: message,
          isSuccess: isSuccess,
        );
      },
    );
  }
}

class _AutoDismissDialog extends StatefulWidget {
  final String title;
  final String message;
  final bool isSuccess;

  const _AutoDismissDialog({
    required this.title,
    required this.message,
    required this.isSuccess,
  });

  @override
  State<_AutoDismissDialog> createState() => _AutoDismissDialogState();
}

class _AutoDismissDialogState extends State<_AutoDismissDialog> {
  @override
  void initState() {
    super.initState();
    int seconds = widget.isSuccess ? 4 : 30;
    Future.delayed(Duration(seconds: seconds), () {
      if (mounted && Navigator.of(context).canPop()) {
        Navigator.of(context).pop();
      }
    });
  }

  List<TextSpan> _parseTextWithBold(String text, TextStyle baseStyle) {
    final List<TextSpan> spans = [];
    final RegExp boldRegex = RegExp(r'\*\*(.*?)\*\*');
    int lastEnd = 0;

    for (final Match match in boldRegex.allMatches(text)) {
      // Thêm text thường trước bold text
      if (match.start > lastEnd) {
        spans.add(TextSpan(
          text: text.substring(lastEnd, match.start),
          style: baseStyle,
        ));
      }

      // Thêm bold text
      spans.add(TextSpan(
        text: match.group(1) ?? '',
        style: baseStyle.copyWith(fontWeight: FontWeight.bold),
      ));

      lastEnd = match.end;
    }

    // Thêm text còn lại sau bold text cuối cùng
    if (lastEnd < text.length) {
      spans.add(TextSpan(
        text: text.substring(lastEnd),
        style: baseStyle,
      ));
    }

    return spans;
  }

  Widget _buildMessageContent() {
    // Tách message theo ký tự \n
    final List<String> messageParts = widget.message.split('\n');

    if (messageParts.length == 1) {
      // Nếu chỉ có 1 dòng, hiển thị như cũ
      return Text(
        widget.message,
        style: const TextStyle(
          fontSize: 14,
          color: Colors.black54,
        ),
        textAlign: TextAlign.center,
      );
    }

    // Nếu có nhiều dòng, xử lý từng dòng
    return Column(
      mainAxisSize: MainAxisSize.min,
      children: messageParts.asMap().entries.map((entry) {
        final int index = entry.key;
        final String part = entry.value;

        return RichText(
          textAlign: TextAlign.center,
          text: TextSpan(
            children: _parseTextWithBold(
              part,
              const TextStyle(
                fontSize: 14,
                color: Colors.black54,
              ),
            ),
          ),
        );
      }).toList(),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Dialog(
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(16),
      ),
      insetPadding: const EdgeInsets.symmetric(horizontal: 24),
      child: Padding(
        padding: const EdgeInsets.all(20),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Lottie.asset(
              widget.isSuccess ? AnimationPath.checked : AnimationPath.fail,
              width: 200,
              height: 200,
              repeat: false,
              frameRate: FrameRate.max,
            ),
            const SizedBox(height: 16),
            Column(
              mainAxisSize: MainAxisSize.min,
              crossAxisAlignment: CrossAxisAlignment.center,
              children: [
                Text(
                  widget.title,
                  style: const TextStyle(
                    fontSize: 16,
                    fontWeight: FontWeight.w600,
                  ),
                  textAlign: TextAlign.center,
                ),
                const SizedBox(height: 4),
                _buildMessageContent(),
              ],
            ),
          ],
        ),
      ),
    );
  }
}
