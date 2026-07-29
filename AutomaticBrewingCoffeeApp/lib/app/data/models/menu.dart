import 'package:abc_androidapp/app/data/models/menu_product_mapping.dart';

class Menu {
  final String menuId;
  final String name;
  final String? description;
  final String status;
  final List<MenuProductMapping> productsInMenu;

  Menu({
    required this.menuId,
    required this.name,
    this.description,
    required this.status,
    required this.productsInMenu,
  });

  factory Menu.fromJson(Map<String, dynamic> json) {
    return Menu(
      menuId: json['menuId'] as String,
      name: json['name'] as String,
      description: json['description'],
      status: json['status'] as String,
      productsInMenu: (json['menuProductMappings'] as List<dynamic>)
          .map((e) => MenuProductMapping.fromJson(e))
          .toList(),
    );
  }
}
