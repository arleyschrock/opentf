#!/bin/sh
SERVER=$1
LOGIN=$2
echo tf /server:$SERVER /login:$LOGIN delete /tmp/tf4mono/test*.txt
tf /server:$SERVER /login:$LOGIN delete /tmp/tf4mono/test*.txt
