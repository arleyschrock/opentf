#!/bin/sh
SERVER=$1
LOGIN=$2
CHANGESET=`tf /server:$SERVER /login:$LOGIN hist /format:brief /stopafter:1 | cut -f 1 -d ' ' | tail -n 1`
echo tf /server:$SERVER /login:$LOGIN /brief diff C$CHANGESET 
tf /server:$SERVER /login:$LOGIN /brief diff C$CHANGESET 
echo tf /server:$SERVER /login:$LOGIN diff C$CHANGESET 
tf /server:$SERVER /login:$LOGIN diff C$CHANGESET 
