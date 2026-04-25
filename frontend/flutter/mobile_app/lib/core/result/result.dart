import 'failure.dart';

sealed class Result<T> {
  const Result();

  R when<R>({
    required R Function(T value) success,
    required R Function(Failure failure) failure,
  }) {
    final Result<T> result = this;

    return switch (result) {
      Success<T>(:final value) => success(value),
      ResultFailure<T>(:final error) => failure(error),
    };
  }
}

class Success<T> extends Result<T> {
  const Success(this.value);

  final T value;
}

class ResultFailure<T> extends Result<T> {
  const ResultFailure(this.error);

  final Failure error;
}
