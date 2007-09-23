#!/bin/sh
SERVER=$1
LOGIN=$2
echo test1 > /tmp/tf4mono/test1.txt
echo test2 > /tmp/tf4mono/test2.txt
echo test3 > /tmp/tf4mono/test3.txt
echo tf /server:$SERVER /login:$LOGIN add /tmp/tf4mono/test*.txt
tf /server:$SERVER /login:$LOGIN add /tmp/tf4mono/test*.txt
