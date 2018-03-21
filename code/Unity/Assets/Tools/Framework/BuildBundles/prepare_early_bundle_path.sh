#!/bin/sh
source ~/.bash_profile
DstPath=$GOPATH/src/$3/static
> $2
for file in $DstPath/*
do
	if test -d $file; then
		dir=${file##*/}
		if [[ $dir == *asset_bundles_* ]]; then
			v=${dir/asset_bundles_/}
			if [ "$1"x \> "$v"x ]; then
				echo $file >> $2
			fi
		fi
	fi
done