#!/bin/bash
# server=build.palaso.org
# project=Bloom
# build=Bloom-Default-Linux64-Continuous
# root_dir=..
# Auto-generated by https://github.com/chrisvire/BuildUpdate.
# Do not edit this file by hand!

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
f1=$(basename $1)
f2=$(basename $2)
cd $(dirname $2)
wget -q -L -N $1
# wget has no true equivalent of curl's -o option.
# Different versions of wget handle (or not) % escaping differently.
# A URL query is the only reason why $f1 and $f2 should differ.
if [ "$f1" != "$f2" ]; then mv $f2\?* $f2; fi
cd -
}


# *** Results ***
# build: Bloom-3.8-Linux64-Continuous (Bloom_Bloom38Linux64Continuous)
# project: Bloom
# URL: http://build.palaso.org/viewType.html?buildTypeId=Bloom_Bloom38Linux64Continuous
# VCS: git://github.com/BloomBooks/BloomDesktop.git [master]
# dependencies:
# [0] build: bloom-win32-static-dependencies (bt396)
#     project: Bloom
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt396
#     clean: false
#     revision: bloom-3.8.tcbuildtag
#     paths: {"connections.dll"=>"DistFiles", "*.chm"=>"DistFiles", "MSBuild.Community.Tasks.dll"=>"build/", "MSBuild.Community.Tasks.Targets"=>"build/"}
# [1] build: BloomPlayer-Master-Continuous (BPContinuous)
#     project: Bloom
#     URL: http://build.palaso.org/viewType.html?buildTypeId=BPContinuous
#     clean: false
#     revision: bloom-3.8.tcbuildtag
#     paths: {"*.*"=>"DistFiles/"}
#     VCS: https://github.com/BloomBooks/BloomPlayer [refs/heads/master]
# [2] build: Squirrel (Bloom_Squirrel)
#     project: Bloom
#     URL: http://build.palaso.org/viewType.html?buildTypeId=Bloom_Squirrel
#     clean: false
#     revision: bloom-3.8.tcbuildtag
#     paths: {"ICSharpCode.SharpZipLib.*"=>"lib/dotnet"}
#     VCS: https://github.com/BloomBooks/Squirrel.Windows.git [refs/heads/master]
# [3] build: YouTrackSharp (Bloom_YouTrackSharp)
#     project: Bloom
#     URL: http://build.palaso.org/viewType.html?buildTypeId=Bloom_YouTrackSharp
#     clean: false
#     revision: bloom-3.8.tcbuildtag
#     paths: {"bin/YouTrackSharp.dll"=>"lib/dotnet", "bin/YouTrackSharp.pdb"=>"lib/dotnet"}
#     VCS: https://github.com/BloomBooks/YouTrackSharp.git [LinuxCompatible]
# [4] build: pdf.js (bt401)
#     project: BuildTasks
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt401
#     clean: false
#     revision: bloom-3.8.tcbuildtag
#     paths: {"pdfjs-viewer.zip!**"=>"DistFiles/pdf"}
#     VCS: https://github.com/mozilla/pdf.js.git [gh-pages]
# [5] build: GeckofxHtmlToPdf-trusty64-continuous (GeckofxHtmlToPdfTrusty64)
#     project: GeckofxHtmlToPdf
#     URL: http://build.palaso.org/viewType.html?buildTypeId=GeckofxHtmlToPdfTrusty64
#     clean: false
#     revision: bloom-3.8.tcbuildtag
#     paths: {"Args.dll"=>"lib/dotnet", "GeckofxHtmlToPdf.exe"=>"lib/dotnet", "GeckofxHtmlToPdf.exe.config"=>"lib/dotnet"}
#     VCS: https://github.com/BloomBooks/geckofxHtmlToPdf [refs/heads/master]
# [6] build: icucil-precise64-Continuous (bt281)
#     project: Libraries
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt281
#     clean: false
#     revision: bloom-3.8.tcbuildtag
#     paths: {"icu.net.*"=>"lib/dotnet/icu48"}
#     VCS: https://github.com/sillsdev/icu-dotnet [master]
# [7] build: icucil-precise64-icu55 Continuous (Icu55)
#     project: Libraries
#     URL: http://build.palaso.org/viewType.html?buildTypeId=Icu55
#     clean: false
#     revision: bloom-3.8.tcbuildtag
#     paths: {"icu.net.*"=>"lib/dotnet/icu55"}
#     VCS: https://github.com/sillsdev/icu-dotnet [master]
# [8] build: icucil-precise64-icu52 Continuous (bt413)
#     project: Archived
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt413
#     clean: false
#     revision: bloom-3.8.tcbuildtag
#     paths: {"icu.net.*"=>"lib/dotnet/icu52"}
#     VCS: https://github.com/sillsdev/icu-dotnet [master]
# [9] build: PdfDroplet-Linux-Dev-Continuous (bt344)
#     project: PdfDroplet
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt344
#     clean: false
#     revision: bloom-3.8.tcbuildtag
#     paths: {"PdfDroplet.exe"=>"lib/dotnet", "PdfSharp.dll*"=>"lib/dotnet"}
#     VCS: https://github.com/sillsdev/pdfDroplet [master]
# [10] build: TidyManaged-master-precise64-continuous (bt351)
#     project: TidyManaged
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt351
#     clean: false
#     revision: bloom-3.8.tcbuildtag
#     paths: {"TidyManaged.dll*"=>"lib/dotnet"}
#     VCS: https://github.com/BloomBooks/TidyManaged.git [master]
# [11] build: palaso-precise64-master Continuous (bt322)
#     project: libpalaso
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt322
#     clean: false
#     revision: bloom-3.8.tcbuildtag
#     paths: {"Palaso.BuildTasks.dll"=>"build/", "*.dll*"=>"lib/dotnet"}
#     VCS: https://github.com/sillsdev/libpalaso.git [master]

