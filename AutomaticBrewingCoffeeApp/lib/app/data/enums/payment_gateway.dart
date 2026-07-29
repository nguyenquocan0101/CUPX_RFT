enum PaymentGateway {
  vnpay,
  reso,
  mpos,
  atm;
  
  String get value {
    switch (this) {
      case PaymentGateway.vnpay:
        return 'VNPay';
      case PaymentGateway.mpos:
        return 'MPOS';
      case PaymentGateway.reso:
        return 'Reso';
      case PaymentGateway.atm:
        return 'ATM';
    }
  }
  
  static PaymentGateway fromString(String value) {
    switch (value.toLowerCase()) {
      case 'vnpay':
        return PaymentGateway.vnpay;
      case 'mpos':
        return PaymentGateway.mpos;
      case 'reso':
        return PaymentGateway.reso;
      case 'atm':
        return PaymentGateway.atm;
      default:
        return PaymentGateway.vnpay; // Default value
    }
  }
}
