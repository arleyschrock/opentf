#!/bin/sh
SERVER=$1
LOGIN=$2
WORKSPACE=$3
SERVERPATH=$4
echo tf /server:$SERVER /login:$LOGIN view $SERVERPATH/test1.txt
tf /server:$SERVER /login:$LOGIN view $SERVERPATH/test1.txt
