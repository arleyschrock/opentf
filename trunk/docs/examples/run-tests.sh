#!/bin/bash
SERVER=$1
LOGIN=$2
WORKSPACE=$3

function usage {
  echo "usage: run-tests.sh <server> <login> <workspace>"
  exit
}

function run_tests {
		for i in ./t*-cmd.sh; do
				echo $i $SERVER $LOGIN $WORKSPACE
				echo
		    $i $SERVER $LOGIN $WORKSPACE
				echo
		done
}

if [ "x$SERVER" = "x" ] ||  [ "x$LOGIN" = "x" ] || [ "x$WORKSPACE" = "x" ]; then usage; fi

run_tests

