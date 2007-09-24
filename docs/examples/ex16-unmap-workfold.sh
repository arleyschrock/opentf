#!/bin/sh
SERVER=$1
LOGIN=$2
echo tf /server:$SERVER /login:$LOGIN workfold /unmap /tmp/opentf
tf /server:$SERVER /login:$LOGIN workfold /unmap /tmp/opentf
