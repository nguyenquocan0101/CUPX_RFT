import 'package:abc_androidapp/app/core/router/app_router.dart';
import 'package:abc_androidapp/app/data/models/menu_product_mapping.dart';
import 'package:abc_androidapp/app/data/models/product/product.dart';
import 'package:abc_androidapp/app/data/models/product/product_category.dart';
import 'package:abc_androidapp/app/presentation/blocs/menu/menu_bloc.dart';
import 'package:abc_androidapp/app/presentation/screens/menu/widgets/banner_app_bar.dart';
import 'package:abc_androidapp/app/presentation/screens/menu/widgets/bottom_app_bar.dart';
import 'package:abc_androidapp/app/presentation/widgets/custom_app_bar.dart';
import 'package:abc_androidapp/app/presentation/screens/menu/widgets/product_card.dart';
import 'package:abc_androidapp/app/presentation/widgets/section.dart';
import 'package:abc_androidapp/app/presentation/widgets/side_bar_item.dart';
import 'package:abc_androidapp/config/themes/app_palette.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

class MenuScreen extends StatefulWidget {
  static const String route = "/menu";
  const MenuScreen({super.key});

  @override
  State<MenuScreen> createState() => _MenuScreenState();
}

class _MenuScreenState extends State<MenuScreen> {
  String _selectCategory = "";
  final ScrollController _scrollController = ScrollController();
  final Map<String, List<MenuProductMapping>> groupedByCategory = {};
  final Map<String, ProductCategory> categoryMap = {};
  final Map<String, GlobalKey> sectionKeys = {};
  bool _isProgrammaticScroll = false;
  DateTime _lastScrollUpdate = DateTime.now();

  @override
  void initState() {
    super.initState();
    context.read<MenuBloc>().add(const GetMenuEvent());
    _scrollController.addListener(_onScroll);
  }

  @override
  void dispose() {
    _scrollController.removeListener(_onScroll);
    _scrollController.dispose();
    super.dispose();
  }

  void _initializeCategoryData(List<dynamic> productsInMenu) {
    groupedByCategory.clear();
    categoryMap.clear();
    sectionKeys.clear();

    // Group products by category
    for (var productInMenu in productsInMenu) {
      if (productInMenu.product.isActive == false) {
        continue;
      }
      final Product product = productInMenu.product;
      final ProductCategory category = product.category ??
          ProductCategory(
            productCategoryId: "uncategorized",
            name: "Chưa Nhóm",
            description: "",
            status: "active",
            imageUrl: "",
            displayOrder: 100,
          );

      final categoryId = category.productCategoryId;
      categoryMap[categoryId] = category; // Lưu mapping

      if (groupedByCategory.containsKey(categoryId)) {
        groupedByCategory[categoryId]!.add(productInMenu);
      } else {
        groupedByCategory[categoryId] = [productInMenu];
      }
    }

    // Initialize section keys
    for (var categoryId in groupedByCategory.keys) {
      if (groupedByCategory[categoryId]!.isNotEmpty) {
        sectionKeys[categoryId] = GlobalKey();
      }
    }

    // Set first category as selected if none selected
    if (_selectCategory.isEmpty && groupedByCategory.isNotEmpty) {
      _selectCategory = groupedByCategory.keys.first;
    }

    // Calculate section positions after layout
    WidgetsBinding.instance.addPostFrameCallback((_) {
      _calculateSectionPositions();
    });
  }

  void _calculateSectionPositions() {
    for (var categoryId in sectionKeys.keys) {
      final key = sectionKeys[categoryId];
      final renderObject = key?.currentContext?.findRenderObject();
      if (renderObject != null && renderObject is RenderBox) {
        final position = renderObject.localToGlobal(Offset.zero);
        print('Position for category $categoryId: $position');
      }
    }
  }

  void _onScroll() {
    if (_isProgrammaticScroll) return;

    // Debounce scroll updates
    final now = DateTime.now();
    if (now.difference(_lastScrollUpdate).inMilliseconds < 5000) return;
    _lastScrollUpdate = now;

    final scrollOffset = _scrollController.offset;
    String? newSelectedCategory;

    for (final categoryId in sectionKeys.keys) {
      final key = sectionKeys[categoryId];
      final renderObject = key?.currentContext?.findRenderObject();
      if (renderObject != null && renderObject is RenderBox) {
        final position = renderObject.localToGlobal(Offset.zero).dy;
        if (scrollOffset >= position -1) {
          // Tolerance of 50px
          newSelectedCategory = categoryId;
        } else if (scrollOffset < position) {
          break;
        }
      }
    }

    if (newSelectedCategory != null && _selectCategory != newSelectedCategory) {
      setState(() {
        _selectCategory = newSelectedCategory!;
      });
    }
  }

