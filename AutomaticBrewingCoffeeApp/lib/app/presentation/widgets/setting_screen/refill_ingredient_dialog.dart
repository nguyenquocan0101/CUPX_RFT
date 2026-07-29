import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:tdesign_flutter/tdesign_flutter.dart';
import 'package:abc_androidapp/app/data/models/organization/kiosk.dart';

class RefillIngredientDialog extends StatefulWidget {
  final DeviceIngredientState ingredient;
  final Function(int) onConfirm;

  const RefillIngredientDialog({
    super.key,
    required this.ingredient,
    required this.onConfirm,
  });

  @override
  State<RefillIngredientDialog> createState() => _RefillIngredientDialogState();
}

class _RefillIngredientDialogState extends State<RefillIngredientDialog> {
  late TextEditingController _capacityController;
  late FocusNode _capacityFocusNode;
  bool _isValidCapacity = true;
  String? _errorMessage;

  @override
  void initState() {
    super.initState();
    _capacityController = TextEditingController(
      text: widget.ingredient.maxCapacity.toString(),
    );
    _capacityFocusNode = FocusNode();
    
    WidgetsBinding.instance.addPostFrameCallback((_) {
      _capacityFocusNode.requestFocus();
      _capacityController.selection = TextSelection(
        baseOffset: 0,
        extentOffset: _capacityController.text.length,
      );
    });
  }

  @override
  void dispose() {
    _capacityController.dispose();
    _capacityFocusNode.dispose();
    super.dispose();
  }

  void _validateCapacity() {
    final value = int.tryParse(_capacityController.text);
    setState(() {
      if (value == null) {
        _isValidCapacity = false;
        _errorMessage = 'Vui lòng nhập số hợp lệ';
      } else if (value < widget.ingredient.minCapacity) {
        _isValidCapacity = false;
        _errorMessage = 'Giá trị tối thiểu: ${widget.ingredient.minCapacity} ${widget.ingredient.unit}';
      } else if (value > widget.ingredient.maxCapacity) {
        _isValidCapacity = false;
        _errorMessage = 'Giá trị tối đa: ${widget.ingredient.maxCapacity} ${widget.ingredient.unit}';
      } else {
        _isValidCapacity = true;
        _errorMessage = null;
      }
    });
  }

  void _setCapacity(int value) {
    _capacityController.text = value.toString();
    _validateCapacity();
  }

