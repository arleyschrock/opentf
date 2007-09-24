#!/bin/sh
SERVER=$1
LOGIN=$2
echo test1 > /tmp/opentf/test1.txt
echo test2 > /tmp/opentf/test2.txt
echo test3 > /tmp/opentf/test3.txt
echo tf /server:$SERVER /login:$LOGIN add /tmp/opentf/test*.txt
tf /server:$SERVER /login:$LOGIN add /tmp/opentf/test*.txt
