#!/bin/sh
SERVER=$1
LOGIN=$2
echo tf /server:$SERVER /login:$LOGIN history /tmp/opentf /format:brief
tf /server:$SERVER /login:$LOGIN history /tmp/opentf /format:brief
echo tf /server:$SERVER /login:$LOGIN history /tmp/opentf /format:detailed
tf /server:$SERVER /login:$LOGIN history /tmp/opentf /format:detailed
echo tf /server:$SERVER /login:$LOGIN history /tmp/opentf /format:byowner
tf /server:$SERVER /login:$LOGIN history /tmp/opentf /format:byowner

