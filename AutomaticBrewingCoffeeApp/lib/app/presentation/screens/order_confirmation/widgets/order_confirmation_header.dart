import 'package:abc_androidapp/app/core/helpers/time_formatter.dart';
import 'package:abc_androidapp/config/themes/app_palette.dart';
import 'package:flutter/material.dart';
import 'dart:async';

class OrderConfirmationHeader extends StatefulWidget {
  final double fem;
  final String address;

  const OrderConfirmationHeader({
    super.key,
    required this.fem,
    required this.address,
  });

  @override
  State<OrderConfirmationHeader> createState() => _OrderConfirmationHeaderState();
}

class _OrderConfirmationHeaderState extends State<OrderConfirmationHeader> {
  late Timer _timer;
  late String _currentTime;

  @override
  void initState() {
    super.initState();
    _updateTime();

    _timer = Timer.periodic(const Duration(seconds: 2), (timer) {
      _updateTime();
    });
  }

  void _updateTime() {
    final now = DateTime.now();
    setState(() {
      _currentTime = formatOnlyTime(now.toString());
    });
  }

  @override
  void dispose() {
    _timer.cancel(); 
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Column(
      children: [
        Row(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Icon(
              Icons.location_on_outlined,
              color: AppPalette.blue.primary.withOpacity(0.95),
              size: 24 * widget.fem,
            ),
            SizedBox(width: 8 * widget.fem),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  RichText(
                    text: TextSpan(
                      style: TextStyle(
                        fontSize: 16 * widget.fem,
                        height: 1.5,
                        fontWeight: FontWeight.w300,
                        color: AppPalette.blue.primary.withOpacity(0.95),
                      ),
                      children: [
                        TextSpan(
                          text: widget.address,
                          style: const TextStyle(
                            fontWeight: FontWeight.w600,
                          ),
                        ),
                      ],
                    ),
                  ),
                ],
              ),
            ),
          ],
        ),
        SizedBox(height: 16 * widget.fem),
        Row(
          crossAxisAlignment: CrossAxisAlignment.center,
          children: [
            Icon(
              Icons.access_time,
              color: AppPalette.blue.primary.withOpacity(0.95),
              size: 24 * widget.fem,
            ),
            SizedBox(width: 8 * widget.fem),
            RichText(
              text: TextSpan(
                style: TextStyle(
                  fontSize: 16 * widget.fem,
                  color: Colors.black87,
                ),
                children: [
                  TextSpan(
                    text: 'Thời gian xác nhận: ',
                    style: TextStyle(
                      fontWeight: FontWeight.w400,
                      color: AppPalette.blue.primary.withOpacity(0.95)
                    ),
                  ),
                  TextSpan(
                    text: _currentTime, 
                    style: TextStyle(
                      fontWeight: FontWeight.w600,
                      color: AppPalette.blue.primary.withOpacity(0.95),
                    ),
                  ),
                ],
              ),
            ),
          ],
        ),
      ],
    );
  }
}