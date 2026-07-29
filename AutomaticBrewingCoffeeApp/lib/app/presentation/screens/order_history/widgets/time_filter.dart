import 'package:abc_androidapp/app/presentation/widgets/common/dialog.dart';
import 'package:abc_androidapp/config/themes/app_palette.dart';
import 'package:flutter/material.dart';

class TimeFilter extends StatefulWidget {
  final void Function(DateTime? startTime, DateTime? endTime) onApply;
  final VoidCallback onReset;
  final int pageIndex;
  final int pageSize;

  const TimeFilter({
    super.key,
    required this.onApply,
    required this.onReset,
    required this.pageIndex,
    required this.pageSize,
  });

  @override
  State<TimeFilter> createState() => _TimeFilterState();
}

class _TimeFilterState extends State<TimeFilter> {
  DateTime? startDate;
  DateTime? endDate;

  @override
  Widget build(BuildContext context) {
    return Container(
      width: double.infinity,
      decoration: const BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.vertical(top: Radius.circular(32)),
        boxShadow: [
          BoxShadow(
            color: Color(0x1A000000),
            blurRadius: 24,
            offset: Offset(0, -8),
          ),
        ],
      ),
      child: Padding(
        padding: const EdgeInsets.fromLTRB(32, 24, 32, 32),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            // Handle bar
            Container(
              width: 48,
              height: 4,
              decoration: BoxDecoration(
                color: const Color(0xFFE5E7EB),
                borderRadius: BorderRadius.circular(2),
              ),
            ),
            const SizedBox(height: 32),

            // Header
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                const Text(
                  'Lọc theo thời gian',
                  style: TextStyle(
                    fontSize: 28,
                    fontWeight: FontWeight.w700,
                    color: Color(0xFF1F2937),
                    letterSpacing: -0.5,
                  ),
                ),
                Container(
                  width: 40,
                  height: 40,
                  decoration: BoxDecoration(
                    color: const Color(0xFFF9FAFB),
                    borderRadius: BorderRadius.circular(12),
                    border: Border.all(
                      color: const Color(0xFFE5E7EB),
                      width: 1,
                    ),
                  ),
                  child: IconButton(
                    onPressed: () => Navigator.pop(context),
                    icon: const Icon(
                      Icons.close_rounded,
                      size: 20,
                      color: Color(0xFF6B7280),
                    ),
                    padding: EdgeInsets.zero,
                  ),
                ),
              ],
            ),
            const SizedBox(height: 40),

