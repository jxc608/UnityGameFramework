#!/usr/bin/env python
# encoding: utf-8

import shutil
import os

from template import excel2json
from template import buildcustom

builds = [
	buildcustom,
]

if os.path.exists("json/") == True:
	shutil.rmtree("json/")

def work(works):
	for config in works:
		excel = "../excel/" + config.excel
		template = config.template
		excel2json.generate(excel, "./json/", template, config.folder + '/')

def build_configs():
	for build in builds:
		work(build.configs)

build_configs()

print "done"
