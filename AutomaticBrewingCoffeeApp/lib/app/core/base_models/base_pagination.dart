class Pagination<TResult> {
  int size;
  int page;
  int total;
  int totalPages;
  List<TResult> items;

  Pagination({
    required this.size,
    required this.page,
    required this.total,
    required this.totalPages,
    required this.items,
  });

  factory Pagination.empty() {
    return Pagination<TResult>(
      size: 0,
      page: 0,
      total: 0,
      totalPages: 0,
      items: [],
    );
  }

  factory Pagination.fromSource(
    List<TResult> source,
    int page,
    int size,
    int firstPage,
  ) {
    if (firstPage > page) {
      throw ArgumentError(
        'Page ($page) must be greater or equal than firstPage ($firstPage)',
      );
    }

    final total = source.length;
    final totalPages = (total / size).ceil();
    final items = source.skip((page - firstPage) * size).take(size).toList();

    return Pagination<TResult>(
      size: size,
      page: page,
      total: total,
      totalPages: totalPages,
      items: items,
    );
  }

  factory Pagination.fromJson({
    required Map<String, dynamic> json,
    required TResult Function(Map<String, dynamic>) fromJsonItem,
  }) {
    final itemsJson = json['items'] as List<dynamic>;
    List<TResult> items = itemsJson
        .map((item) => fromJsonItem(item as Map<String, dynamic>))
        .toList();

    return Pagination<TResult>(
      size: json['size'],
      page: json['page'],
      total: json['total'],
      totalPages: json['totalPages'],
      items: items,
    );
  }
}
