#!/bin/sh
SERVER=$1
LOGIN=$2
WORKSPACE=$3
echo tf /server:$SERVER /login:$LOGIN get /tmp/tf4mono
tf /server:$SERVER /login:$LOGIN get /tmp/tf4mono
