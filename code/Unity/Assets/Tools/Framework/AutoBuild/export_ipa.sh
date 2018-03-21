# #!/bin/bash
#工程路径
project_path=$1
#IPA名称
ipa_name=$2
#build文件夹路径
build_path=${project_path}/build
cd $project_path
#清理
xcodebuild clean
#编译工程
xcodebuild -sdk iphoneos -scheme Unity-iPhone -archivePath build/${ipa_name}.xcarchive archive
#打包
xcrun xcodebuild -exportArchive -exportOptionsPlist $4 -archivePath build/${ipa_name}.xcarchive -exportPath .. >> $3
#改名
mv ../Unity-iPhone.ipa ../${ipa_name}.ipa >> $3


#fir login
/usr/local/bin/fir login 8c0c0e5b7ba26233f3138e7c4558b638 >> $3
#fir 发布
/usr/local/bin/fir publish ../${ipa_name}.ipa >> $3
