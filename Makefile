.PHONY: all clean update test image
.DELETE_ON_ERROR:

SHELL=/bin/bash -o pipefail
BIN_DIR=bin
PKG_CONFIG=.pkg_config
TEST_RESULTS_DIR=testResults

all: $(BIN_DIR)/azure-cloud-controller-manager

clean:
	rm -rf $(BIN_DIR) $(PKG_CONFIG) $(TEST_RESULTS_DIR)

update:
	scripts/update-dependencies.sh

test:
	mkdir -p $(TEST_RESULTS_DIR)
	cd pkg && go test -v ./... | tee ../$(TEST_RESULTS_DIR)/unittest.txt
ifdef JENKINS_HOME
	scripts/convert-test-report.pl $(TEST_RESULTS_DIR)/unittest.txt > $(TEST_RESULTS_DIR)/unittest.xml
endif

image: $(PKG_CONFIG)
	docker build -t $(shell scripts/image-tag.sh) .

$(PKG_CONFIG):
	scripts/pkg-config.sh > $@

$(BIN_DIR)/azure-cloud-controller-manager: $(PKG_CONFIG) main.go $(wildcard pkg/**/*)
	 go build -o $@ $(shell cat $<)
