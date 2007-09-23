#!/bin/sh
SERVER=$1
LOGIN=$2
WORKSPACE=$3
echo tf /server:$SERVER /login:$LOGIN /workspace:$WORKSPACE properties "$/"
tf /server:$SERVER /login:$LOGIN /workspace:$WORKSPACE properties "$/"

