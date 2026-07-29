
import 'package:abc_androidapp/app/data/models/organization/kiosk.dart';
import 'package:abc_androidapp/app/data/models/organization/update_ingredient_request.dart';

abstract class KioskRepository {
  Future<Kiosk> getKioskInfo();
  Future<bool> updateIngredient(UpdateIngredientRequest request);
}
