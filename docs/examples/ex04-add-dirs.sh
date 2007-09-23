#!/bin/sh
SERVER=$1
LOGIN=$2
mkdir /tmp/tf4mono/testdir1
mkdir /tmp/tf4mono/testdir2
echo tf /server:$SERVER /login:$LOGIN add /tmp/tf4mono/testdir1 /tmp/tf4mono/testdir2
tf /server:$SERVER /login:$LOGIN add /tmp/tf4mono/testdir1 /tmp/tf4mono/testdir2
