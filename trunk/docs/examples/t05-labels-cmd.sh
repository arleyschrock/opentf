#!/bin/sh
SERVER=$1
LOGIN=$2
echo tf /server:$SERVER /login:$LOGIN labels
tf /server:$SERVER /login:$LOGIN labels
echo tf /server:$SERVER /login:$LOGIN labels /owner:"*"
tf /server:$SERVER /login:$LOGIN labels /owner:"*"
