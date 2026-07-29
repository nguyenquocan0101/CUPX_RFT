import 'package:abc_androidapp/app/data/models/menu.dart';

abstract class MenuRepository {
  Future<Menu> getMenuById(String menuId);
  Future<Menu> getMenu();
}
