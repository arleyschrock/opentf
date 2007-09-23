#!/bin/bash
SERVER=$1
LOGIN=$2
WORKSPACE=$3
SERVER_PATH=$4

function usage {
  echo "usage: run-examples.sh <server> <login> <workspace> <server-path>"
  exit
}

function run_examples {
		for i in ./ex*.sh; do
				echo $i $SERVER $LOGIN $WORKSPACE $SERVER_PATH
				echo
		    $i $SERVER $LOGIN $WORKSPACE $SERVER_PATH
				echo
		done
}

if [ "x$SERVER" = "x" ] ||  [ "x$LOGIN" = "x" ] || [ "x$WORKSPACE" = "x" ] || [ "x$SERVER_PATH" = "x" ]; then usage; fi

run_examples
