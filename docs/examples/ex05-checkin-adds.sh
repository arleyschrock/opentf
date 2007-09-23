#!/bin/sh
SERVER=$1
LOGIN=$2
echo tf /server:$SERVER /login:$LOGIN checkin /tmp/tf4mono/ /C:first.checkin /recursive
tf /server:$SERVER /login:$LOGIN checkin /tmp/tf4mono/ /C:first.checkin /recursive
