ARG source_root=/go/src/github.com/Azure/kubernetes-azure-cloud-controller-manager

FROM golang:1.8.3-jessie AS build
ARG source_root
WORKDIR $source_root
COPY . .
RUN make

FROM buildpack-deps:jessie-curl
ARG source_root
WORKDIR /
COPY --from=build $source_root/bin/azure-cloud-controller-manager .
CMD ["/azure-cloud-controller-manager", "--version"]
