enum DeviceType {
  cupDroppingMachine,
  coffeeBrewingMachine,
  iceMakerMachine,
  roboticArm,
}

extension DeviceTypeExtension on DeviceType {
  String get displayName {
    switch (this) {
      case DeviceType.cupDroppingMachine:
        return 'Cup Dropping Machine';
      case DeviceType.coffeeBrewingMachine:
        return 'Coffee Brewing Machine';
      case DeviceType.iceMakerMachine:
        return 'Ice Maker Machine';
      case DeviceType.roboticArm:
        return 'Robotic Arm';
    }
  }

  static DeviceType? fromString(String value) {
    switch (value.toLowerCase()) {
      case 'cup dropping machine':
        return DeviceType.cupDroppingMachine;
      case 'coffee brewing machine':
        return DeviceType.coffeeBrewingMachine;
      case 'ice maker machine':
        return DeviceType.iceMakerMachine;
      case 'robotic arm':
        return DeviceType.roboticArm;
      default:
        return null;
    }
  }
}
