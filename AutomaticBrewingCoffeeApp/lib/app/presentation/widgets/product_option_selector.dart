import 'package:abc_androidapp/config/themes/app_palette.dart';
import 'package:flutter/material.dart';

class ProductOptionSelector extends StatefulWidget {
  final String title;
  final List<String> items;
  final ValueChanged<String> onSelect;
  final int selectedIndex;
  final String? initialSelectedItem; // ✅ Add this for better control

  const ProductOptionSelector({
    super.key,
    required this.title,
    required this.items,
    required this.onSelect,
    this.selectedIndex = 0,
    this.initialSelectedItem,
  });

  @override
  State<ProductOptionSelector> createState() => _ProductOptionSelectorState();
}

class _ProductOptionSelectorState extends State<ProductOptionSelector> {
  late int selectedIndex;

  @override
  void initState() {
    super.initState();
    _initializeSelectedIndex();
  }

  void _initializeSelectedIndex() {
    if (widget.initialSelectedItem != null) {
      // ✅ Find index by item name if provided
      final index = widget.items.indexOf(widget.initialSelectedItem!);
      selectedIndex = index >= 0 ? index : widget.selectedIndex;
    } else {
      selectedIndex = widget.selectedIndex;
    }
    
    // ✅ Ensure index is within bounds
    if (selectedIndex >= widget.items.length) {
      selectedIndex = 0;
    }
  }

  @override
  void didUpdateWidget(ProductOptionSelector oldWidget) {
    super.didUpdateWidget(oldWidget);
    
    // ✅ Handle widget updates properly
    if (oldWidget.selectedIndex != widget.selectedIndex ||
        oldWidget.initialSelectedItem != widget.initialSelectedItem ||
        oldWidget.items != widget.items) {
      _initializeSelectedIndex();
    }
  }

  void _handleSelection(int index) {
    print('Tapped index: $index, item: ${widget.items[index]}'); // ✅ Debug log
    
    setState(() {
      selectedIndex = index;
    });
    
    widget.onSelect(widget.items[index]);
  }

  @override
  Widget build(BuildContext context) {
    return Container(
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(10.0),
        border: Border.all(color: AppPalette.blue.primary),
      ),
      padding: const EdgeInsets.all(12.0),
      margin: const EdgeInsets.symmetric(vertical: 10.0),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            widget.title,
            style: Theme.of(context).textTheme.titleLarge!.copyWith(
              fontWeight: FontWeight.bold,
            ),
            overflow: TextOverflow.ellipsis,
            maxLines: 1,
          ),
          const SizedBox(height: 10),
          
          // ✅ Improved list generation with better tap handling
          ...widget.items.asMap().entries.map((entry) {
            final index = entry.key;
            final item = entry.value;
            final isSelected = index == selectedIndex;
            
            return InkWell( // ✅ Use InkWell instead of GestureDetector
              onTap: () => _handleSelection(index),
              child: Container(
                width: double.infinity, // ✅ Ensure full width tap area
                padding: const EdgeInsets.symmetric(vertical: 12.0, horizontal: 4.0),
                decoration: index != (widget.items.length - 1)
                    ? BoxDecoration(
                        border: Border(
                          bottom: BorderSide(
                            color: AppPalette.grey.grey2,
                            width: 1.0,
                          ),
                        ),
                      )
                    : null,
                child: Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    Expanded( // ✅ Prevent overflow
                      child: Text(
                        item,
                        style: Theme.of(context).textTheme.titleMedium?.copyWith(
                          color: isSelected 
                              ? AppPalette.blue.primary 
                              : Colors.black87,
                          fontWeight: isSelected 
                              ? FontWeight.w600 
                              : FontWeight.normal,
                        ),
                        overflow: TextOverflow.ellipsis,
                      ),
                    ),
                    
                    const SizedBox(width: 8),
                    
                    AnimatedSwitcher( // ✅ Smooth icon transition
                      duration: const Duration(milliseconds: 200),
                      child: Icon(
                        isSelected ? Icons.check_circle : Icons.circle_outlined,
                        color: isSelected 
                            ? AppPalette.blue.primary 
                            : AppPalette.grey.grey2,
                        size: 20,
                        key: ValueKey(isSelected),
                      ),
                    ),
                  ],
                ),
              ),
            );
          }).toList(),
        ],
      ),
    );
  }
}