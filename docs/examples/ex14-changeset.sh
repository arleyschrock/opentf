#!/bin/sh
SERVER=$1
LOGIN=$2
CHANGESET=`tf /server:$SERVER /login:$LOGIN hist /format:brief /stopafter:1 | cut -f 1 -d ' '`
echo tf /server:$SERVER /login:$LOGIN changeset $CHANGESET /format:brief
tf /server:$SERVER /login:$LOGIN changeset $CHANGESET /format:brief
echo tf /server:$SERVER /login:$LOGIN changeset $CHANGESET /format:detailed
tf /server:$SERVER /login:$LOGIN changeset $CHANGESET /format:detailed
