#!/bin/sh
SERVER=$1
LOGIN=$2
echo tf /server:$SERVER /login:$LOGIN history /tmp/tf4mono /format:brief
tf /server:$SERVER /login:$LOGIN history /tmp/tf4mono /format:brief
echo tf /server:$SERVER /login:$LOGIN history /tmp/tf4mono /format:detailed
tf /server:$SERVER /login:$LOGIN history /tmp/tf4mono /format:detailed
echo tf /server:$SERVER /login:$LOGIN history /tmp/tf4mono /format:byowner
tf /server:$SERVER /login:$LOGIN history /tmp/tf4mono /format:byowner

