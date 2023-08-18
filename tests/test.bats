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

@test "[howtos] - KvOperations.csx" {
  runExample $HOWTOS_DIR KvOperations.csx
  assert_success
}

@test "[howtos] - CollectionManager.csx" {
  runExample $HOWTOS_DIR CollectionManager.csx
  assert_success
}

@test "[howtos] - N1qlQueries.csx" {
  runExample $HOWTOS_DIR N1qlQueries.csx
  assert_success
}

@test "[howtos] - ScopeQueries.csx" {
  runExample $HOWTOS_DIR ScopeQueries.csx
  assert_success
}

@test "[howtos] - Auth.csx" {
  runExample $HOWTOS_DIR Auth.csx
  assert_success
}

@test "[howtos] - ClusterExample.csx" {
  runExample $HOWTOS_DIR ClusterExample.csx
  assert_success
}

@test "[howtos] - EncryptingUsingSdk.csx" {
  runExample $HOWTOS_DIR EncryptingUsingSdk.csx
  assert_success
}

@test "[howtos] - ErrorHandling.csx" {
  runExample $HOWTOS_DIR ErrorHandling.csx
  assert_success
}

@test "[howtos] - ManagingConnections.csx" {
  runExample $HOWTOS_DIR ManagingConnections.csx
  assert_success
}

@test "[howtos] - ProvisioningResourcesBuckets.csx" {
  runExample $HOWTOS_DIR ProvisioningResourcesBuckets.csx
  assert_success
}

@test "[howtos] - ProvisioningResourcesViews.csx" {
  runExample $HOWTOS_DIR ProvisioningResourcesViews.csx
  assert_success
}

@test "[howtos] - QueryIndexManagerExample.csx" {
  runExample $HOWTOS_DIR QueryIndexManagerExample.csx
  assert_success
}

@test "[howtos] - SubDocument.csx" {
  runExample $HOWTOS_DIR SubDocument.csx
  assert_success
}

@test "[howtos] - UserManagementExample.csx" {
  runExample $HOWTOS_DIR UserManagementExample.csx
  assert_success
}