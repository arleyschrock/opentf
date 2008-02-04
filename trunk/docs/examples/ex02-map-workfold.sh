#!/bin/sh
SERVER=$1
LOGIN=$2
WORKSPACE=$3
SERVER_PATH=$4
rm -rf /tmp/opentf
echo tf /server:$SERVER /login:$LOGIN workfold $SERVER_PATH /tmp/opentf
tf /server:$SERVER /login:$LOGIN workfold $SERVER_PATH /tmp/opentf
