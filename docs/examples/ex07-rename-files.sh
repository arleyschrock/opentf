#!/bin/sh
SERVER=$1
LOGIN=$2
echo tf /server:$SERVER /login:$LOGIN rename /tmp/opentf/test1.txt /tmp/opentf/test4.txt
tf /server:$SERVER /login:$LOGIN rename /tmp/opentf/test1.txt /tmp/opentf/test4.txt
