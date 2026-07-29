enum ProductStatus {
  selling,
  unSelling,
}

extension ProductStatusExtension on ProductStatus {
  String get displayName {
    switch (this) {
      case ProductStatus.selling:
        return 'Selling';
      case ProductStatus.unSelling:
        return 'UnSelling';
    }
  }

  static ProductStatus? fromString(String value) {
    switch (value.toLowerCase()) {
      case 'Selling':
        return ProductStatus.selling;
      case 'UnSelling':
        return ProductStatus.unSelling;
      default:
        return null;
    }
  }
}
