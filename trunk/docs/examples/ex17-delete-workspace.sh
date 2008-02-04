#!/bin/sh
SERVER=$1
LOGIN=$2
$USER=$5
echo tf /server:$SERVER /login:$LOGIN workspace /delete "example;$USER"
tf /server:$SERVER /login:$LOGIN workspace /delete "example;$USER"
