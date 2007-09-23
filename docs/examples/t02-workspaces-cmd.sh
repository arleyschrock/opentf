#!/bin/sh
SERVER=$1
LOGIN=$2
echo tf workspaces /server:$SERVER /login:$LOGIN
tf workspaces /server:$SERVER /login:$LOGIN
echo
echo tf workspaces /server:$SERVER /login:$LOGIN /owner:"*"
tf /server:$SERVER /login:$LOGIN workspaces /owner:"*"
echo
echo tf /server:$SERVER /login:$LOGIN workspaces /owner:"*" /computer:"*"
tf /server:$SERVER /login:$LOGIN workspaces /owner:"*" /computer:"*"
echo
