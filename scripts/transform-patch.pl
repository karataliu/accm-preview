#!/usr/bin/perl -w -n

# Copyright (c) Microsoft Corporation. All rights reserved.
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

use 5.012;

state $skip = 0;
if (m#^diff --git a/(.*) .*$#) {
    $skip = ($1 !~ m#^pkg/cloudprovider/providers/azure/#);
    print STDERR "# skipping file $1\n" if $skip;
}
next if $skip;

print s#/cloudprovider/providers/azure/#/azureprovider/#gr;
