import 'package:flutter/widgets.dart';

import 'app/app.dart';
import 'app/bootstrap/app_bootstrap.dart';

Future<void> main() async {
  WidgetsFlutterBinding.ensureInitialized();

  final AppBootstrap bootstrap = await AppBootstrap.initialize();

  runApp(DomuApp(bootstrap: bootstrap));
}