  void _scrollToSection(String categoryId) {
    final key = sectionKeys[categoryId];
    if (key == null || key.currentContext == null) return;

    _isProgrammaticScroll = true;
    Scrollable.ensureVisible(
      key.currentContext!,
      duration: const Duration(milliseconds: 500),
      curve: Curves.easeInOut,
    ).then((_) {
      _isProgrammaticScroll = false;
    });
  }

  void _handleClickCategory(String selectItem, String categoryId) {
    setState(() {
      _selectCategory = selectItem;
      _scrollToSection(categoryId);
    });
  }

  @override
  Widget build(BuildContext context) {
    final size = MediaQuery.of(context).size;

    return Scaffold(
      extendBodyBehindAppBar: true,
       appBar: BannerAppBar(
        imagePaths: ['assets/images/banner_test.png'],
      ),
      backgroundColor: AppPalette.white,
      body: BlocConsumer<MenuBloc, MenuState>(
        listener: (context, state) {
          if (state is MenuLoaded) {
            _initializeCategoryData(state.menuInKiosk.productsInMenu);
          }
        },
        builder: (context, state) {
          if (state is MenuLoaded) {
            final categories = groupedByCategory.entries.toList()
              ..sort((a, b) {
                final categoryA = categoryMap[a.key]!;
                final categoryB = categoryMap[b.key]!;
                return categoryA.displayOrder.compareTo(categoryB.displayOrder);
              });

            return Column(
              children: [
                Container(
                  width: size.width,
                  height: 230,
                  color: AppPalette.blue.blue1,
                ),
                const SizedBox(height: 10.0),
                Expanded(
                  child: Row(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      // Sidebar
                      Container(
                        width: 150,
                        decoration: BoxDecoration(
                          color: AppPalette.white,
                          borderRadius: BorderRadius.circular(12),
                          boxShadow: [
                            BoxShadow(
                              color: Colors.black.withOpacity(0.05),
                              blurRadius: 4,
                              offset: const Offset(0, 2),
                            ),
                          ],
                        ),
                        child: ListView.builder(
                          padding: EdgeInsets.zero,
                          itemCount: categories.length,
                          itemBuilder: (context, index) {
                            final categoryIdKey = categories[index].key;
                            final category =
                                categoryMap[categoryIdKey];
                            final productsInMenu = groupedByCategory[categoryIdKey]!;
                            if (productsInMenu.isEmpty)
                              return const SizedBox.shrink();
                            return SideBarItem(
                              title: category!.name ?? "Chưa Nhóm",
                              onTap: () {
                                _handleClickCategory(
                                    categoryIdKey, categoryIdKey);
                              },
                              isChosen: _selectCategory == categoryIdKey,
                              imageUrl: productsInMenu.first.product.imageUrl,
                            );
                          },
                        ),
                      ),
                      const SizedBox(width: 8.0),
                      // Main content area
                      Expanded(
                        child: Padding(
                          padding: const EdgeInsets.only(right: 8.0),
                          child: CustomScrollView(
                            controller: _scrollController,
                            physics: const BouncingScrollPhysics(),
                            slivers: [
                              SliverList(
                                delegate: SliverChildBuilderDelegate(
                                  (context, index) {
                                    final entry = categories[index];
                                    final categoryId = entry.key;
                                    final categoryName =
                                        categoryMap[categoryId]?.name ??
                                            "Chưa Nhóm";
                                    final productsInCategory = entry.value;
                                    if (productsInCategory.isEmpty) {
                                      return const SizedBox.shrink();
                                    }
                                    return Section(
                                      key: sectionKeys[categoryId],
                                      title: categoryName,
                                      detail: Column(
                                        children: productsInCategory
                                            .map((productInMenu) => Padding(
                                                  padding: const EdgeInsets
                                                      .symmetric(vertical: 4.0),
                                                  child: ProductCard(
                                                    product: productInMenu.product,
                                                    isAvailable: productInMenu.isAvailable,
                                                    onTap: () {
                                                      AppRouter
                                                          .navigateToProductDetailScreen(
                                                              productInMenu.product);
                                                    },
                                                  ),
                                                ))
                                            .toList(),
                                      ),
                                    );
                                  },
                                  childCount: groupedByCategory.length,
                                ),
                              ),
                            ],
                          ),
                        ),
                      ),
                    ],
                  ),
                ),
              ],
            );
          }
          if(state is MenuError) {
            return Center(
              child: Text(
                state.message,
                style: TextStyle(color: AppPalette.blue.primary),
              ),
            );
          }
          return const Center(child: CircularProgressIndicator());
        },
      ),
      bottomNavigationBar: CartBottomBar(
        onCheckout: () {
          AppRouter.navigateToOrderConfirmationScreen();
        },
      ),
    );
  }
}
