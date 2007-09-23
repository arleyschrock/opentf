#!/bin/sh
echo tf help
tf help
for i in `tf help /list`; do
	echo tf help $i;
  tf help $i;
done
