#!/bin/bash
# server=build.palaso.org
# project=Bloom
# build=Bloom-Default-Continuous
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
f=$(basename $2)
d=$(dirname $2)
cd $d
wget -q -L -N $1
cd -
}

# clean destination directories
rm -rf ../src/BloomBrowserUI/bookEdit/js/libsynphony
rm -rf ../src/BloomBrowserUI/bookEdit/test/libsynphony

# *** Results ***
# build: Bloom-Default-Continuous (bt222)
# project: Bloom
# URL: http://build.palaso.org/viewType.html?buildTypeId=bt222
# VCS: git://github.com/BloomBooks/BloomDesktop.git [master]
# dependencies:
# [0] build: bloom-3.-win32-static-dependencies (bt396)
#     project: Bloom
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt396
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"geckofx.zip!**"=>"lib/dotnet", "optipng-0.7.4-win32/optipng.exe"=>"DistFiles", "*.chm"=>"DistFiles", "connections.dll"=>"DistFiles", "MSBuild.Community.Tasks.dll"=>"build", "MSBuild.Community.Tasks.Targets"=>"build"}
# [1] build: LibSynphony (bt394)
#     project: Bloom
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt394
#     clean: true
#     revision: latest.lastSuccessful
#     paths: {"libsynphony-js.zip!**"=>"src/BloomBrowserUI/bookEdit/js/libsynphony", "libsynphony-test-js.zip!**"=>"src/BloomBrowserUI/bookEdit/test/libsynphony"}
#     VCS: https://bitbucket.org/phillip_hopper/synphony [default]
# [2] build: Squirrel (Bloom_Squirrel)
#     project: Bloom
#     URL: http://build.palaso.org/viewType.html?buildTypeId=Bloom_Squirrel
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"*.*"=>"lib/dotnet"}
#     VCS: https://github.com/BloomBooks/Squirrel.Windows.git [refs/heads/master]
# [3] build: YouTrackSharp (Bloom_YouTrackSharp)
#     project: Bloom
#     URL: http://build.palaso.org/viewType.html?buildTypeId=Bloom_YouTrackSharp
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"bin/YouTrackSharp.dll"=>"lib/dotnet", "bin/YouTrackSharp.pdb"=>"lib/dotnet"}
#     VCS: https://github.com/phillip-hopper/YouTrackSharp.git [LinuxCompatible]
# [4] build: pdf.js (bt401)
#     project: BuildTasks
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt401
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"pdfjs-viewer.zip!**"=>"DistFiles/pdf"}
#     VCS: https://github.com/mozilla/pdf.js.git [gh-pages]
# [5] build: chorus-win32-master-nostrongname Continuous (bt437)
#     project: Chorus
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt437
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"policy_9_0_Microsoft_VC90_CRT_x86.msm"=>"build\\ChorusInstallerStuff", "Vulcan.Uczniowie.HelpProvider.dll"=>"output/release", "Microsoft_VC90_CRT_x86.msm"=>"build\\ChorusInstallerStuff", "ChorusMergeModule.msm"=>"build\\ChorusInstallerStuff", "*.exe"=>"lib/dotnet", "*.dll"=>"lib/dotnet", "Mercurial.zip!**"=>".", "MercurialExtensions/**"=>"MercurialExtensions"}
#     VCS: https://github.com/sillsdev/chorus.git [master]
# [6] build: XulRunner29-win32 (bt400)
#     project: GeckoFx
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt400
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"xulrunner-29.0.1.en-US.win32.zip!**"=>"lib"}
# [7] build: GeckofxHtmlToPdf-Win32-continuous (bt463)
#     project: GeckofxHtmlToPdf
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt463
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"Args.dll"=>"lib/dotnet", "GeckofxHtmlToPdf.exe"=>"lib/dotnet", "GeckofxHtmlToPdf.exe.config"=>"lib/dotnet"}
#     VCS: https://github.com/hatton/geckofxHtmlToPdf [refs/heads/master]
# [8] build: PdfDroplet-Win-Dev-Continuous (bt54)
#     project: PdfDroplet
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt54
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"PdfDroplet.exe"=>"lib/dotnet", "PdfSharp.dll"=>"lib/dotnet"}
#     VCS: http://bitbucket.org/hatton/pdfdroplet [default]
# [9] build: TidyManaged-master-win32-continuous (bt349)
#     project: TidyManaged
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt349
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"*.*"=>"lib/dotnet"}
#     VCS: https://github.com/BloomBooks/TidyManaged.git [master]
# [10] build: palaso-win32-master-nostrongname Continuous (bt436)
#     project: libpalaso
#     URL: http://build.palaso.org/viewType.html?buildTypeId=bt436
#     clean: false
#     revision: latest.lastSuccessful
#     paths: {"Palaso.BuildTasks.dll"=>"build/", "*.dll"=>"lib/dotnet"}
#     VCS: https://github.com/sillsdev/libpalaso.git []

