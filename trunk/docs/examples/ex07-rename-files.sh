#!/bin/sh
SERVER=$1
LOGIN=$2
echo tf /server:$SERVER /login:$LOGIN rename /tmp/tf4mono/test1.txt /tmp/tf4mono/test4.txt
tf /server:$SERVER /login:$LOGIN rename /tmp/tf4mono/test1.txt /tmp/tf4mono/test4.txt
