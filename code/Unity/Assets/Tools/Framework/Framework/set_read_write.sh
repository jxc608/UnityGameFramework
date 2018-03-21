#!/bin/sh
cat $1 | while read line
do
	chmod u+w $2/$line
done
