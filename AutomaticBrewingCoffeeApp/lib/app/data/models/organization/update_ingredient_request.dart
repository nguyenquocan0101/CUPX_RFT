import 'package:abc_androidapp/app/data/models/organization/kiosk.dart';

class UpdateIngredientRequest {
  final String deviceIngredientStateId;
  final int warningPercent;
  final int currentCapacity;
  final bool isWarning;
  final bool isRenewable;
  final bool isPrimary;

  UpdateIngredientRequest({
    required this.deviceIngredientStateId,
    required this.warningPercent,
    required this.currentCapacity,
    required this.isWarning,
    required this.isRenewable,
    required this.isPrimary,
  });

  // Convert to JSON for API call (excluding deviceIngredientStateId since it goes in URL)
  Map<String, dynamic> toJson() {
    return {
      'warningPercent': warningPercent,
      'currentCapacity': currentCapacity,
      'isWarning': isWarning,
      'isRenewable': isRenewable,
      'isPrimary': isPrimary,
    };
  }

  // Full JSON including deviceIngredientStateId if needed
  Map<String, dynamic> toFullJson() {
    return {
      'deviceIngredientStateId': deviceIngredientStateId,
      'warningPercent': warningPercent,
      'currentCapacity': currentCapacity,
      'isWarning': isWarning,
      'isRenewable': isRenewable,
      'isPrimary': isPrimary,
    };
  }

  factory UpdateIngredientRequest.fromIngredientState(
    DeviceIngredientState state, {
    int? newCurrentCapacity,
    int? newWarningPercent,
    bool? newIsWarning,
    bool? newIsRenewable,
    bool? newIsPrimary,
  }) {
    return UpdateIngredientRequest(
      deviceIngredientStateId: state.deviceIngredientStateId,
      warningPercent: newWarningPercent ?? state.warningPercent,
      currentCapacity: newCurrentCapacity ?? state.currentCapacity,
      isWarning: newIsWarning ?? state.isWarning,
      isRenewable: newIsRenewable ?? state.isRenewable,
      isPrimary: newIsPrimary ?? state.isPrimary,
    );
  }

  factory UpdateIngredientRequest.refill(
    DeviceIngredientState state, {
    int? customCapacity,
  }) {
    return UpdateIngredientRequest(
      deviceIngredientStateId: state.deviceIngredientStateId,
      warningPercent: state.warningPercent,
      currentCapacity: customCapacity ?? state.maxCapacity,
      isWarning: false, // Reset warning after refill
      isRenewable: state.isRenewable,
      isPrimary: state.isPrimary,
    );
  }

  @override
  String toString() {
    return 'UpdateIngredientRequest(deviceIngredientStateId: $deviceIngredientStateId, '
           'warningPercent: $warningPercent, currentCapacity: $currentCapacity, '
           'isWarning: $isWarning, isRenewable: $isRenewable, isPrimary: $isPrimary)';
  }

  @override
  bool operator ==(Object other) {
    if (identical(this, other)) return true;
    return other is UpdateIngredientRequest &&
        other.deviceIngredientStateId == deviceIngredientStateId &&
        other.warningPercent == warningPercent &&
        other.currentCapacity == currentCapacity &&
        other.isWarning == isWarning &&
        other.isRenewable == isRenewable &&
        other.isPrimary == isPrimary;
  }

  @override
  int get hashCode {
    return deviceIngredientStateId.hashCode ^
        warningPercent.hashCode ^
        currentCapacity.hashCode ^
        isWarning.hashCode ^
        isRenewable.hashCode ^
        isPrimary.hashCode;
  }
}