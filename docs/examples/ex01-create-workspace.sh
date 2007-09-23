#!/bin/sh
SERVER=$1
LOGIN=$2
echo tf /server:$SERVER /login:$LOGIN workspace /new "example;JREED"
tf /server:$SERVER /login:$LOGIN workspace /new "example;JREED"
