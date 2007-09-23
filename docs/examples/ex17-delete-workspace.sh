#!/bin/sh
SERVER=$1
LOGIN=$2
echo tf /server:$SERVER /login:$LOGIN workspace /delete example
tf /server:$SERVER /login:$LOGIN workspace /delete example
