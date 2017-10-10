# Dependency management
We use [glide](https://github.com/Masterminds/glide) to manage package dependency, which can automation vendor dependency management and also support various kind of vendoring tools.

- [glide](https://github.com/Masterminds/glide) (Recommended version: 0.12.3)
- [glide-vc](https://github.com/sgotti/glide-vc) (Recommended version: 0.1.0)

## Staging packages
There are some packages under `staging` directory of Kubernetes repository, to which we will refer as `k8s staging packages`.

Those packages are intended to be periodically published under top-level `k8s.io` repositories, and the code under `staging` directory serves as authoritative (see description [here](https://github.com/kubernetes/kubernetes/tree/master/staging) ).

In Kubernetes code, those packages can be refereed via `k8s.io/package` directly, that is because in Kubernetes repository there are corresponding soft links under vendor directory pointing to those under staging. This is a workaround by Kubernetes  (see also discussion [here](https://github.com/kubernetes/kubernetes/pull/24202)).

Such workaround makes things a bit tricky for packages that will depend on Kubernetes. The code in this repository depends on a part of Kubernetes (various controller logic), and thus indirectly depends on the `k8s staging packages`, such as `k8s.io/api`. There is no explicit mapping between main Kubernetes repository version and those `k8s staging packages`' version. Which makes it difficult to find proper version of `k8s staging packages`

For example, a vendoring tool such as glide will try to fetch `k8s.io/api` during resolving packages. But it doesn't know the exact version tag to get(not recorded in `Godeps.json`), and then it will try to use HEAD version, which is not likely to be correct.

Hereby we create a script (`scripts/update-dependencies.sh`) to update dependencies. It uses `glide`'s `mirror` feature, copying staging package to a local and set it as upstream for `k8s staging packages`.

Command `make update` will trigger the update script run.

Currently there's a known issue that if you run `make update` from a stable branch, it will still trigger `glide.lock` to be modified. That's because some 'k8s.io/*' packages are synced from local git repository, which has an unstable git commit version.

We welcome new proposals if you find better ways to address this issue.

## Update existing dependencies
To update existing dependencies, just update version in `glide.yaml` and then run `make update`

## Introduce new dependencies
Check glide.lock or vendor first, if it is already there, just use it.
If it is not there, then first add it in glide.yaml.
```
- package: github.com/a/b
  version: {version}
```

Then add the package used in source code, but referenced by '_'.
```
_ "github.com/a/b/package"
```

Then run `make update`, this will pull in the dependency correctly. It is suggested make this step a single commit when sending pull request.
