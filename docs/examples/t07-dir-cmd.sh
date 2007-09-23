#!/bin/sh
SERVER=$1
LOGIN=$2
echo tf /server:$SERVER /login:$LOGIN dir "$/"
tf /server:$SERVER /login:$LOGIN dir "$/"

