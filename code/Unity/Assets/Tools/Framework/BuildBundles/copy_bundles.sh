#!/bin/sh
source ~/.bash_profile
DstPath=$GOPATH/src/$5/static/asset_bundles_$3/$2
rm -rf $DstPath
cd $1
find . -name '*' ! -name '*.meta' ! -name '*.zip' ! -name '.DS_Store' | cpio -updm $DstPath
for file in $4/*
do
	if test -f $file; then
		f=${file##*/}
		if [[ $f == $2*.upk ]]; then
			cp $file $DstPath/../$f
		fi
	fi
done