import 'package:abc_androidapp/app/core/helpers/price_formatter.dart';
import 'package:flutter/widgets.dart';

class PriceRow extends StatelessWidget {
  final String label;
  final double amount;
  final bool isTotal;
  final double fem;
  final Color primaryColor;

  const PriceRow({
    super.key,
    required this.label,
    required this.amount,
    required this.fem,
    required this.primaryColor,
    this.isTotal = false,
  });

  @override
  Widget build(BuildContext context) {
    return Row(
      mainAxisAlignment: MainAxisAlignment.spaceBetween,
      children: [
        Text(
          label,
          style: TextStyle(
            fontSize: 14 * fem,
            fontWeight: isTotal ? FontWeight.w600 : FontWeight.normal,
          ),
        ),
        Text(
          formatPrice(amount),
          style: TextStyle(
            fontSize: isTotal ? 16 * fem : 14 * fem,
            fontWeight: isTotal ? FontWeight.w600 : FontWeight.normal,
            color: isTotal ? primaryColor : null,
          ),
        ),
      ],
    );
  }
}