# make sure output directories exist
mkdir -p ../.
mkdir -p ../DistFiles
mkdir -p ../DistFiles/pdf
mkdir -p ../Downloads
mkdir -p ../MercurialExtensions
mkdir -p ../MercurialExtensions/fixutf8
mkdir -p ../build
mkdir -p ../build/
mkdir -p ../build/ChorusInstallerStuff
mkdir -p ../lib
mkdir -p ../lib/dotnet
mkdir -p ../output/release
mkdir -p ../src/BloomBrowserUI/bookEdit/js/libsynphony
mkdir -p ../src/BloomBrowserUI/bookEdit/test/libsynphony

# download artifact dependencies
copy_auto http://build.palaso.org/guestAuth/repository/download/bt396/latest.lastSuccessful/geckofx.zip ../Downloads/geckofx.zip
copy_auto http://build.palaso.org/guestAuth/repository/download/bt396/latest.lastSuccessful/optipng-0.7.4-win32/optipng.exe ../DistFiles/optipng.exe
copy_auto http://build.palaso.org/guestAuth/repository/download/bt396/latest.lastSuccessful/Bloom.chm ../DistFiles/Bloom.chm
copy_auto http://build.palaso.org/guestAuth/repository/download/bt396/latest.lastSuccessful/connections.dll ../DistFiles/connections.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt396/latest.lastSuccessful/MSBuild.Community.Tasks.dll ../build/MSBuild.Community.Tasks.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt396/latest.lastSuccessful/MSBuild.Community.Tasks.Targets ../build/MSBuild.Community.Tasks.Targets
copy_auto http://build.palaso.org/guestAuth/repository/download/bt394/latest.lastSuccessful/libsynphony-js.zip ../Downloads/libsynphony-js.zip
copy_auto http://build.palaso.org/guestAuth/repository/download/bt394/latest.lastSuccessful/libsynphony-test-js.zip ../Downloads/libsynphony-test-js.zip
copy_auto http://build.palaso.org/guestAuth/repository/download/Bloom_Squirrel/latest.lastSuccessful/DeltaCompressionDotNet.MsDelta.dll ../lib/dotnet/DeltaCompressionDotNet.MsDelta.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Bloom_Squirrel/latest.lastSuccessful/DeltaCompressionDotNet.PatchApi.dll ../lib/dotnet/DeltaCompressionDotNet.PatchApi.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Bloom_Squirrel/latest.lastSuccessful/DeltaCompressionDotNet.dll ../lib/dotnet/DeltaCompressionDotNet.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Bloom_Squirrel/latest.lastSuccessful/ICSharpCode.SharpZipLib.dll ../lib/dotnet/ICSharpCode.SharpZipLib.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Bloom_Squirrel/latest.lastSuccessful/ICSharpCode.SharpZipLib.xml ../lib/dotnet/ICSharpCode.SharpZipLib.xml
copy_auto http://build.palaso.org/guestAuth/repository/download/Bloom_Squirrel/latest.lastSuccessful/Microsoft.Web.XmlTransform.dll ../lib/dotnet/Microsoft.Web.XmlTransform.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Bloom_Squirrel/latest.lastSuccessful/Setup.exe ../lib/dotnet/Setup.exe
copy_auto http://build.palaso.org/guestAuth/repository/download/Bloom_Squirrel/latest.lastSuccessful/Splat.dll ../lib/dotnet/Splat.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Bloom_Squirrel/latest.lastSuccessful/Squirrel.dll ../lib/dotnet/Squirrel.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Bloom_Squirrel/latest.lastSuccessful/SyncReleases.exe ../lib/dotnet/SyncReleases.exe
copy_auto http://build.palaso.org/guestAuth/repository/download/Bloom_Squirrel/latest.lastSuccessful/Update.com ../lib/dotnet/Update.com
copy_auto http://build.palaso.org/guestAuth/repository/download/Bloom_Squirrel/latest.lastSuccessful/Update.exe ../lib/dotnet/Update.exe
copy_auto http://build.palaso.org/guestAuth/repository/download/Bloom_Squirrel/latest.lastSuccessful/WpfAnimatedGif.dll ../lib/dotnet/WpfAnimatedGif.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Bloom_Squirrel/latest.lastSuccessful/WpfAnimatedGif.xml ../lib/dotnet/WpfAnimatedGif.xml
copy_auto http://build.palaso.org/guestAuth/repository/download/Bloom_Squirrel/latest.lastSuccessful/rcedit.exe ../lib/dotnet/rcedit.exe
copy_auto http://build.palaso.org/guestAuth/repository/download/Bloom_Squirrel/latest.lastSuccessful/signtool.exe ../lib/dotnet/signtool.exe
copy_auto http://build.palaso.org/guestAuth/repository/download/Bloom_YouTrackSharp/latest.lastSuccessful/bin/YouTrackSharp.dll ../lib/dotnet/YouTrackSharp.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/Bloom_YouTrackSharp/latest.lastSuccessful/bin/YouTrackSharp.pdb ../lib/dotnet/YouTrackSharp.pdb
copy_auto http://build.palaso.org/guestAuth/repository/download/bt401/latest.lastSuccessful/pdfjs-viewer.zip ../Downloads/pdfjs-viewer.zip
copy_auto http://build.palaso.org/guestAuth/repository/download/bt437/latest.lastSuccessful/policy_9_0_Microsoft_VC90_CRT_x86.msm ../build/ChorusInstallerStuff/policy_9_0_Microsoft_VC90_CRT_x86.msm
copy_auto http://build.palaso.org/guestAuth/repository/download/bt437/latest.lastSuccessful/Vulcan.Uczniowie.HelpProvider.dll ../output/release/Vulcan.Uczniowie.HelpProvider.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt437/latest.lastSuccessful/Microsoft_VC90_CRT_x86.msm ../build/ChorusInstallerStuff/Microsoft_VC90_CRT_x86.msm
copy_auto http://build.palaso.org/guestAuth/repository/download/bt437/latest.lastSuccessful/ChorusMergeModule.msm ../build/ChorusInstallerStuff/ChorusMergeModule.msm
copy_auto http://build.palaso.org/guestAuth/repository/download/bt437/latest.lastSuccessful/Chorus.exe ../lib/dotnet/Chorus.exe
copy_auto http://build.palaso.org/guestAuth/repository/download/bt437/latest.lastSuccessful/ChorusHub.exe ../lib/dotnet/ChorusHub.exe
copy_auto http://build.palaso.org/guestAuth/repository/download/bt437/latest.lastSuccessful/ChorusHubApp.exe ../lib/dotnet/ChorusHubApp.exe
copy_auto http://build.palaso.org/guestAuth/repository/download/bt437/latest.lastSuccessful/ChorusMerge.exe ../lib/dotnet/ChorusMerge.exe
copy_auto http://build.palaso.org/guestAuth/repository/download/bt437/latest.lastSuccessful/Autofac.dll ../lib/dotnet/Autofac.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt437/latest.lastSuccessful/LibChorus.TestUtilities.dll ../lib/dotnet/LibChorus.TestUtilities.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt437/latest.lastSuccessful/LibChorus.dll ../lib/dotnet/LibChorus.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt437/latest.lastSuccessful/Palaso.Lift.dll ../lib/dotnet/Palaso.Lift.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt437/latest.lastSuccessful/Palaso.dll ../lib/dotnet/Palaso.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt437/latest.lastSuccessful/PalasoUIWindowsForms.dll ../lib/dotnet/PalasoUIWindowsForms.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt437/latest.lastSuccessful/Vulcan.Uczniowie.HelpProvider.dll ../lib/dotnet/Vulcan.Uczniowie.HelpProvider.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt437/latest.lastSuccessful/icu.net.dll ../lib/dotnet/icu.net.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt437/latest.lastSuccessful/icudt54.dll ../lib/dotnet/icudt54.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt437/latest.lastSuccessful/icuin54.dll ../lib/dotnet/icuin54.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt437/latest.lastSuccessful/icuuc54.dll ../lib/dotnet/icuuc54.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt437/latest.lastSuccessful/Mercurial.zip ../Downloads/Mercurial.zip
copy_auto http://build.palaso.org/guestAuth/repository/download/bt437/latest.lastSuccessful/MercurialExtensions/.guidsForInstaller.xml ../MercurialExtensions/.guidsForInstaller.xml
copy_auto http://build.palaso.org/guestAuth/repository/download/bt437/latest.lastSuccessful/MercurialExtensions/Dummy.txt ../MercurialExtensions/Dummy.txt
copy_auto http://build.palaso.org/guestAuth/repository/download/bt437/latest.lastSuccessful/MercurialExtensions/fixutf8/.gitignore ../MercurialExtensions/fixutf8/.gitignore
copy_auto http://build.palaso.org/guestAuth/repository/download/bt437/latest.lastSuccessful/MercurialExtensions/fixutf8/.guidsForInstaller.xml ../MercurialExtensions/fixutf8/.guidsForInstaller.xml
copy_auto http://build.palaso.org/guestAuth/repository/download/bt437/latest.lastSuccessful/MercurialExtensions/fixutf8/.hg_archival.txt ../MercurialExtensions/fixutf8/.hg_archival.txt
copy_auto http://build.palaso.org/guestAuth/repository/download/bt437/latest.lastSuccessful/MercurialExtensions/fixutf8/.hgignore ../MercurialExtensions/fixutf8/.hgignore
copy_auto http://build.palaso.org/guestAuth/repository/download/bt437/latest.lastSuccessful/MercurialExtensions/fixutf8/README. ../MercurialExtensions/fixutf8/README.
copy_auto http://build.palaso.org/guestAuth/repository/download/bt437/latest.lastSuccessful/MercurialExtensions/fixutf8/buildcpmap.py ../MercurialExtensions/fixutf8/buildcpmap.py
copy_auto http://build.palaso.org/guestAuth/repository/download/bt437/latest.lastSuccessful/MercurialExtensions/fixutf8/cpmap.pyc ../MercurialExtensions/fixutf8/cpmap.pyc
copy_auto http://build.palaso.org/guestAuth/repository/download/bt437/latest.lastSuccessful/MercurialExtensions/fixutf8/fixutf8.py ../MercurialExtensions/fixutf8/fixutf8.py
copy_auto http://build.palaso.org/guestAuth/repository/download/bt437/latest.lastSuccessful/MercurialExtensions/fixutf8/fixutf8.pyc ../MercurialExtensions/fixutf8/fixutf8.pyc
copy_auto http://build.palaso.org/guestAuth/repository/download/bt437/latest.lastSuccessful/MercurialExtensions/fixutf8/fixutf8.pyo ../MercurialExtensions/fixutf8/fixutf8.pyo
copy_auto http://build.palaso.org/guestAuth/repository/download/bt437/latest.lastSuccessful/MercurialExtensions/fixutf8/osutil.py ../MercurialExtensions/fixutf8/osutil.py
copy_auto http://build.palaso.org/guestAuth/repository/download/bt437/latest.lastSuccessful/MercurialExtensions/fixutf8/osutil.pyc ../MercurialExtensions/fixutf8/osutil.pyc
copy_auto http://build.palaso.org/guestAuth/repository/download/bt437/latest.lastSuccessful/MercurialExtensions/fixutf8/osutil.pyo ../MercurialExtensions/fixutf8/osutil.pyo
copy_auto http://build.palaso.org/guestAuth/repository/download/bt437/latest.lastSuccessful/MercurialExtensions/fixutf8/win32helper.py ../MercurialExtensions/fixutf8/win32helper.py
copy_auto http://build.palaso.org/guestAuth/repository/download/bt437/latest.lastSuccessful/MercurialExtensions/fixutf8/win32helper.pyc ../MercurialExtensions/fixutf8/win32helper.pyc
copy_auto http://build.palaso.org/guestAuth/repository/download/bt437/latest.lastSuccessful/MercurialExtensions/fixutf8/win32helper.pyo ../MercurialExtensions/fixutf8/win32helper.pyo
copy_auto http://build.palaso.org/guestAuth/repository/download/bt400/latest.lastSuccessful/xulrunner-29.0.1.en-US.win32.zip ../Downloads/xulrunner-29.0.1.en-US.win32.zip
copy_auto http://build.palaso.org/guestAuth/repository/download/bt463/latest.lastSuccessful/Args.dll ../lib/dotnet/Args.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt463/latest.lastSuccessful/GeckofxHtmlToPdf.exe ../lib/dotnet/GeckofxHtmlToPdf.exe
copy_auto http://build.palaso.org/guestAuth/repository/download/bt463/latest.lastSuccessful/GeckofxHtmlToPdf.exe.config ../lib/dotnet/GeckofxHtmlToPdf.exe.config
copy_auto http://build.palaso.org/guestAuth/repository/download/bt54/latest.lastSuccessful/PdfDroplet.exe ../lib/dotnet/PdfDroplet.exe
copy_auto http://build.palaso.org/guestAuth/repository/download/bt54/latest.lastSuccessful/PdfSharp.dll ../lib/dotnet/PdfSharp.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt349/latest.lastSuccessful/TidyManaged.dll ../lib/dotnet/TidyManaged.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt349/latest.lastSuccessful/TidyManaged.dll.config ../lib/dotnet/TidyManaged.dll.config
copy_auto http://build.palaso.org/guestAuth/repository/download/bt349/latest.lastSuccessful/libtidy.dll ../lib/dotnet/libtidy.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt436/latest.lastSuccessful/Palaso.BuildTasks.dll?branch=%3Cdefault%3E ../build/Palaso.BuildTasks.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt436/latest.lastSuccessful/Commons.Xml.Relaxng.dll?branch=%3Cdefault%3E ../lib/dotnet/Commons.Xml.Relaxng.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt436/latest.lastSuccessful/Enchant.Net.dll?branch=%3Cdefault%3E ../lib/dotnet/Enchant.Net.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt436/latest.lastSuccessful/Interop.WIA.dll?branch=%3Cdefault%3E ../lib/dotnet/Interop.WIA.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt436/latest.lastSuccessful/Ionic.Zip.dll?branch=%3Cdefault%3E ../lib/dotnet/Ionic.Zip.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt436/latest.lastSuccessful/Keyman7Interop.dll?branch=%3Cdefault%3E ../lib/dotnet/Keyman7Interop.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt436/latest.lastSuccessful/KeymanLink.dll?branch=%3Cdefault%3E ../lib/dotnet/KeymanLink.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt436/latest.lastSuccessful/L10NSharp.dll?branch=%3Cdefault%3E ../lib/dotnet/L10NSharp.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt436/latest.lastSuccessful/Palaso.BuildTasks.dll?branch=%3Cdefault%3E ../lib/dotnet/Palaso.BuildTasks.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt436/latest.lastSuccessful/Palaso.DictionaryServices.dll?branch=%3Cdefault%3E ../lib/dotnet/Palaso.DictionaryServices.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt436/latest.lastSuccessful/Palaso.Lift.dll?branch=%3Cdefault%3E ../lib/dotnet/Palaso.Lift.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt436/latest.lastSuccessful/Palaso.Media.dll?branch=%3Cdefault%3E ../lib/dotnet/Palaso.Media.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt436/latest.lastSuccessful/Palaso.TestUtilities.dll?branch=%3Cdefault%3E ../lib/dotnet/Palaso.TestUtilities.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt436/latest.lastSuccessful/Palaso.Tests.dll?branch=%3Cdefault%3E ../lib/dotnet/Palaso.Tests.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt436/latest.lastSuccessful/Palaso.dll?branch=%3Cdefault%3E ../lib/dotnet/Palaso.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt436/latest.lastSuccessful/PalasoUIWindowsForms.GeckoBrowserAdapter.dll?branch=%3Cdefault%3E ../lib/dotnet/PalasoUIWindowsForms.GeckoBrowserAdapter.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt436/latest.lastSuccessful/PalasoUIWindowsForms.dll?branch=%3Cdefault%3E ../lib/dotnet/PalasoUIWindowsForms.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt436/latest.lastSuccessful/SIL.Archiving.dll?branch=%3Cdefault%3E ../lib/dotnet/SIL.Archiving.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt436/latest.lastSuccessful/SIL.ScriptureControls.Tests.dll?branch=%3Cdefault%3E ../lib/dotnet/SIL.ScriptureControls.Tests.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt436/latest.lastSuccessful/SIL.ScriptureControls.dll?branch=%3Cdefault%3E ../lib/dotnet/SIL.ScriptureControls.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt436/latest.lastSuccessful/SIL.ScriptureUtils.Tests.dll?branch=%3Cdefault%3E ../lib/dotnet/SIL.ScriptureUtils.Tests.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt436/latest.lastSuccessful/SIL.ScriptureUtils.dll?branch=%3Cdefault%3E ../lib/dotnet/SIL.ScriptureUtils.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt436/latest.lastSuccessful/debug/Commons.Xml.Relaxng.dll?branch=%3Cdefault%3E ../lib/dotnet/Commons.Xml.Relaxng.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt436/latest.lastSuccessful/debug/Interop.WIA.dll?branch=%3Cdefault%3E ../lib/dotnet/Interop.WIA.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt436/latest.lastSuccessful/debug/Ionic.Zip.dll?branch=%3Cdefault%3E ../lib/dotnet/Ionic.Zip.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt436/latest.lastSuccessful/debug/Keyman7Interop.dll?branch=%3Cdefault%3E ../lib/dotnet/Keyman7Interop.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt436/latest.lastSuccessful/debug/KeymanLink.dll?branch=%3Cdefault%3E ../lib/dotnet/KeymanLink.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt436/latest.lastSuccessful/debug/L10NSharp.dll?branch=%3Cdefault%3E ../lib/dotnet/L10NSharp.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt436/latest.lastSuccessful/debug/Palaso.BuildTasks.dll?branch=%3Cdefault%3E ../lib/dotnet/Palaso.BuildTasks.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt436/latest.lastSuccessful/debug/Palaso.DictionaryServices.dll?branch=%3Cdefault%3E ../lib/dotnet/Palaso.DictionaryServices.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt436/latest.lastSuccessful/debug/Palaso.Lift.dll?branch=%3Cdefault%3E ../lib/dotnet/Palaso.Lift.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt436/latest.lastSuccessful/debug/Palaso.Media.dll?branch=%3Cdefault%3E ../lib/dotnet/Palaso.Media.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt436/latest.lastSuccessful/debug/Palaso.TestUtilities.dll?branch=%3Cdefault%3E ../lib/dotnet/Palaso.TestUtilities.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt436/latest.lastSuccessful/debug/Palaso.Tests.dll?branch=%3Cdefault%3E ../lib/dotnet/Palaso.Tests.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt436/latest.lastSuccessful/debug/Palaso.dll?branch=%3Cdefault%3E ../lib/dotnet/Palaso.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt436/latest.lastSuccessful/debug/PalasoUIWindowsForms.GeckoBrowserAdapter.dll?branch=%3Cdefault%3E ../lib/dotnet/PalasoUIWindowsForms.GeckoBrowserAdapter.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt436/latest.lastSuccessful/debug/PalasoUIWindowsForms.dll?branch=%3Cdefault%3E ../lib/dotnet/PalasoUIWindowsForms.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt436/latest.lastSuccessful/debug/SIL.Archiving.dll?branch=%3Cdefault%3E ../lib/dotnet/SIL.Archiving.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt436/latest.lastSuccessful/debug/SIL.ScriptureControls.Tests.dll?branch=%3Cdefault%3E ../lib/dotnet/SIL.ScriptureControls.Tests.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt436/latest.lastSuccessful/debug/SIL.ScriptureControls.dll?branch=%3Cdefault%3E ../lib/dotnet/SIL.ScriptureControls.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt436/latest.lastSuccessful/debug/icu.net.dll?branch=%3Cdefault%3E ../lib/dotnet/icu.net.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt436/latest.lastSuccessful/debug/icudt54.dll?branch=%3Cdefault%3E ../lib/dotnet/icudt54.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt436/latest.lastSuccessful/debug/icuin54.dll?branch=%3Cdefault%3E ../lib/dotnet/icuin54.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt436/latest.lastSuccessful/debug/icuuc54.dll?branch=%3Cdefault%3E ../lib/dotnet/icuuc54.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt436/latest.lastSuccessful/debug/irrKlang.NET4.dll?branch=%3Cdefault%3E ../lib/dotnet/irrKlang.NET4.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt436/latest.lastSuccessful/debug/taglib-sharp.dll?branch=%3Cdefault%3E ../lib/dotnet/taglib-sharp.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt436/latest.lastSuccessful/icu.net.dll?branch=%3Cdefault%3E ../lib/dotnet/icu.net.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt436/latest.lastSuccessful/icudt54.dll?branch=%3Cdefault%3E ../lib/dotnet/icudt54.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt436/latest.lastSuccessful/icuin54.dll?branch=%3Cdefault%3E ../lib/dotnet/icuin54.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt436/latest.lastSuccessful/icuuc54.dll?branch=%3Cdefault%3E ../lib/dotnet/icuuc54.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt436/latest.lastSuccessful/irrKlang.NET4.dll?branch=%3Cdefault%3E ../lib/dotnet/irrKlang.NET4.dll
copy_auto http://build.palaso.org/guestAuth/repository/download/bt436/latest.lastSuccessful/taglib-sharp.dll?branch=%3Cdefault%3E ../lib/dotnet/taglib-sharp.dll
# extract downloaded zip files
unzip -uqo ../Downloads/geckofx.zip -d ../lib/dotnet
unzip -uqo ../Downloads/libsynphony-js.zip -d ../src/BloomBrowserUI/bookEdit/js/libsynphony
unzip -uqo ../Downloads/libsynphony-test-js.zip -d ../src/BloomBrowserUI/bookEdit/test/libsynphony
unzip -uqo ../Downloads/pdfjs-viewer.zip -d ../DistFiles/pdf
unzip -uqo ../Downloads/Mercurial.zip -d ../.
unzip -uqo ../Downloads/xulrunner-29.0.1.en-US.win32.zip -d ../lib
# End of script
