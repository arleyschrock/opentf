#!/bin/sh
SERVER=$1
LOGIN=$2
echo tf /server:$SERVER /login:$LOGIN rename /tmp/opentf/testdir1 /tmp/opentf/testdir3
tf /server:$SERVER /login:$LOGIN rename /tmp/opentf/testdir1 /tmp/opentf/testdir3
