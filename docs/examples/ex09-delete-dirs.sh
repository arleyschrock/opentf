#!/bin/sh
SERVER=$1
LOGIN=$2
echo tf /server:$SERVER /login:$LOGIN delete /tmp/opentf/testdir3 /tmp/opentf/testdir2
tf /server:$SERVER /login:$LOGIN delete /tmp/opentf/testdir3 /tmp/opentf/testdir2