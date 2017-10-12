.PHONY: all clean update test image
.DELETE_ON_ERROR:

SHELL=/bin/bash -o pipefail
BIN_DIR=bin
PKG_CONFIG=.pkg_config
TEST_RESULTS_DIR=testResults
# TODO: fix code and remove disabled options
GOMETALINTER_OPTION=--tests -D deadcode -D gocyclo -D vetshadow -D gas -D ineffassign

all: $(BIN_DIR)/azure-cloud-controller-manager

clean:
	rm -rf $(BIN_DIR) $(PKG_CONFIG) $(TEST_RESULTS_DIR)

update:
	scripts/update-dependencies.sh

test-unit:
	mkdir -p $(TEST_RESULTS_DIR)
	cd pkg && go test -v ./... | tee ../$(TEST_RESULTS_DIR)/unittest.txt
ifdef JENKINS_HOME
	scripts/convert-test-report.pl $(TEST_RESULTS_DIR)/unittest.txt > $(TEST_RESULTS_DIR)/unittest.xml
endif

test-lint:
	gometalinter.v1 $(GOMETALINTER_OPTION) ./ pkg/...

test-lint-prepare:
	go get -u gopkg.in/alecthomas/gometalinter.v1
	gometalinter.v1 -i

image: $(PKG_CONFIG)
	docker build -t $(shell scripts/image-tag.sh) .

$(PKG_CONFIG):
	scripts/pkg-config.sh > $@

$(BIN_DIR)/azure-cloud-controller-manager: $(PKG_CONFIG) main.go $(wildcard pkg/**/*)
	 go build -o $@ $(shell cat $<)
