#!/usr/bin/env bash

# this is required if any git module has been initialised to another remote repo
# If any of the submodule directory is contaminated, add "--force" argument
git submodule sync && \
git submodule foreach git fetch && \
git submodule update --init --recursive "${@}"
