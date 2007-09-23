#!/bin/sh
SERVER=$1
LOGIN=$2
echo tf /server:$SERVER /login:$LOGIN delete /tmp/tf4mono/testdir3 /tmp/tf4mono/testdir2
tf /server:$SERVER /login:$LOGIN delete /tmp/tf4mono/testdir3 /tmp/tf4mono/testdir2