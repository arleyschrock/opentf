#!/bin/sh
SERVER=$1
LOGIN=$2
echo tf /server:$SERVER /login:$LOGIN history "$/" /format:brief /stopafter:256
tf /server:$SERVER /login:$LOGIN history "$/" /format:brief /stopafter:256
echo tf /server:$SERVER /login:$LOGIN history "$/" /format:detailed /stopafter:3
tf /server:$SERVER /login:$LOGIN history "$/" /format:detailed /stopafter:3
echo tf /server:$SERVER /login:$LOGIN history "$/" /format:byowner /stopafter:300
tf /server:$SERVER /login:$LOGIN history "$/" /format:byowner /stopafter:300
