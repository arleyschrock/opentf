#!/bin/sh
SERVER=$1
LOGIN=$2
echo tf /server:$SERVER /login:$LOGIN checkin /tmp/opentf/ /C:first.checkin /recursive
tf /server:$SERVER /login:$LOGIN checkin /tmp/opentf/ /C:first.checkin /recursive
