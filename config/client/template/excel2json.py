#env python
# encoding: utf-8

import sys
import xlrd
import json
import os
import os.path
from os.path import basename

def generate(excel_path, out_path, template, assetFolder):
	wb = xlrd.open_workbook(excel_path)
	sheet = wb.sheet_by_index(0)

	idxkey = []
	for i,k in enumerate(sheet.row_values(1)):
		idxkey.append((i, k.strip().encode('utf8') ))
	
	celldict = {}
	for i in range(2, sheet.nrows):
		row_data = sheet.row_values(i)
		# print row_data
		temp_dict = {}
		# for index, key in enumerate(sheet.row_values(1)):

		for (idx, key) in idxkey:
			valuetype = template.get(key);
			if not valuetype:
				continue

			value = row_data[idx]
			try:
				if valuetype == "int":
					value = int(value)
				else:
					if isinstance(value, float):
						value = str(float(value))
					max = len(value)
					if max > 2 and value[max-2:max] == ".0" and key != "version":
						value = value[0:max-2];
					value = value if isinstance(value, unicode) else str(value)
			except Exception, e:
				print excel_path + " " + key + " " + value
				raise e


			temp_dict[key] = value

			# jsonkey = keyformat % temp_dict
			# print jsonkey
		jsonkey = template["keyformat"] % temp_dict
		celldict[jsonkey] = temp_dict


	# print celldict
	context =  json.dumps(celldict, indent=4, ensure_ascii=False, sort_keys=True).encode('utf8')
	#context =  json.dumps(celldict, ensure_ascii=False, sort_keys=False).encode('utf8')

	filename = os.path.splitext(excel_path.replace('../excel/', ''))[0] + ".json"
	filename = filename.replace (filename.split('/')[-1], '') + "Config/" + filename.split('/')[-1]
	filename = assetFolder + filename
	folder = out_path + filename.replace (filename.split('/')[-1], '')
	if os.path.exists(folder) == False:
		os.makedirs(folder)
	with open(out_path + filename, 'w') as fd:
		fd.write(context)

