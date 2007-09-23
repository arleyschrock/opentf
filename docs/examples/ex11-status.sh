#!/bin/sh
SERVER=$1
LOGIN=$2
echo tf /server:$SERVER /login:$LOGIN status /tmp/tf4mono/
tf /server:$SERVER /login:$LOGIN status /tmp/tf4mono/
