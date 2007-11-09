#!/bin/sh
SERVER=$1
LOGIN=$2
echo tf /server:$SERVER /login:$LOGIN shelvesets /owner:"*"
tf /server:$SERVER /login:$LOGIN shelvesets /owner:"*"

