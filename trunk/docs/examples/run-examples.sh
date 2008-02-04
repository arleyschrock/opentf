#!/bin/bash
SERVER=$1
LOGIN="$2,$5"
WORKSPACE=$3
SERVER_PATH=$4
USER=$2

function usage {
  echo "usage: run-examples.sh <server> <user@domain> <workspace> <server-path> <password>"
  exit
}

function run_examples {
		for i in ./ex*.sh; do
				echo $i $SERVER $LOGIN $WORKSPACE $SERVER_PATH $USER
				echo
		    $i $SERVER $LOGIN $WORKSPACE $SERVER_PATH $USER
				echo
		done
}

if [ "x$SERVER" = "x" ] ||  [ "x$LOGIN" = "x" ] || [ "x$WORKSPACE" = "x" ] || [ "x$SERVER_PATH" = "x" ]; then usage; fi

run_examples
