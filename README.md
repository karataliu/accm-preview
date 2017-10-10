# Kubernetes azure-cloud-controller-manager

## Overview
`Kubernetes azure-cloud-controller-manager` is a Kubernetes component that supports interoperability with Azure cloud platform. It runs together with other master components to provide the Kubernetes cluster’s control plane.

[Kubernetes cloud-controller-manager](https://kubernetes.io/docs/concepts/overview/components/#cloud-controller-manager) is a Kubernetes component designed for running controllers that would interact with various cloud platforms. It operates with the cloud through [Kubernetes cloud provider interface](https://github.com/kubernetes/kubernetes/blob/master/pkg/cloudprovider/cloud.go), meanwhile different cloud platforms would have corresponding implementations of the cloud provider interface.

This project provides an Azure specialized version of `Kubernetes cloud controller manager`: it depends on Kubernetes project and builds upon [cloud-controller-manager/app](https://github.com/kubernetes/kubernetes/tree/master/cmd/cloud-controller-manager/app). This project also incorporates [Azure cloud provider](pkg/azureprovider), which implements `Kubernetes cloud provider interface` using [azure-sdk-for-go](https://github.com/Azure/azure-sdk-for-go).

## Usage
`Kubernetes azure-cloud-controller-manager` will release container image at [location TBD](#), and can be run as containers. For development, you could also build a standalone binary and run.

We also recommend using [acs-engine](https://github.com/Azure/acs-engine) to deploy Kubernetes cluster, which supports deploying `Kubernetes azure-cloud-controller-manager` for Kubernetes v1.8+, see [doc TBD](#) for details.

## Development
Prerequisions:
- [golang](https://golang.org/doc/install) (Recommended version: 1.8.3)

Build project:
```
make
```

Run unit tests:
```
make test
```

Updating dependency: (please check [Dependency management](docs/dependency-management.md) for additional information)
```
make update
```

Please also check following development docs:
- [Release versioning](docs/release-versioning.md)
- [Dependency management](docs/dependency-management.md)
- [Issues and pull request migration](docs/issues-and-pull-requests-migration.md)

## Roadmap
- Better documentation
- Automation test infrastructure
- Better test coverage

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.microsoft.com.

When you submit a pull request, a CLA-bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., label, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
