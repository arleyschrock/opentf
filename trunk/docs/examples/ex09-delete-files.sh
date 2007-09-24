#!/bin/sh
SERVER=$1
LOGIN=$2
echo tf /server:$SERVER /login:$LOGIN delete /tmp/opentf/test*.txt
tf /server:$SERVER /login:$LOGIN delete /tmp/opentf/test*.txt
