import 'dart:async';
import 'package:flutter/material.dart';

// Interface để truy cập setEnableCallback
abstract class CountdownTimerController {
  void setEnableCallback(bool value);
}

class CountdownTimer extends StatefulWidget {
  final DateTime expiredDate;
  final VoidCallback? onExpired;
  final TextStyle? textStyle;
  final bool enableCallback;

  const CountdownTimer({
    Key? key,
    required this.expiredDate,
    this.onExpired,
    this.textStyle,
    this.enableCallback = true,
  }) : super(key: key);

  @override
  State<CountdownTimer> createState() => _CountdownTimerState();
}

class _CountdownTimerState extends State<CountdownTimer> implements CountdownTimerController {
  Timer? _timer;
  Duration _timeLeft = Duration.zero;
  bool _hasExpired = false;
  bool _enableCallback = true;

  @override
  void initState() {
    super.initState();
    _enableCallback = widget.enableCallback;
    _startTimer();
  }

  @override
  void dispose() {
    _timer?.cancel();
    super.dispose();
  }

  @override
  void setEnableCallback(bool value) {
    if (mounted) {
      setState(() {
        _enableCallback = value;
      });
    }
  }

  void _startTimer() {
    _updateTime();
    _timer = Timer.periodic(const Duration(seconds: 1), (_) => _updateTime());
  }

  void _updateTime() {
    if (!mounted) return;

    final now = DateTime.now();
    final difference = widget.expiredDate.difference(now);

    setState(() {
      if (difference.isNegative && !_hasExpired) {
        _timeLeft = Duration.zero;
        _hasExpired = true;
        _timer?.cancel();

        if (_enableCallback) {
          WidgetsBinding.instance.addPostFrameCallback((_) {
            if (mounted) widget.onExpired?.call();
          });
        }
      } else if (!difference.isNegative) {
        _timeLeft = difference;
      }
    });
  }

  String _formatTime() {
    final minutes = _timeLeft.inMinutes;
    final seconds = _timeLeft.inSeconds % 60;
    return "${minutes.toString().padLeft(2, '0')}:${seconds.toString().padLeft(2, '0')}";
  }

  @override
  Widget build(BuildContext context) {
    return Text(
      _formatTime(),
      style: widget.textStyle ??
          TextStyle(
            fontSize: 30,
            color: Colors.grey.shade800,
            fontWeight: FontWeight.w900,
          ),
    );
  }
}