  @override
  Widget build(BuildContext context) {
    return Dialog(
      backgroundColor: Colors.transparent,
      insetPadding: const EdgeInsets.symmetric(horizontal: 24, vertical: 40),
      child: Container(
        width: MediaQuery.of(context).size.width * 0.9,
        constraints: const BoxConstraints(
          maxWidth: 600,
          maxHeight: 700,
        ),
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(24),
          boxShadow: [
            BoxShadow(
              color: Colors.black.withOpacity(0.15),
              blurRadius: 20,
              offset: const Offset(0, 8),
            ),
          ],
        ),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            _buildDialogHeader(),
            Flexible(
              child: SingleChildScrollView(
                padding: const EdgeInsets.fromLTRB(24, 0, 24, 24),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    _buildIngredientInfoCard(),
                    const SizedBox(height: 24),
                    _buildInputSection(),
                    const SizedBox(height: 20),
                    _buildQuickSelectButtons(),
                  ],
                ),
              ),
            ),
            _buildDialogActions(),
          ],
        ),
      ),
    );
  }

  Widget _buildDialogHeader() {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(24),
      decoration: BoxDecoration(
        gradient: LinearGradient(
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
          colors: [
            const Color(0xFF57B7E7).withOpacity(0.1),
            const Color(0xFF57B7E7).withOpacity(0.05),
          ],
        ),
        borderRadius: const BorderRadius.only(
          topLeft: Radius.circular(24),
          topRight: Radius.circular(24),
        ),
      ),
      child: Row(
        children: [
          Container(
            padding: const EdgeInsets.all(12),
            decoration: BoxDecoration(
              gradient: LinearGradient(
                colors: [
                  const Color(0xFF57B7E7).withOpacity(0.2),
                  const Color(0xFF57B7E7).withOpacity(0.1),
                ],
              ),
              borderRadius: BorderRadius.circular(12),
            ),
            child: const Icon(
              TDIcons.refresh,
              color: Color(0xFF57B7E7),
              size: 24,
            ),
          ),
          const SizedBox(width: 16),
          const Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  'Refill nguyên liệu',
                  style: TextStyle(
                    fontSize: 24,
                    fontWeight: FontWeight.bold,
                    color: Color(0xFF57B7E7),
                  ),
                ),
                SizedBox(height: 4),
                Text(
                  'Cập nhật dung lượng sau khi refill',
                  style: TextStyle(
                    fontSize: 14,
                    color: Colors.grey,
                    fontWeight: FontWeight.w500,
                  ),
                ),
              ],
            ),
          ),
          IconButton(
            onPressed: () => Navigator.of(context).pop(),
            icon: const Icon(TDIcons.close, color: Colors.grey, size: 24),
            style: IconButton.styleFrom(
              backgroundColor: Colors.grey.withOpacity(0.1),
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(12),
              ),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildIngredientInfoCard() {
    final percentage = widget.ingredient.capacityPercentage;
    
    Color getStatusColor() {
      if (percentage > 60) return Colors.green;
      if (percentage > 30) return Colors.orange;
      return Colors.red;
    }

    return Container(
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        gradient: LinearGradient(
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
          colors: [
            getStatusColor().withOpacity(0.08),
            getStatusColor().withOpacity(0.03),
          ],
        ),
        borderRadius: BorderRadius.circular(16),
        border: Border.all(color: getStatusColor().withOpacity(0.2)),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Container(
                padding: const EdgeInsets.all(12),
                decoration: BoxDecoration(
                  color: getStatusColor().withOpacity(0.15),
                  borderRadius: BorderRadius.circular(12),
                ),
                child: Icon(
                  TDIcons.layers,
                  color: getStatusColor(),
                  size: 24,
                ),
              ),
              const SizedBox(width: 16),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      widget.ingredient.ingredientType,
                      style: const TextStyle(
                        fontSize: 20,
                        fontWeight: FontWeight.bold,
                        color: Colors.black87,
                      ),
                    ),
                    const SizedBox(height: 4),
                    Text(
                      'Nguyên liệu ${widget.ingredient.isPrimary ? 'chính' : 'phụ'}',
                      style: TextStyle(
                        fontSize: 14,
                        color: Colors.grey.shade600,
                        fontWeight: FontWeight.w500,
                      ),
                    ),
                  ],
                ),
              ),
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
                decoration: BoxDecoration(
                  color: getStatusColor().withOpacity(0.15),
                  borderRadius: BorderRadius.circular(20),
                ),
                child: Text(
                  '${percentage.toStringAsFixed(1)}%',
                  style: TextStyle(
                    fontSize: 16,
                    fontWeight: FontWeight.bold,
                    color: getStatusColor(),
                  ),
                ),
              ),
            ],
          ),
          
          const SizedBox(height: 16),
          
          // Progress bar
          Container(
            height: 10,
            decoration: BoxDecoration(
              color: Colors.grey.shade200,
              borderRadius: BorderRadius.circular(5),
            ),
            child: FractionallySizedBox(
              alignment: Alignment.centerLeft,
              widthFactor: percentage / 100,
              child: Container(
                decoration: BoxDecoration(
                  gradient: LinearGradient(
                    colors: [
                      getStatusColor().withOpacity(0.8),
                      getStatusColor(),
                    ],
                  ),
                  borderRadius: BorderRadius.circular(5),
                ),
              ),
            ),
          ),
          
          const SizedBox(height: 16),
          
          // Capacity details
          Row(
            children: [
              Expanded(
                child: _buildDetailItem(
                  'Hiện tại',
                  '${widget.ingredient.currentCapacity} ${widget.ingredient.unit}',
                  TDIcons.info_circle,
                  Colors.blue,
                ),
              ),
              const SizedBox(width: 12),
              Expanded(
                child: _buildDetailItem(
                  'Tối đa',
                  '${widget.ingredient.maxCapacity} ${widget.ingredient.unit}',
                  TDIcons.arrow_up,
                  Colors.green,
                ),
              ),
              const SizedBox(width: 12),
              Expanded(
                child: _buildDetailItem(
                  'Tối thiểu',
                  '${widget.ingredient.minCapacity} ${widget.ingredient.unit}',
                  TDIcons.arrow_down,
                  Colors.orange,
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }

  Widget _buildDetailItem(String label, String value, IconData icon, Color color) {
    return Container(
      padding: const EdgeInsets.all(12),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(10),
        border: Border.all(color: color.withOpacity(0.2)),
      ),
      child: Column(
        children: [
          Icon(icon, size: 16, color: color),
          const SizedBox(height: 4),
          Text(
            label,
            style: TextStyle(
              fontSize: 11,
              color: color,
              fontWeight: FontWeight.w600,
            ),
          ),
          const SizedBox(height: 2),
          Text(
            value,
            style: const TextStyle(
              fontSize: 12,
              fontWeight: FontWeight.bold,
              color: Colors.black87,
            ),
            textAlign: TextAlign.center,
          ),
        ],
      ),
    );
  }

  Widget _buildInputSection() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        const Text(
          'Nhập dung lượng mới',
          style: TextStyle(
            fontSize: 18,
            fontWeight: FontWeight.bold,
            color: Colors.black87,
          ),
        ),
        const SizedBox(height: 8),
        Text(
          'Dung lượng phải nằm trong khoảng ${widget.ingredient.minCapacity} - ${widget.ingredient.maxCapacity} ${widget.ingredient.unit}',
          style: TextStyle(
            fontSize: 14,
            color: Colors.grey.shade600,
          ),
        ),
        const SizedBox(height: 16),
        
        TextField(
          controller: _capacityController,
          focusNode: _capacityFocusNode,
          keyboardType: TextInputType.number,
          inputFormatters: [
            FilteringTextInputFormatter.digitsOnly,
          ],
          onChanged: (_) => _validateCapacity(),
          style: const TextStyle(
            fontSize: 18,
            fontWeight: FontWeight.bold,
          ),
          decoration: InputDecoration(
            hintText: 'Nhập dung lượng...',
            suffixText: widget.ingredient.unit,
            prefixIcon: Container(
              margin: const EdgeInsets.all(12),
              padding: const EdgeInsets.all(8),
              decoration: BoxDecoration(
                color: _isValidCapacity 
                    ? const Color(0xFF57B7E7).withOpacity(0.1) 
                    : Colors.red.withOpacity(0.1),
                borderRadius: BorderRadius.circular(8),
              ),
              child: Icon(
                TDIcons.edit,
                color: _isValidCapacity ? const Color(0xFF57B7E7) : Colors.red,
                size: 20,
              ),
            ),
            border: OutlineInputBorder(
              borderRadius: BorderRadius.circular(16),
            ),
            focusedBorder: OutlineInputBorder(
              borderRadius: BorderRadius.circular(16),
              borderSide: BorderSide(
                color: _isValidCapacity ? const Color(0xFF57B7E7) : Colors.red,
                width: 2,
              ),
            ),
            errorText: _errorMessage,
          ),
        ),
      ],
    );
  }

  Widget _buildQuickSelectButtons() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        const Text(
          'Chọn nhanh',
          style: TextStyle(
            fontSize: 16,
            fontWeight: FontWeight.w600,
            color: Colors.black87,
          ),
        ),
        const SizedBox(height: 12),
        Row(
          children: [
            Expanded(
              child: _buildQuickButton(
                'Đầy',
                widget.ingredient.maxCapacity,
                const Color(0xFF57B7E7),
              ),
            ),
            const SizedBox(width: 12),
            Expanded(
              child: _buildQuickButton(
                'Nửa',
                ((widget.ingredient.maxCapacity + widget.ingredient.minCapacity) / 2).round(),
                Colors.orange,
              ),
            ),
            const SizedBox(width: 12),
            Expanded(
              child: _buildQuickButton(
                'Tối thiểu',
                widget.ingredient.minCapacity,
                Colors.grey,
              ),
            ),
          ],
        ),
      ],
    );
  }

  Widget _buildQuickButton(String label, int value, Color color) {
    return OutlinedButton(
      onPressed: () => _setCapacity(value),
      style: OutlinedButton.styleFrom(
        side: BorderSide(color: color, width: 2),
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(12),
        ),
        padding: const EdgeInsets.symmetric(vertical: 16),
        backgroundColor: color.withOpacity(0.05),
      ),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          Text(
            label,
            style: TextStyle(
              color: color,
              fontSize: 14,
              fontWeight: FontWeight.bold,
            ),
          ),
          const SizedBox(height: 4),
          Text(
            '$value ${widget.ingredient.unit}',
            style: TextStyle(
              color: color,
              fontSize: 12,
              fontWeight: FontWeight.w500,
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildDialogActions() {
    return Container(
      padding: const EdgeInsets.all(24),
      decoration: BoxDecoration(
        color: Colors.grey.shade50,
        borderRadius: const BorderRadius.only(
          bottomLeft: Radius.circular(24),
          bottomRight: Radius.circular(24),
        ),
        border: Border(
          top: BorderSide(color: Colors.grey.shade200),
        ),
      ),
      child: Row(
        children: [
          Expanded(
            child: TextButton(
              onPressed: () => Navigator.of(context).pop(),
              style: TextButton.styleFrom(
                padding: const EdgeInsets.symmetric(vertical: 16),
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(12),
                ),
                backgroundColor: Colors.grey.shade100,
              ),
              child: const Text(
                'Hủy',
                style: TextStyle(
                  fontSize: 16,
                  fontWeight: FontWeight.w600,
                  color: Colors.grey,
                ),
              ),
            ),
          ),
          const SizedBox(width: 16),
          Expanded(
            flex: 2,
            child: ElevatedButton(
              onPressed: !_isValidCapacity
                  ? null
                  : () {
                      final value = int.parse(_capacityController.text);
                      Navigator.of(context).pop();
                      widget.onConfirm(value);
                    },
              style: ElevatedButton.styleFrom(
                backgroundColor: _isValidCapacity 
                    ? const Color(0xFF57B7E7) 
                    : Colors.grey.shade300,
                foregroundColor: Colors.white,
                padding: const EdgeInsets.symmetric(vertical: 16),
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(12),
                ),
              ),
              child: const Text(
                'Xác nhận refill',
                style: TextStyle(
                  fontSize: 16,
                  fontWeight: FontWeight.bold,
                ),
              ),
            ),
          ),
        ],
      ),
    );
  }
}