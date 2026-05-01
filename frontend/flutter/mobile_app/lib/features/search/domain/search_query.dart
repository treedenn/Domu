class SearchQuery {
  const SearchQuery({
    required this.text,
    this.expiringWithinDays,
  });

  final String text;
  final int? expiringWithinDays;
}
