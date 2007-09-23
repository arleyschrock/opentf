#!/bin/sh
SERVER=$1
LOGIN=$2
echo tf /server:$SERVER /login:$LOGIN changeset /latest
tf /server:$SERVER /login:$LOGIN changeset /latest

