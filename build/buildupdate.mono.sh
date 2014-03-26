#!/bin/bash
# server=build.palaso.org
# project=WeSay1.4-Linux
# build=wesay1.4-precise64-continuous
# root_dir=..
# $Id: 8fa3d81233f94c653ca4357ed5e300790d8bc404 $

cd "$(dirname "$0")"

# *** Functions ***
force=0
clean=0

while getopts fc opt; do
case $opt in
f) force=1 ;;
c) clean=1 ;;
esac
done

shift $((OPTIND - 1))

copy_auto() {
if [ "$clean" == "1" ]
then
echo cleaning $2
rm -f ""$2""
else
where_curl=$(type -P curl)
where_wget=$(type -P wget)
if [ "$where_curl" != "" ]
then
copy_curl $1 $2
elif [ "$where_wget" != "" ]
then
copy_wget $1 $2
else
echo "Missing curl or wget"
exit 1
fi
fi
}

copy_curl() {
echo "curl: $2 <= $1"
if [ -e "$2" ] && [ "$force" != "1" ]
then
curl -# -L -z $2 -o $2 $1
else
curl -# -L -o $2 $1
fi
}

copy_wget() {
echo "wget: $2 <= $1"
f=$(basename $2)
d=$(dirname $2)
cd $d
wget -q -L -N $1
cd -
}


# *** Results ***
# build: wesay1.4-precise64-continuous (bt314)
# project: WeSay1.4-Linux
# URL: http://build.palaso.org/viewType.html?buildTypeId=bt314
# VCS: https://github.com/sillsdev/wesay [develop]
# dependencies:
# [0] build: chorus-precise64-master Continuous (bt323)
#     project: Chorus
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt323
#     clean: false
#     revision: pre-tmx-migration.tcbuildtag
#     paths: {"Chorus.exe"=>"lib/Release", "Chorus.exe.mdb"=>"lib/Release", "ChorusHub.exe"=>"lib/Release", "ChorusHub.exe.mdb"=>"lib/Release", "ChorusMerge.exe"=>"lib/Release", "ChorusMerge.exe.mdb"=>"lib/Release", "LibChorus.dll"=>"lib/Release", "LibChorus.dll.mdb"=>"lib/Release", "LibChorus.TestUtilities.dll"=>"lib/Release", "LibChorus.TestUtilities.dll.mdb"=>"lib/Release", "Autofac.dll"=>"lib/Release", "NDesk.DBus.dll"=>"lib/Release", "NDesk.DBus.dll.config"=>"lib/Release", "geckofix.so"=>"lib/Release", "geckofx-core-14.dll"=>"lib/Release", "geckofx-core-14.dll.config"=>"lib/Release", "Geckofx-Winforms-14.dll"=>"lib/Release", "debug/**"=>"lib/Debug", "Mercurial-i686.zip"=>"lib/common", "Mercurial-x86_64.zip"=>"lib/common"}
#     VCS: https://github.com/sillsdev/chorus.git [master]
# [1] build: L10NSharp Mono continuous (bt271)
#     project: L10NSharp
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt271
#     clean: false
#     revision: pre-tmx-migration.tcbuildtag
#     paths: {"L10NSharp.dll"=>"lib/Release", "L10NSharp.dll.mdb"=>"lib/Release"}
#     VCS: https://bitbucket.org/hatton/l10nsharp []
# [2] build: L10NSharp Mono continuous (bt271)
#     project: L10NSharp
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt271
#     clean: false
#     revision: pre-tmx-migration.tcbuildtag
#     paths: {"L10NSharp.dll"=>"lib/Debug", "L10NSharp.dll.mdb"=>"lib/Debug"}
#     VCS: https://bitbucket.org/hatton/l10nsharp []
# [3] build: palaso-precise64-master Continuous (bt322)
#     project: libpalaso
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt322
#     clean: false
#     revision: pre-tmx-migration.tcbuildtag
#     paths: {"Enchant.Net.dll"=>"lib/Release", "Enchant.Net.dll.config"=>"lib/Release", "ibusdotnet.dll"=>"lib/Release", "Palaso.dll"=>"lib/Release", "Palaso.dll.mdb"=>"lib/Release", "Palaso.dll.config"=>"lib/Release", "Palaso.DictionaryServices.dll"=>"lib/Release", "Palaso.DictionaryServices.dll.mdb"=>"lib/Release", "Palaso.Lift.dll"=>"lib/Release", "Palaso.Lift.dll.mdb"=>"lib/Release", "Palaso.Media.dll"=>"lib/Release", "Palaso.Media.dll.mdb"=>"lib/Release", "Palaso.Media.dll.config"=>"lib/Release", "Palaso.Tests.dll"=>"lib/Release", "Palaso.Tests.dll.mdb"=>"lib/Release", "Palaso.TestUtilities.dll"=>"lib/Release", "Palaso.TestUtilities.dll.mdb"=>"lib/Release", "PalasoUIWindowsForms.dll"=>"lib/Release", "PalasoUIWindowsForms.dll.mdb"=>"lib/Release", "PalasoUIWindowsForms.dll.config"=>"lib/Release", "SIL.Archiving.dll"=>"lib/Release", "SIL.Archiving.dll.config"=>"lib/Release", "SIL.Archiving.dll.mdb"=>"lib/Release", "debug/**"=>"lib/Debug", "Ionic.Zip.dll"=>"lib/Release"}
#     VCS: https://github.com/sillsdev/libpalaso.git [master]
# [4] build: icucil-precise64-Continuous (bt281)
#     project: Libraries
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt281
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"icu.net.*"=>"lib/Release"}
#     VCS: https://github.com/sillsdev/icu-dotnet [master]
# [5] build: icucil-precise64-Continuous (bt281)
#     project: Libraries
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt281
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"icu.net.*"=>"lib/Debug"}
#     VCS: https://github.com/sillsdev/icu-dotnet [master]
# [6] build: wesay-doc-default (bt184)
#     project: WeSay Windows
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt184
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"WeSay_Helps.chm"=>"common"}
#     VCS: http://hg.palaso.org/wesay-doc []

