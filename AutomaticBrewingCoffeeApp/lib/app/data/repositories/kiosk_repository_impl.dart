import 'package:abc_androidapp/app/data/datasources/kiosk_datasource.dart';
import 'package:abc_androidapp/app/data/models/organization/kiosk.dart';
import 'package:abc_androidapp/app/data/models/organization/update_ingredient_request.dart';
import 'package:abc_androidapp/app/domain/repositories/kiosk_repository.dart';

class KioskRepositoryImpl extends KioskRepository {
  final KioskDatasource kioskDatasource;

  KioskRepositoryImpl({required this.kioskDatasource});

  @override
  Future<Kiosk> getKioskInfo() async {
    var result = await kioskDatasource.getKioskInfo();
    return result.responseRequest!;
  }

  @override
  Future<bool> updateIngredient(UpdateIngredientRequest request) async {
    var result = await kioskDatasource.updateIngredient(request);
    return result.responseRequest!;
  }
}
