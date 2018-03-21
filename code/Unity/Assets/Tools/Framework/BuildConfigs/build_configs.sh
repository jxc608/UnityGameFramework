#!/bin/bash
source ~/.bash_profile
cd $1/../../../config/client
python jsonbuild.py
TargetPath=$1/
if [ ! -d $TargetPath ]; then
	mkdir -p $TargetPath
fi
cp -rp ./json/* $TargetPath
