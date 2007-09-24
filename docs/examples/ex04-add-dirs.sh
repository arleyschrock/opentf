#!/bin/sh
SERVER=$1
LOGIN=$2
mkdir /tmp/opentf/testdir1
mkdir /tmp/opentf/testdir2
echo tf /server:$SERVER /login:$LOGIN add /tmp/opentf/testdir1 /tmp/opentf/testdir2
tf /server:$SERVER /login:$LOGIN add /tmp/opentf/testdir1 /tmp/opentf/testdir2
