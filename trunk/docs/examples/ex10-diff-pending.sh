#!/bin/sh
SERVER=$1
LOGIN=$2
echo tf /server:$SERVER /login:$LOGIN diff /brief /tmp/tf4mono/
tf /server:$SERVER /login:$LOGIN diff /brief /tmp/tf4mono/
echo tf /server:$SERVER /login:$LOGIN diff /tmp/tf4mono/
tf /server:$SERVER /login:$LOGIN diff /tmp/tf4mono/
