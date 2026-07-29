import 'package:intl/intl.dart';

String formatTime(String time) {
  DateFormat dateFormat = DateFormat("yyyy-MM-dd HH:mm:ss");
  String result = time.replaceAll('T', ' ');
  DateTime dateTime = dateFormat.parse(result);
  String formattedDate = DateFormat('HH:mm dd/MM/yyyy').format(dateTime);
  return formattedDate;
}

String formatOnlyTime(String time) {
  DateFormat dateFormat = DateFormat("yyyy-MM-dd HH:mm:ss");
  String result = time.replaceAll('T', ' ');
  DateTime dateTime = dateFormat.parse(result);
  String formattedDate = DateFormat('HH:mm').format(dateTime);
  return formattedDate;
}

String formatOnlyDate(String time) {
  DateFormat dateFormat = DateFormat("yyyy-MM-dd HH:mm:ss");
  String result = time.replaceAll('T', ' ');
  DateTime dateTime = dateFormat.parse(result);
  String formattedDate = DateFormat('dd/MM/yyyy').format(dateTime);
  return formattedDate;
}

String formatTimeOnly(String time) {
  DateTime dateTime = DateFormat("HH:mm:ss").parse(time);
  String formattedDate = DateFormat('HH:mm').format(dateTime);
  return formattedDate;
}