            // Time section
            Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Wrap(
                  spacing: 12,
                  runSpacing: 12,
                  children: [
                    _buildChip(context, 'Hôm nay', () {
                      final now = DateTime.now();
                      final startTime =
                          DateTime(now.year, now.month, now.day, 0, 0, 0);
                      final endTime = DateTime(
                          now.year, now.month, now.day, 23, 59, 59, 999);
                      widget.onApply(startTime, endTime);
                      Navigator.pop(context);
                    }),
                    _buildChip(context, 'Tuần', () {
                      final now = DateTime.now();
                      final startTime =
                          DateTime(now.year, now.month, now.day, 0, 0, 0);
                      final endTime = startTime.add(const Duration(
                          days: 6,
                          hours: 23,
                          minutes: 59,
                          seconds: 59,
                          milliseconds: 999));
                      widget.onApply(startTime, endTime);
                      Navigator.pop(context);
                    }),
                    _buildChip(context, 'Tháng', () {
                      final now = DateTime.now();
                      final startTime =
                          DateTime(now.year, now.month, 1, 0, 0, 0);
                      final endTime =
                          DateTime(now.year, now.month + 1, 0, 23, 59, 59, 999);
                      widget.onApply(startTime, endTime);
                      Navigator.pop(context);
                    }),
                  ],
                ),
                const SizedBox(height: 24),
                Row(
                  children: [
                    Expanded(
                        child: _buildDateField(context, 'Từ ngày', startDate,
                            (picked) {
                      if (endDate != null && picked.isAfter(endDate!)) {
                        showCustomDialog(
                          context: context,
                          message:
                              'Ngày bắt đầu phải trước hoặc bằng ngày kết thúc',
                          type: DialogType.error,
                        );

                        return; // không cập nhật ngày
                      }
                      setState(() {
                        startDate = picked;
                      });
                    })),
                    const SizedBox(width: 16),
                    Expanded(
                        child: _buildDateField(context, 'Đến ngày', endDate,
                            (picked) {
                      if (startDate != null && picked.isBefore(startDate!)) {
                        showCustomDialog(
                          context: context,
                          message:
                              'Ngày kết thúc phải sau hoặc bằng ngày bắt đầu',
                          type: DialogType.error,
                        );

                        return; // không cập nhật ngày
                      }
                      setState(() {
                        endDate = picked;
                      });
                    })),
                  ],
                ),
              ],
            ),
            const SizedBox(height: 48),
            // Actions
            Row(
              children: [
                Expanded(
                  child: OutlinedButton(
                    onPressed: () {
                      setState(() {
                        startDate = null;
                        endDate = null;
                      });
                      widget.onReset();
                      Navigator.pop(context);
                    },
                    style: OutlinedButton.styleFrom(
                      padding: const EdgeInsets.symmetric(vertical: 16),
                      shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(16),
                      ),
                      side: const BorderSide(
                          color: Color(0xFFE5E7EB), width: 1.5),
                    ),
                    child: const Text(
                      'Đặt lại',
                      style:
                          TextStyle(fontSize: 18, fontWeight: FontWeight.w600),
                    ),
                  ),
                ),
                const SizedBox(width: 20),
                Expanded(
                  child: ElevatedButton(
                    onPressed: () {
                      final startTime = startDate == null ? null : DateTime(startDate!.year,
                          startDate!.month, startDate!.day, 0, 0, 0);
                      final endTime = endDate == null ? null : DateTime(endDate!.year, endDate!.month,
                          endDate!.day, 23, 59, 59, 999);
                      widget.onApply(startTime, endTime);
                      Navigator.pop(context);
                    },
                    style: ElevatedButton.styleFrom(
                      backgroundColor: const Color(0xFF57B7E7),
                      padding: const EdgeInsets.symmetric(vertical: 16),
                      shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(16),
                      ),
                    ),
                    child: const Text(
                      'Áp dụng',
                      style:
                          TextStyle(fontSize: 18, fontWeight: FontWeight.w600, color: AppPalette.white),
                    ),
                  ),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildChip(BuildContext context, String label, VoidCallback onTap) {
    return InkWell(
      onTap: onTap,
      child: Container(
        padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 12),
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(16),
          border: Border.all(color: const Color(0xFFE5E7EB), width: 1.5),
          boxShadow: const [
            BoxShadow(
              color: Color(0x0A000000),
              blurRadius: 8,
              offset: Offset(0, 2),
            ),
          ],
        ),
        child: Text(
          label,
          style: const TextStyle(
            color: Color(0xFF6B7280),
            fontSize: 16,
            fontWeight: FontWeight.w500,
            letterSpacing: -0.1,
          ),
        ),
      ),
    );
  }

  Widget _buildDateField(BuildContext context, String label,
      DateTime? selectedDate, Function(DateTime) onDatePicked) {
    return InkWell(
      onTap: () async {
        final picked = await showDatePicker(
          context: context,
          initialDate: selectedDate ?? DateTime.now(),
          firstDate: DateTime(2020),
          lastDate: DateTime.now().add(const Duration(days: 365)),
          builder: (context, child) => Theme(
            data: Theme.of(context).copyWith(
              colorScheme: Theme.of(context).colorScheme.copyWith(
                    primary: const Color(0xFF57B7E7),
                  ),
            ),
            child: child!,
          ),
        );
        if (picked != null) {
          onDatePicked(picked);
        }
      },
      child: Container(
        height: 56,
        padding: const EdgeInsets.symmetric(horizontal: 16),
        decoration: BoxDecoration(
          color: const Color(0xFFF9FAFB),
          borderRadius: BorderRadius.circular(16),
          border: Border.all(color: const Color(0xFFE5E7EB), width: 1.5),
        ),
        child: Row(
          children: [
            const Icon(Icons.date_range_outlined,
                size: 20, color: Color(0xFF6B7280)),
            const SizedBox(width: 12),
            Expanded(
              child: Text(
                selectedDate != null
                    ? "${selectedDate.day.toString().padLeft(2, '0')}/${selectedDate.month.toString().padLeft(2, '0')}/${selectedDate.year}"
                    : label,
                style: const TextStyle(
                  fontSize: 16,
                  fontWeight: FontWeight.w500,
                  color: Color(0xFF6B7280),
                  letterSpacing: -0.1,
                ),
              ),
            ),
            const Icon(Icons.keyboard_arrow_down_rounded,
                size: 20, color: Color(0xFF9CA3AF)),
          ],
        ),
      ),
    );
  }
}
