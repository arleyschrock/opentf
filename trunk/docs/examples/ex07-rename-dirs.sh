#!/bin/sh
SERVER=$1
LOGIN=$2
echo tf /server:$SERVER /login:$LOGIN rename /tmp/tf4mono/testdir1 /tmp/tf4mono/testdir3
tf /server:$SERVER /login:$LOGIN rename /tmp/tf4mono/testdir1 /tmp/tf4mono/testdir3
