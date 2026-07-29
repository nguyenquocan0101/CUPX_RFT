// Models
import 'package:abc_androidapp/config/constants/image_path.dart';

class Category {
  String id;
  String code;
  String name;
  String type;
  int displayOrder;
  String description;
  String? picUrl;

  Category({
    required this.id,
    required this.code,
    required this.name,
    required this.type,
    required this.displayOrder,
    required this.description,
    this.picUrl,
  });

  factory Category.fromJson(Map<String, dynamic> json) {
    return Category(
      id: json['id'] ?? '',
      code: json['code'] ?? '',
      name: json['name'] ?? '',
      type: json['type'] ?? '',
      displayOrder: json['displayOrder'] is int
          ? json['displayOrder']
          : int.tryParse(json['displayOrder'].toString()) ?? 0,
      description: json['description'] ?? '',
      picUrl: json['picUrl'], // Có thể null, nên giữ nguyên
    );
  }

  Map<String, dynamic> toJson() {
    final Map<String, dynamic> data = <String, dynamic>{};
    data['id'] = id;
    data['code'] = code;
    data['name'] = name;
    data['type'] = type;
    data['displayOrder'] = displayOrder;
    data['description'] = description;
    data['picUrl'] = picUrl;
    return data;
  }
}

// Temporary function to create dummy categories for testing
List<Category> getDummyCategories() {
  return [
    Category(
      id: '1',
      code: 'CAT001',
      name: 'Coffee',
      type: 'Normal',
      displayOrder: 1,
      description: 'Delicious coffee selections',
      picUrl: ImagePath.greyOutlineCupIcon,
    ),
    Category(
      id: '2',
      code: 'CAT002',
      name: 'Latte',
      type: 'Normal',
      displayOrder: 2,
      description: 'Refreshing tea choices',
      picUrl: ImagePath.greyOutlineCupIcon, // Replace with tea icon
    ),
    Category(
      id: '3',
      code: 'CAT003',
      name: 'Expresso',
      type: 'Normal',
      displayOrder: 3,
      description: 'Fresh and fruity juices',
      picUrl: ImagePath.greyOutlineCupIcon, // Replace with juice icon
    ),
    // Category(
    //   id: '4',
    //   code: 'CAT004',
    //   name: 'Smoothies',
    //   type: 'Normal',
    //   displayOrder: 4,
    //   description: 'Creamy and nutritious smoothies',
    //   picUrl: ImagePath.greyOutlineCupIcon, // Replace with smoothie icon
    // ),
    // Category(
    //   id: '5',
    //   code: 'CAT005',
    //   name: 'Snacks',
    //   type: 'Extra',
    //   displayOrder: 5,
    //   description: 'Tasty snacks to complement your drink',
    //   picUrl: ImagePath.greyOutlineCupIcon, // Replace with snack icon
    // ),
  ];
}
