class BaseQuery {
  String? filterBy;
  String? filterQuery;
  int page;
  int size;
  String? sortBy;
  bool isAsc;

  BaseQuery({
    this.filterBy,
    this.filterQuery,
    this.page = 1,
    this.size = 10,
    this.sortBy,
    this.isAsc = true,
  });

  Map<String, dynamic> toMap() {
    return {
      if (filterBy != null) 'filterBy': filterBy,
      if (filterQuery != null) 'filterQuery': filterQuery,
      'page': page,
      'size': size,
      if (sortBy != null) 'sortBy': sortBy,
      'isAsc': isAsc,
    };
  }

  String toParameterString() {
    final map = toMap();
    return map.entries
        .map((entry) =>
            '${Uri.encodeQueryComponent(entry.key)}=${Uri.encodeQueryComponent(entry.value.toString())}')
        .join('&');
  }
}