# make sure output directories exist
mkdir -p ../DistFiles
mkdir -p ../DistFiles/
mkdir -p ../DistFiles/pdf
mkdir -p ../Downloads
mkdir -p ../build/
mkdir -p ../lib/dotnet
mkdir -p ../lib/dotnet/icu48
mkdir -p ../lib/dotnet/icu52
mkdir -p ../lib/dotnet/icu55

# download artifact dependencies
copy_auto http://build.palaso.org/guestAuth/repository/download/bt396/bloom-3.8.tcbuildtag/connections.dll ../DistFiles/connections.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt396/bloom-3.8.tcbuildtag/Bloom.chm ../DistFiles/Bloom.chm
copy_auto http://build.palaso.org/guestAuth/repository/download/bt396/bloom-3.8.tcbuildtag/MSBuild.Community.Tasks.dll ../build/MSBuild.Community.Tasks.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt396/bloom-3.8.tcbuildtag/MSBuild.Community.Tasks.Targets ../build/MSBuild.Community.Tasks.Targets
copy_auto http://build.palaso.org/guestAuth/repository/download/BPContinuous/bloom-3.8.tcbuildtag/bloomPlayer.js ../DistFiles/bloomPlayer.js
copy_auto http://build.palaso.org/guestAuth/repository/download/Bloom_Squirrel/bloom-3.8.tcbuildtag/ICSharpCode.SharpZipLib.dll ../lib/dotnet/ICSharpCode.SharpZipLib.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Bloom_Squirrel/bloom-3.8.tcbuildtag/ICSharpCode.SharpZipLib.xml ../lib/dotnet/ICSharpCode.SharpZipLib.xml
copy_auto http://build.palaso.org/guestAuth/repository/download/Bloom_YouTrackSharp/bloom-3.8.tcbuildtag/bin/YouTrackSharp.dll ../lib/dotnet/YouTrackSharp.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Bloom_YouTrackSharp/bloom-3.8.tcbuildtag/bin/YouTrackSharp.pdb ../lib/dotnet/YouTrackSharp.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt401/bloom-3.8.tcbuildtag/pdfjs-viewer.zip ../Downloads/pdfjs-viewer.zip
copy_auto http://build.palaso.org/guestAuth/repository/download/GeckofxHtmlToPdfTrusty64/bloom-3.8.tcbuildtag/Args.dll ../lib/dotnet/Args.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/GeckofxHtmlToPdfTrusty64/bloom-3.8.tcbuildtag/GeckofxHtmlToPdf.exe ../lib/dotnet/GeckofxHtmlToPdf.exe
copy_auto http://build.palaso.org/guestAuth/repository/download/GeckofxHtmlToPdfTrusty64/bloom-3.8.tcbuildtag/GeckofxHtmlToPdf.exe.config ../lib/dotnet/GeckofxHtmlToPdf.exe.config
copy_auto http://build.palaso.org/guestAuth/repository/download/bt281/bloom-3.8.tcbuildtag/icu.net.0.0.0.0.nupkg ../lib/dotnet/icu48/icu.net.0.0.0.0.nupkg
copy_auto http://build.palaso.org/guestAuth/repository/download/bt281/bloom-3.8.tcbuildtag/icu.net.dll ../lib/dotnet/icu48/icu.net.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt281/bloom-3.8.tcbuildtag/icu.net.dll.config ../lib/dotnet/icu48/icu.net.dll.config
copy_auto http://build.palaso.org/guestAuth/repository/download/bt281/bloom-3.8.tcbuildtag/icu.net.dll.mdb ../lib/dotnet/icu48/icu.net.dll.mdb
copy_auto http://build.palaso.org/guestAuth/repository/download/Icu55/bloom-3.8.tcbuildtag/icu.net.0.0.0.0.nupkg ../lib/dotnet/icu55/icu.net.0.0.0.0.nupkg
copy_auto http://build.palaso.org/guestAuth/repository/download/Icu55/bloom-3.8.tcbuildtag/icu.net.dll ../lib/dotnet/icu55/icu.net.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Icu55/bloom-3.8.tcbuildtag/icu.net.dll.config ../lib/dotnet/icu55/icu.net.dll.config
copy_auto http://build.palaso.org/guestAuth/repository/download/Icu55/bloom-3.8.tcbuildtag/icu.net.dll.mdb ../lib/dotnet/icu55/icu.net.dll.mdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt413/bloom-3.8.tcbuildtag/icu.net.dll ../lib/dotnet/icu52/icu.net.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt413/bloom-3.8.tcbuildtag/icu.net.dll.config ../lib/dotnet/icu52/icu.net.dll.config
copy_auto http://build.palaso.org/guestAuth/repository/download/bt413/bloom-3.8.tcbuildtag/icu.net.dll.mdb ../lib/dotnet/icu52/icu.net.dll.mdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt344/bloom-3.8.tcbuildtag/PdfDroplet.exe ../lib/dotnet/PdfDroplet.exe
copy_auto http://build.palaso.org/guestAuth/repository/download/bt344/bloom-3.8.tcbuildtag/PdfSharp.dll ../lib/dotnet/PdfSharp.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt351/bloom-3.8.tcbuildtag/TidyManaged.dll ../lib/dotnet/TidyManaged.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt351/bloom-3.8.tcbuildtag/TidyManaged.dll.config ../lib/dotnet/TidyManaged.dll.config
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/bloom-3.8.tcbuildtag/Palaso.BuildTasks.dll ../build/Palaso.BuildTasks.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/bloom-3.8.tcbuildtag/Commons.Xml.Relaxng.dll ../lib/dotnet/Commons.Xml.Relaxng.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/bloom-3.8.tcbuildtag/Enchant.Net.dll ../lib/dotnet/Enchant.Net.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/bloom-3.8.tcbuildtag/Enchant.Net.dll.config ../lib/dotnet/Enchant.Net.dll.config
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/bloom-3.8.tcbuildtag/Ionic.Zip.dll ../lib/dotnet/Ionic.Zip.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/bloom-3.8.tcbuildtag/L10NSharp.dll ../lib/dotnet/L10NSharp.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/bloom-3.8.tcbuildtag/NDesk.DBus.dll ../lib/dotnet/NDesk.DBus.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/bloom-3.8.tcbuildtag/NDesk.DBus.dll.config ../lib/dotnet/NDesk.DBus.dll.config
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/bloom-3.8.tcbuildtag/Newtonsoft.Json.dll ../lib/dotnet/Newtonsoft.Json.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/bloom-3.8.tcbuildtag/Palaso.BuildTasks.dll ../lib/dotnet/Palaso.BuildTasks.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/bloom-3.8.tcbuildtag/SIL.Archiving.dll ../lib/dotnet/SIL.Archiving.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/bloom-3.8.tcbuildtag/SIL.Archiving.dll.config ../lib/dotnet/SIL.Archiving.dll.config
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/bloom-3.8.tcbuildtag/SIL.Archiving.dll.mdb ../lib/dotnet/SIL.Archiving.dll.mdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/bloom-3.8.tcbuildtag/SIL.Core.Tests.dll ../lib/dotnet/SIL.Core.Tests.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/bloom-3.8.tcbuildtag/SIL.Core.Tests.dll.mdb ../lib/dotnet/SIL.Core.Tests.dll.mdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/bloom-3.8.tcbuildtag/SIL.Core.dll ../lib/dotnet/SIL.Core.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/bloom-3.8.tcbuildtag/SIL.Core.dll.config ../lib/dotnet/SIL.Core.dll.config
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/bloom-3.8.tcbuildtag/SIL.Core.dll.mdb ../lib/dotnet/SIL.Core.dll.mdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/bloom-3.8.tcbuildtag/SIL.DictionaryServices.dll ../lib/dotnet/SIL.DictionaryServices.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/bloom-3.8.tcbuildtag/SIL.DictionaryServices.dll.mdb ../lib/dotnet/SIL.DictionaryServices.dll.mdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/bloom-3.8.tcbuildtag/SIL.Lexicon.dll ../lib/dotnet/SIL.Lexicon.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/bloom-3.8.tcbuildtag/SIL.Lexicon.dll.mdb ../lib/dotnet/SIL.Lexicon.dll.mdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/bloom-3.8.tcbuildtag/SIL.Lift.dll ../lib/dotnet/SIL.Lift.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/bloom-3.8.tcbuildtag/SIL.Lift.dll.mdb ../lib/dotnet/SIL.Lift.dll.mdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/bloom-3.8.tcbuildtag/SIL.Media.dll ../lib/dotnet/SIL.Media.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/bloom-3.8.tcbuildtag/SIL.Media.dll.config ../lib/dotnet/SIL.Media.dll.config
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/bloom-3.8.tcbuildtag/SIL.Media.dll.mdb ../lib/dotnet/SIL.Media.dll.mdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/bloom-3.8.tcbuildtag/SIL.Scripture.dll ../lib/dotnet/SIL.Scripture.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/bloom-3.8.tcbuildtag/SIL.Scripture.dll.mdb ../lib/dotnet/SIL.Scripture.dll.mdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/bloom-3.8.tcbuildtag/SIL.TestUtilities.dll ../lib/dotnet/SIL.TestUtilities.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/bloom-3.8.tcbuildtag/SIL.TestUtilities.dll.mdb ../lib/dotnet/SIL.TestUtilities.dll.mdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/bloom-3.8.tcbuildtag/SIL.Windows.Forms.GeckoBrowserAdapter.dll ../lib/dotnet/SIL.Windows.Forms.GeckoBrowserAdapter.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/bloom-3.8.tcbuildtag/SIL.Windows.Forms.GeckoBrowserAdapter.dll.mdb ../lib/dotnet/SIL.Windows.Forms.GeckoBrowserAdapter.dll.mdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/bloom-3.8.tcbuildtag/SIL.Windows.Forms.Keyboarding.dll ../lib/dotnet/SIL.Windows.Forms.Keyboarding.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/bloom-3.8.tcbuildtag/SIL.Windows.Forms.Keyboarding.dll.config ../lib/dotnet/SIL.Windows.Forms.Keyboarding.dll.config
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/bloom-3.8.tcbuildtag/SIL.Windows.Forms.Keyboarding.dll.mdb ../lib/dotnet/SIL.Windows.Forms.Keyboarding.dll.mdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/bloom-3.8.tcbuildtag/SIL.Windows.Forms.Scripture.dll ../lib/dotnet/SIL.Windows.Forms.Scripture.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/bloom-3.8.tcbuildtag/SIL.Windows.Forms.Scripture.dll.mdb ../lib/dotnet/SIL.Windows.Forms.Scripture.dll.mdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/bloom-3.8.tcbuildtag/SIL.Windows.Forms.WritingSystems.dll ../lib/dotnet/SIL.Windows.Forms.WritingSystems.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/bloom-3.8.tcbuildtag/SIL.Windows.Forms.WritingSystems.dll.mdb ../lib/dotnet/SIL.Windows.Forms.WritingSystems.dll.mdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/bloom-3.8.tcbuildtag/SIL.Windows.Forms.dll ../lib/dotnet/SIL.Windows.Forms.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/bloom-3.8.tcbuildtag/SIL.Windows.Forms.dll.config ../lib/dotnet/SIL.Windows.Forms.dll.config
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/bloom-3.8.tcbuildtag/SIL.Windows.Forms.dll.mdb ../lib/dotnet/SIL.Windows.Forms.dll.mdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/bloom-3.8.tcbuildtag/SIL.WritingSystems.Tests.dll ../lib/dotnet/SIL.WritingSystems.Tests.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/bloom-3.8.tcbuildtag/SIL.WritingSystems.Tests.dll.mdb ../lib/dotnet/SIL.WritingSystems.Tests.dll.mdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/bloom-3.8.tcbuildtag/SIL.WritingSystems.dll ../lib/dotnet/SIL.WritingSystems.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/bloom-3.8.tcbuildtag/SIL.WritingSystems.dll.mdb ../lib/dotnet/SIL.WritingSystems.dll.mdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/bloom-3.8.tcbuildtag/Spart.dll ../lib/dotnet/Spart.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/bloom-3.8.tcbuildtag/ibusdotnet.dll ../lib/dotnet/ibusdotnet.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/bloom-3.8.tcbuildtag/icu.net.dll ../lib/dotnet/icu.net.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/bloom-3.8.tcbuildtag/icu.net.dll.config ../lib/dotnet/icu.net.dll.config
copy_auto http://build.palaso.org/guestAuth/repository/download/bt322/bloom-3.8.tcbuildtag/taglib-sharp.dll ../lib/dotnet/taglib-sharp.dll
# extract downloaded zip files
unzip -uqo ../Downloads/pdfjs-viewer.zip -d ../DistFiles/pdf
# End of script
