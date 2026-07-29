import 'package:abc_androidapp/config/themes/app_palette.dart';
import 'package:flutter/material.dart';

class BottomCartBar extends StatelessWidget {
  @override
  Widget build(BuildContext context) {
    return Container(
      width: double.infinity,
      height: 120,
      decoration: const BoxDecoration(
        color: Color(0xFFFAFDFC),
        boxShadow: [
          BoxShadow(
            color: Color(0x3F979696),
            blurRadius: 16,
            offset: Offset(0, -5),
            spreadRadius: 0,
          )
        ],
      ),
      child: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 16.0),
        child: Row(
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          children: [
            Text(
              'Tổng \$0.00',
              textAlign: TextAlign.center,
              style: Theme.of(context).textTheme.labelLarge,
            ),
            Container(
              padding: const EdgeInsets.symmetric(horizontal: 64, vertical: 16),
              decoration: ShapeDecoration(
                color: AppPalette.blue.primary,
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(72),
                ),
              ),
              child: Text(
                'Xem đơn hàng (0)',
                textAlign: TextAlign.center,
                style: Theme.of(context).textTheme.labelLarge,
              ),
            ),
          ],
        ),
      ),
    );
  }
}
