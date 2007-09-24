#!/bin/sh
SERVER=$1
LOGIN=$2
echo tf /server:$SERVER /login:$LOGIN diff /brief /tmp/opentf/
tf /server:$SERVER /login:$LOGIN diff /brief /tmp/opentf/
echo tf /server:$SERVER /login:$LOGIN diff /tmp/opentf/
tf /server:$SERVER /login:$LOGIN diff /tmp/opentf/
