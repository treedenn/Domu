import '../domain/space.dart';

abstract class SpacesRepository {
  Future<List<Space>> getSpaces();
}

class StubSpacesRepository implements SpacesRepository {
  @override
  Future<List<Space>> getSpaces() async {
    return const <Space>[];
  }
}
