import 'package:abc_androidapp/config/constants/animation_path.dart';
import 'package:flutter/material.dart';
import 'package:lottie/lottie.dart';
import 'package:abc_androidapp/config/themes/app_color_extension.dart';
import 'package:abc_androidapp/config/themes/app_palette.dart';
import 'package:abc_androidapp/config/themes/app_typography.dart';

class FailureScreen extends StatefulWidget {
  static const String route = '/failure';

  const FailureScreen({Key? key}) : super(key: key);

  @override
  State<FailureScreen> createState() => _FailureScreenState();
}

class _FailureScreenState extends State<FailureScreen> {
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
                    color: colors.error,
                  ),
                  SizedBox(height: 16),
                  Text(
                    "Đang xử lý lỗi...",
                    style: AppTypography.bodyLarge.copyWith(
                      color: colors.error,
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
                                Container(
                                  padding: EdgeInsets.symmetric(
                                      vertical: 12, horizontal: 24),
                                  decoration: BoxDecoration(
                                    color:
                                        AppPalette.blue.blue1.withOpacity(0.3),
                                    borderRadius: BorderRadius.circular(16),
                                    border: Border.all(
                                      color: colors.error.withOpacity(0.5),
                                      width: 2,
                                    ),
                                  ),
                                  child: Text(
                                    "THANH TOÁN THẤT BẠI",
                                    style:
                                        AppTypography.headlineMedium.copyWith(
                                      color: colors.onSurface,
                                    ),
                                    textAlign: TextAlign.center,
                                  ),
                                ),
                                const SizedBox(height: 32),
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
                                        Icons.error_outline,
                                        size: 80,
                                        color: colors.error,
                                      ),
                                    ),
                                    Lottie.asset(
                                      AnimationPath.fail,
                                      width: 200,
                                      height: 200,
                                      repeat: false,
                                      frameRate: FrameRate.max,
                                    ),
                                  ],
                                ),
                                const SizedBox(height: 24),
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
                                          Icon(Icons.warning,
                                              color: colors.error),
                                          SizedBox(width: 8),
                                          Expanded(
                                            child: Text(
                                              "Giao dịch không thành công",
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
                                        "Vui lòng kiểm tra phương thức thanh toán và thử lại.",
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
                                ElevatedButton(
                                  onPressed: () {},
                                  style: ElevatedButton.styleFrom(
                                    backgroundColor: colors.error,
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
                                      Icon(Icons.refresh, size: 24),
                                      const SizedBox(width: 8),
                                      Text(
                                        "THỬ LẠI",
                                        style:
                                            AppTypography.labelLarge.copyWith(
                                          color: colors.onPrimary,
                                        ),
                                      ),
                                    ],
                                  ),
                                ),
                                const SizedBox(height: 16),
                                OutlinedButton(
                                  onPressed: () {
                                    Navigator.pop(context);
                                  },
                                  style: OutlinedButton.styleFrom(
                                    side: BorderSide(color: colors.error),
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
                                        "QUAY LẠI",
                                        style:
                                            AppTypography.labelLarge.copyWith(
                                          color: colors.error,
                                        ),
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
