#!/bin/sh
SERVER=$1
LOGIN=$2
WORKSPACE=$3
echo tf workfold /server:$SERVER /login:$LOGIN /workspace:$WORKSPACE
tf workfold /server:$SERVER /login:$LOGIN /workspace:$WORKSPACE
