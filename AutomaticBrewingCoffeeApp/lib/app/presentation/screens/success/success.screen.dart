import 'package:abc_androidapp/config/constants/animation_path.dart';
import 'package:flutter/material.dart';
import 'package:lottie/lottie.dart';
import 'package:qr_flutter/qr_flutter.dart';
import 'package:abc_androidapp/config/themes/app_color_extension.dart';
import 'package:abc_androidapp/config/themes/app_palette.dart';
import 'package:abc_androidapp/config/themes/app_typography.dart';

class SuccessScreen extends StatefulWidget {
  static const String route = '/success';

  const SuccessScreen({Key? key}) : super(key: key);

  @override
  State<SuccessScreen> createState() => _SuccessScreenState();
}

class _SuccessScreenState extends State<SuccessScreen> {
  final String orderNumber = "0012";
  bool _isLoading = true;

  @override
  void initState() {
    super.initState();
    _loadData();
  }

  Future<void> _loadData() async {
    await Future.delayed(const Duration(seconds: 1));
    setState(() {
      _isLoading = false;
    });
  }

  @override
  Widget build(BuildContext context) {
    final colors = Theme.of(context).extension<AppColorsExtension>()!;

    return Scaffold(
      body: _isLoading
          ? Center(
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  CircularProgressIndicator(
                    color: colors.primary,
                  ),
                  SizedBox(height: 16),
                  Text(
                    "Đang chuẩn bị đơn hàng...",
                    style: AppTypography.bodyLarge.copyWith(
                      color: colors.primary,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                ],
              ),
            )
          : Container(
              decoration: BoxDecoration(
                image: DecorationImage(
                  image: NetworkImage(
                    'https://images.unsplash.com/photo-1511537190424-bbbab87ac5eb?ixlib=rb-1.2.1&auto=format&fit=crop&w=1350&q=80',
                  ),
                  fit: BoxFit.cover,
                  colorFilter: ColorFilter.mode(
                    Colors.black.withOpacity(0.2),
                    BlendMode.darken,
                  ),
                ),
              ),
              child: SafeArea(
                child: Center(
                  child: SingleChildScrollView(
                    padding: const EdgeInsets.all(24.0),
                    child: Card(
                      elevation: 8,
                      shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(24),
                      ),
                      color: colors.cardBackground,
                      child: Column(
                        mainAxisSize: MainAxisSize.min,
                        children: [
                          Padding(
                            padding: const EdgeInsets.all(24.0),
                            child: Column(
                              children: [
                                // Thank you message
                                Container(
                                  padding: EdgeInsets.symmetric(
                                      vertical: 12, horizontal: 24),
                                  decoration: BoxDecoration(
                                    color:
                                        AppPalette.blue.blue1.withOpacity(0.3),
                                    borderRadius: BorderRadius.circular(16),
                                    border: Border.all(
                                      color: colors.primary.withOpacity(0.5),
                                      width: 2,
                                    ),
                                  ),
                                  child: Text(
                                    "CẢM ƠN ĐẠT 09",
                                    style:
                                        AppTypography.headlineMedium.copyWith(
                                      color: colors.onSurface,
                                    ),
                                    textAlign: TextAlign.center,
                                  ),
                                ),
                                const SizedBox(height: 32),

                                // Coffee cup animation
                                Stack(
                                  alignment: Alignment.center,
                                  children: [
                                    Container(
                                      width: 150,
                                      height: 150,
                                      decoration: BoxDecoration(
                                        color: AppPalette.blue.blue1
                                            .withOpacity(0.3),
                                        shape: BoxShape.circle,
                                      ),
                                      child: Icon(
                                        Icons.coffee,
                                        size: 80,
                                        color: colors.primary,
                                      ),
                                    ),
                                    Lottie.asset(
                                      AnimationPath.success,
                                      width: 200,
                                      height: 200,
                                      repeat: false,
                                      frameRate: FrameRate.max,
                                    ),
                                  ],
                                ),
                                const SizedBox(height: 24),

                                // Order number with coffee bean decoration
                                Row(
                                  mainAxisAlignment: MainAxisAlignment.center,
                                  children: [
                                    Icon(Icons.coffee_outlined,
                                        color: colors.primary),
                                    SizedBox(width: 8),
                                    Text(
                                      "Mã đơn của bạn",
                                      style: AppTypography.titleMedium.copyWith(
                                        color: colors.onSurface,
                                      ),
                                    ),
                                    SizedBox(width: 8),
                                    Icon(Icons.coffee_outlined,
                                        color: colors.primary),
                                  ],
                                ),
                                const SizedBox(height: 16),
                                Container(
                                  padding: const EdgeInsets.symmetric(
                                      vertical: 16, horizontal: 32),
                                  decoration: BoxDecoration(
                                    color:
                                        AppPalette.blue.blue1.withOpacity(0.3),
                                    borderRadius: BorderRadius.circular(16),
                                    border: Border.all(
                                      color: colors.primary,
                                      width: 2,
                                    ),
                                    boxShadow: [
                                      BoxShadow(
                                        color: colors.primary.withOpacity(0.2),
                                        blurRadius: 10,
                                        offset: Offset(0, 4),
                                      ),
                                    ],
                                  ),
                                  child: Text(
                                    orderNumber,
                                    style: AppTypography.displayLarge.copyWith(
                                      color: colors.onSurface,
                                    ),
                                  ),
                                ),
                                const SizedBox(height: 32),

                                // Waiting message with coffee theme
                                Container(
                                  padding: EdgeInsets.all(16),
                                  decoration: BoxDecoration(
                                    color:
                                        AppPalette.blue.blue1.withOpacity(0.2),
                                    borderRadius: BorderRadius.circular(12),
                                    border: Border.all(
                                      color: AppPalette.blue.blue1,
                                      width: 1,
                                    ),
                                  ),
                                  child: Column(
                                    children: [
                                      Row(
                                        children: [
                                          Icon(Icons.access_time,
                                              color: colors.primary),
                                          SizedBox(width: 8),
                                          Expanded(
                                            child: Text(
                                              "Đơn hàng của bạn đang được pha chế",
                                              style: AppTypography.bodyLarge
                                                  .copyWith(
                                                color: colors.onSurface,
                                                fontWeight: FontWeight.bold,
                                              ),
                                            ),
                                          ),
                                        ],
                                      ),
                                      SizedBox(height: 12),
                                      Text(
                                        "Vui lòng đợi ở chỗ ngồi gần quầy. Chúng tôi sẽ phục vụ món bạn nhanh nhất!",
                                        style: AppTypography.bodyLarge.copyWith(
                                          color:
                                              colors.onSurface.withOpacity(0.8),
                                        ),
                                        textAlign: TextAlign.center,
                                      ),
                                    ],
                                  ),
                                ),
                                const SizedBox(height: 32),

                                // Return button
                                ElevatedButton(
                                  onPressed: () {},
                                  style: ElevatedButton.styleFrom(
                                    backgroundColor: colors.primary,
                                    foregroundColor: colors.onPrimary,
                                    padding: const EdgeInsets.symmetric(
                                        vertical: 16, horizontal: 24),
                                    shape: RoundedRectangleBorder(
                                      borderRadius: BorderRadius.circular(12),
                                    ),
                                    minimumSize:
                                        const Size(double.infinity, 60),
                                  ),
                                  child: Row(
                                    mainAxisAlignment: MainAxisAlignment.center,
                                    children: [
                                      Icon(Icons.arrow_back, size: 24),
                                      const SizedBox(width: 8),
                                      Text(
                                        "VỀ MÀN HÌNH CHÍNH",
                                        style:
                                            AppTypography.labelLarge.copyWith(
                                          color: colors.onPrimary,
                                        ),
                                      ),
                                    ],
                                  ),
                                ),
                                const SizedBox(height: 32),

                                // QR code with coffee theme
                                Container(
                                  padding: EdgeInsets.all(16),
                                  decoration: BoxDecoration(
                                    color: colors.cardBackground,
                                    borderRadius: BorderRadius.circular(16),
                                    border: Border.all(
                                      color: AppPalette.blue.blue1,
                                      width: 1,
                                    ),
                                  ),
                                  child: Column(
                                    children: [
                                      Text(
                                        "Quét mã để nhận ưu đãi",
                                        style: AppTypography.bodyLarge.copyWith(
                                          color: colors.primary,
                                          fontWeight: FontWeight.bold,
                                        ),
                                      ),
                                      SizedBox(height: 16),
                                      Row(
                                        children: [
                                          Container(
                                            padding: EdgeInsets.all(8),
                                            decoration: BoxDecoration(
                                              color: colors.cardBackground,
                                              borderRadius:
                                                  BorderRadius.circular(8),
                                              border: Border.all(
                                                color: colors.primary,
                                                width: 2,
                                              ),
                                            ),
                                            child: QrImageView(
                                              data:
                                                  'https://coffeehouse.com/promo',
                                              version: QrVersions.auto,
                                              size: 100,
                                              backgroundColor:
                                                  colors.cardBackground,
                                              foregroundColor: colors.onSurface,
                                            ),
                                          ),
                                          const SizedBox(width: 16),
                                          Expanded(
                                            child: Column(
                                              crossAxisAlignment:
                                                  CrossAxisAlignment.start,
                                              children: [
                                                Text(
                                                  "Giảm 20% cho lần sau!",
                                                  style: AppTypography
                                                      .bodyMedium
                                                      .copyWith(
                                                    color: colors.primary,
                                                    fontWeight: FontWeight.bold,
                                                  ),
                                                ),
                                                SizedBox(height: 8),
                                                Text(
                                                  "Áp dụng cho tất cả các loại đồ uống",
                                                  style: AppTypography
                                                      .bodyMedium
                                                      .copyWith(
                                                    color: colors.onSurface
                                                        .withOpacity(0.7),
                                                  ),
                                                ),
                                              ],
                                            ),
                                          ),
                                        ],
                                      ),
                                    ],
                                  ),
                                ),
                              ],
                            ),
                          ),
                        ],
                      ),
                    ),
                  ),
                ),
              ),
            ),
    );
  }
}
