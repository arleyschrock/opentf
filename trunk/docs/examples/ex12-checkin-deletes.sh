#!/bin/sh
SERVER=$1
LOGIN=$2
echo tf /server:$SERVER /login:$LOGIN checkin /tmp/tf4mono/ /C:delete.checkin /tmp/tf4mono/
tf /server:$SERVER /login:$LOGIN checkin /tmp/tf4mono/ /C:delete.checkin /tmp/tf4mono/
