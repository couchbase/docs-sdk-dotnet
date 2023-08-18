setup() {
	load 'node_modules/bats-support/load'
	load 'node_modules/bats-assert/load'

	HOWTOS_DIR=../modules/howtos/examples
	CONCEPT_DOCS_DIR=../modules/concept-docs/examples
	HELLO_WORLD_DIR=../modules/hello-world/examples

	BATS_TEST_RETRIES=3
}

function runExample() {
	cd $1
	run dotnet script $2
}

diag() {
	printf ' # %s\n' "$@" >&3
}