# make sure output directories exist
mkdir -p ../lib/Release
mkdir -p ../lib/Debug
mkdir -p ../lib/common
mkdir -p ../common

# download artifact dependencies
copy_auto http://build.palaso.org/guestAuth/repository/download/bt323/pre-tmx-migration.tcbuildtag/Chorus.exe ../lib/Release/Chorus.exe
copy_auto http://build.palaso.org/guestAuth/repository/download/bt323/pre-tmx-migration.tcbuildtag/Chorus.exe.mdb ../lib/Release/Chorus.exe.mdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt323/pre-tmx-migration.tcbuildtag/ChorusHub.exe ../lib/Release/ChorusHub.exe
copy_auto http://build.palaso.org/guestAuth/repository/download/bt323/pre-tmx-migration.tcbuildtag/ChorusHub.exe.mdb ../lib/Release/ChorusHub.exe.mdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt323/pre-tmx-migration.tcbuildtag/ChorusMerge.exe ../lib/Release/ChorusMerge.exe
copy_auto http://build.palaso.org/guestAuth/repository/download/bt323/pre-tmx-migration.tcbuildtag/ChorusMerge.exe.mdb ../lib/Release/ChorusMerge.exe.mdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt323/pre-tmx-migration.tcbuildtag/LibChorus.dll ../lib/Release/LibChorus.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt323/pre-tmx-migration.tcbuildtag/LibChorus.dll.mdb ../lib/Release/LibChorus.dll.mdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt323/pre-tmx-migration.tcbuildtag/LibChorus.TestUtilities.dll ../lib/Release/LibChorus.TestUtilities.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt323/pre-tmx-migration.tcbuildtag/LibChorus.TestUtilities.dll.mdb ../lib/Release/LibChorus.TestUtilities.dll.mdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt323/pre-tmx-migration.tcbuildtag/Autofac.dll ../lib/Release/Autofac.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt323/pre-tmx-migration.tcbuildtag/NDesk.DBus.dll ../lib/Release/NDesk.DBus.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt323/pre-tmx-migration.tcbuildtag/NDesk.DBus.dll.config ../lib/Release/NDesk.DBus.dll.config
copy_auto http://build.palaso.org/guestAuth/repository/download/bt323/pre-tmx-migration.tcbuildtag/geckofix.so ../lib/Release/geckofix.so
copy_auto http://build.palaso.org/guestAuth/repository/download/bt323/pre-tmx-migration.tcbuildtag/geckofx-core-14.dll ../lib/Release/geckofx-core-14.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt323/pre-tmx-migration.tcbuildtag/geckofx-core-14.dll.config ../lib/Release/geckofx-core-14.dll.config
copy_auto http://build.palaso.org/guestAuth/repository/download/bt323/pre-tmx-migration.tcbuildtag/Geckofx-Winforms-14.dll ../lib/Release/Geckofx-Winforms-14.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt323/pre-tmx-migration.tcbuildtag/debug/Autofac.dll ../lib/Debug/Autofac.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt323/pre-tmx-migration.tcbuildtag/debug/Chorus.exe ../lib/Debug/Chorus.exe
copy_auto http://build.palaso.org/guestAuth/repository/download/bt323/pre-tmx-migration.tcbuildtag/debug/Chorus.exe.mdb ../lib/Debug/Chorus.exe.mdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt323/pre-tmx-migration.tcbuildtag/debug/ChorusHub.exe ../lib/Debug/ChorusHub.exe
copy_auto http://build.palaso.org/guestAuth/repository/download/bt323/pre-tmx-migration.tcbuildtag/debug/ChorusHub.exe.mdb ../lib/Debug/ChorusHub.exe.mdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt323/pre-tmx-migration.tcbuildtag/debug/ChorusMerge.exe ../lib/Debug/ChorusMerge.exe
copy_auto http://build.palaso.org/guestAuth/repository/download/bt323/pre-tmx-migration.tcbuildtag/debug/ChorusMerge.exe.mdb ../lib/Debug/ChorusMerge.exe.mdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt323/pre-tmx-migration.tcbuildtag/debug/Geckofx-Winforms-14.dll ../lib/Debug/Geckofx-Winforms-14.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt323/pre-tmx-migration.tcbuildtag/debug/LibChorus.TestUtilities.dll ../lib/Debug/LibChorus.TestUtilities.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt323/pre-tmx-migration.tcbuildtag/debug/LibChorus.TestUtilities.dll.mdb ../lib/Debug/LibChorus.TestUtilities.dll.mdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt323/pre-tmx-migration.tcbuildtag/debug/LibChorus.dll ../lib/Debug/LibChorus.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt323/pre-tmx-migration.tcbuildtag/debug/LibChorus.dll.mdb ../lib/Debug/LibChorus.dll.mdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt323/pre-tmx-migration.tcbuildtag/debug/NDesk.DBus.dll ../lib/Debug/NDesk.DBus.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt323/pre-tmx-migration.tcbuildtag/debug/NDesk.DBus.dll.config ../lib/Debug/NDesk.DBus.dll.config
copy_auto http://build.palaso.org/guestAuth/repository/download/bt323/pre-tmx-migration.tcbuildtag/debug/geckofix.so ../lib/Debug/geckofix.so
copy_auto http://build.palaso.org/guestAuth/repository/download/bt323/pre-tmx-migration.tcbuildtag/debug/geckofx-core-14.dll ../lib/Debug/geckofx-core-14.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt323/pre-tmx-migration.tcbuildtag/debug/geckofx-core-14.dll.config ../lib/Debug/geckofx-core-14.dll.config
copy_auto http://build.palaso.org/guestAuth/repository/download/bt323/pre-tmx-migration.tcbuildtag/Mercurial-i686.zip ../lib/common/Mercurial-i686.zip
copy_auto http://build.palaso.org/guestAuth/repository/download/bt323/pre-tmx-migration.tcbuildtag/Mercurial-x86_64.zip ../lib/common/Mercurial-x86_64.zip
copy_auto http://build.palaso.org/guestAuth/repository/download/bt271/pre-tmx-migration.tcbuildtag/L10NSharp.dll ../lib/Release/L10NSharp.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt271/pre-tmx-migration.tcbuildtag/L10NSharp.dll.mdb ../lib/Release/L10NSharp.dll.mdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt271/pre-tmx-migration.tcbuildtag/L10NSharp.dll ../lib/Debug/L10NSharp.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt271/pre-tmx-migration.tcbuildtag/L10NSharp.dll.mdb ../lib/Debug/L10NSharp.dll.mdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/pre-tmx-migration.tcbuildtag/Enchant.Net.dll ../lib/Release/Enchant.Net.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/pre-tmx-migration.tcbuildtag/Enchant.Net.dll.config ../lib/Release/Enchant.Net.dll.config
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/pre-tmx-migration.tcbuildtag/ibusdotnet.dll ../lib/Release/ibusdotnet.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/pre-tmx-migration.tcbuildtag/Palaso.dll ../lib/Release/Palaso.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/pre-tmx-migration.tcbuildtag/Palaso.dll.mdb ../lib/Release/Palaso.dll.mdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/pre-tmx-migration.tcbuildtag/Palaso.dll.config ../lib/Release/Palaso.dll.config
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/pre-tmx-migration.tcbuildtag/Palaso.DictionaryServices.dll ../lib/Release/Palaso.DictionaryServices.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/pre-tmx-migration.tcbuildtag/Palaso.DictionaryServices.dll.mdb ../lib/Release/Palaso.DictionaryServices.dll.mdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/pre-tmx-migration.tcbuildtag/Palaso.Lift.dll ../lib/Release/Palaso.Lift.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/pre-tmx-migration.tcbuildtag/Palaso.Lift.dll.mdb ../lib/Release/Palaso.Lift.dll.mdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/pre-tmx-migration.tcbuildtag/Palaso.Media.dll ../lib/Release/Palaso.Media.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/pre-tmx-migration.tcbuildtag/Palaso.Media.dll.mdb ../lib/Release/Palaso.Media.dll.mdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/pre-tmx-migration.tcbuildtag/Palaso.Media.dll.config ../lib/Release/Palaso.Media.dll.config
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/pre-tmx-migration.tcbuildtag/Palaso.Tests.dll ../lib/Release/Palaso.Tests.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/pre-tmx-migration.tcbuildtag/Palaso.Tests.dll.mdb ../lib/Release/Palaso.Tests.dll.mdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/pre-tmx-migration.tcbuildtag/Palaso.TestUtilities.dll ../lib/Release/Palaso.TestUtilities.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/pre-tmx-migration.tcbuildtag/Palaso.TestUtilities.dll.mdb ../lib/Release/Palaso.TestUtilities.dll.mdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/pre-tmx-migration.tcbuildtag/PalasoUIWindowsForms.dll ../lib/Release/PalasoUIWindowsForms.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/pre-tmx-migration.tcbuildtag/PalasoUIWindowsForms.dll.mdb ../lib/Release/PalasoUIWindowsForms.dll.mdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/pre-tmx-migration.tcbuildtag/PalasoUIWindowsForms.dll.config ../lib/Release/PalasoUIWindowsForms.dll.config
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/pre-tmx-migration.tcbuildtag/SIL.Archiving.dll ../lib/Release/SIL.Archiving.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/pre-tmx-migration.tcbuildtag/SIL.Archiving.dll.config ../lib/Release/SIL.Archiving.dll.config
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/pre-tmx-migration.tcbuildtag/SIL.Archiving.dll.mdb ../lib/Release/SIL.Archiving.dll.mdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/pre-tmx-migration.tcbuildtag/debug/Enchant.Net.dll ../lib/Debug/Enchant.Net.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/pre-tmx-migration.tcbuildtag/debug/Enchant.Net.dll.config ../lib/Debug/Enchant.Net.dll.config
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/pre-tmx-migration.tcbuildtag/debug/Ionic.Zip.dll ../lib/Debug/Ionic.Zip.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/pre-tmx-migration.tcbuildtag/debug/L10NSharp.dll ../lib/Debug/L10NSharp.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/pre-tmx-migration.tcbuildtag/debug/NDesk.DBus.dll ../lib/Debug/NDesk.DBus.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/pre-tmx-migration.tcbuildtag/debug/NDesk.DBus.dll.config ../lib/Debug/NDesk.DBus.dll.config
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/pre-tmx-migration.tcbuildtag/debug/Palaso.BuildTasks.dll ../lib/Debug/Palaso.BuildTasks.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/pre-tmx-migration.tcbuildtag/debug/Palaso.DictionaryServices.dll ../lib/Debug/Palaso.DictionaryServices.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/pre-tmx-migration.tcbuildtag/debug/Palaso.DictionaryServices.dll.mdb ../lib/Debug/Palaso.DictionaryServices.dll.mdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/pre-tmx-migration.tcbuildtag/debug/Palaso.Lift.dll ../lib/Debug/Palaso.Lift.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/pre-tmx-migration.tcbuildtag/debug/Palaso.Lift.dll.mdb ../lib/Debug/Palaso.Lift.dll.mdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/pre-tmx-migration.tcbuildtag/debug/Palaso.Media.dll ../lib/Debug/Palaso.Media.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/pre-tmx-migration.tcbuildtag/debug/Palaso.Media.dll.config ../lib/Debug/Palaso.Media.dll.config
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/pre-tmx-migration.tcbuildtag/debug/Palaso.Media.dll.mdb ../lib/Debug/Palaso.Media.dll.mdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/pre-tmx-migration.tcbuildtag/debug/Palaso.TestUtilities.dll ../lib/Debug/Palaso.TestUtilities.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/pre-tmx-migration.tcbuildtag/debug/Palaso.TestUtilities.dll.mdb ../lib/Debug/Palaso.TestUtilities.dll.mdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/pre-tmx-migration.tcbuildtag/debug/Palaso.Tests.dll ../lib/Debug/Palaso.Tests.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/pre-tmx-migration.tcbuildtag/debug/Palaso.Tests.dll.mdb ../lib/Debug/Palaso.Tests.dll.mdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/pre-tmx-migration.tcbuildtag/debug/Palaso.dll ../lib/Debug/Palaso.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/pre-tmx-migration.tcbuildtag/debug/Palaso.dll.config ../lib/Debug/Palaso.dll.config
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/pre-tmx-migration.tcbuildtag/debug/Palaso.dll.mdb ../lib/Debug/Palaso.dll.mdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/pre-tmx-migration.tcbuildtag/debug/PalasoUIWindowsForms.dll ../lib/Debug/PalasoUIWindowsForms.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/pre-tmx-migration.tcbuildtag/debug/PalasoUIWindowsForms.dll.config ../lib/Debug/PalasoUIWindowsForms.dll.config
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/pre-tmx-migration.tcbuildtag/debug/PalasoUIWindowsForms.dll.mdb ../lib/Debug/PalasoUIWindowsForms.dll.mdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/pre-tmx-migration.tcbuildtag/debug/SIL.Archiving.dll ../lib/Debug/SIL.Archiving.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/pre-tmx-migration.tcbuildtag/debug/SIL.Archiving.dll.config ../lib/Debug/SIL.Archiving.dll.config
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/pre-tmx-migration.tcbuildtag/debug/SIL.Archiving.dll.mdb ../lib/Debug/SIL.Archiving.dll.mdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/pre-tmx-migration.tcbuildtag/debug/ibusdotnet.dll ../lib/Debug/ibusdotnet.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/pre-tmx-migration.tcbuildtag/debug/icu.net.dll ../lib/Debug/icu.net.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/pre-tmx-migration.tcbuildtag/debug/icu.net.dll.config ../lib/Debug/icu.net.dll.config
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/pre-tmx-migration.tcbuildtag/Ionic.Zip.dll ../lib/Release/Ionic.Zip.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt281/latest.lastSuccessful/icu.net.dll ../lib/Release/icu.net.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt281/latest.lastSuccessful/icu.net.dll.config ../lib/Release/icu.net.dll.config
copy_auto http://build.palaso.org/guestAuth/repository/download/bt281/latest.lastSuccessful/icu.net.dll.mdb ../lib/Release/icu.net.dll.mdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt281/latest.lastSuccessful/icu.net.dll ../lib/Debug/icu.net.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt281/latest.lastSuccessful/icu.net.dll.config ../lib/Debug/icu.net.dll.config
copy_auto http://build.palaso.org/guestAuth/repository/download/bt281/latest.lastSuccessful/icu.net.dll.mdb ../lib/Debug/icu.net.dll.mdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt184/latest.lastSuccessful/WeSay_Helps.chm ../common/WeSay_Helps.chm
# End of script
