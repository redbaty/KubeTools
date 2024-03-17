# Overview

This repository is a collection of simple kubernetes commands that I use frequently.
This is not production ready.

Currently, it contains the following commands:

* Cleanup Finalized Pods - `kubectl delete pods --field-selector=status.phase==Succeeded` for all namespaces, this also cleans Failed pods
