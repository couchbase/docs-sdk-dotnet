load 'test_helper'

### Hello World tests

@test "[hello-world] - StartUsing.csx" {
  runExample $HELLO_WORLD_DIR StartUsing.csx
  assert_success
}

@test "[hello-world] - IndexHelloWorld.csx" {
  runExample $HELLO_WORLD_DIR IndexHelloWorld.csx
  assert_success
}

@test "[hello-world] - KvBulkHelloWorld.csx" {
  runExample $HELLO_WORLD_DIR KvBulkHelloWorld.csx
  assert_success
}

@test "[hello-world] - KvHelloWorldScoped.csx" {
  runExample $HELLO_WORLD_DIR KvHelloWorldScoped.csx
  assert_success
}

### Howtos tests