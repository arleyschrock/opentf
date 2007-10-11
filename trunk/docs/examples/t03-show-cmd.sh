#!/bin/sh
echo tf show build
tf show build
echo tf show cache
tf show cache
echo tf show ident /server:$SERVER /login:$LOGIN
tf show ident /server:$SERVER /login:$LOGIN
echo tf show stats /server:$SERVER /login:$LOGIN
tf show stats /server:$SERVER /login:$LOGIN
echo tf show tools /server:$SERVER /login:$LOGIN
tf show tools /server:$SERVER /login:$LOGIN
