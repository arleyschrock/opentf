#!/bin/sh
SERVER=$1
LOGIN=$2
CHANGESET=`tf /server:$SERVER /login:$LOGIN hist /format:brief /stopafter:1 | cut -f 1 -d ' '`
echo tf /server:$SERVER /login:$LOGIN diff C$CHANGESET /brief
tf /server:$SERVER /login:$LOGIN diff C$CHANGESET /brief
echo tf /server:$SERVER /login:$LOGIN diff C$CHANGESET 
tf /server:$SERVER /login:$LOGIN diff C$CHANGESET 
