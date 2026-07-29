import 'package:abc_androidapp/app/data/datasources/menu_datasource.dart';
import 'package:abc_androidapp/app/data/models/menu.dart';
import 'package:abc_androidapp/app/domain/repositories/menu_repository.dart';

class MenuRepositoryImpl extends MenuRepository {
  final MenuDatasource menuDatasource;

  MenuRepositoryImpl({required this.menuDatasource});
  @override
  Future<Menu> getMenuById(String menuId) async {
    var result = await menuDatasource.getMenuById(menuId);
    return result.response!;
  }

  @override
  Future<Menu> getMenu() async {
    var result = await menuDatasource.getMenu();
    return result.response!;
  }
}
