$solutionPath = Get-Location

pushd $solutionPath

$version = "0.0.7"
$modulePath = "$solutionPath\PoshDotnetDumpAnalyzeViewerModule"
$configuration = "Release"
$output = "$solutionPath\Releases\publish"

dotnet publish $modulePath -c $configuration -o $output
cd "$solutionPath\Releases"

Remove-Item $version -Force -Recurse -ErrorAction SilentlyContinue

mkdir $version
cd $version
mkdir "PoshDotnetDumpAnalyzeViewerModule"
cd "PoshDotnetDumpAnalyzeViewerModule"

$dllNames =
    "NStack.dll",
    "PoshDotnetDumpAnalyzeViewer.dll",
    "PoshDotnetDumpAnalyzeViewerModule.dll",
    "Terminal.Gui.dll"

foreach ($file in $dllNames) {
    Copy-Item "$output\$file" -Destination ".\"
}

Copy-Item "$modulePath\PoshDotnetDumpAnalyzeViewerModule.psd1" -Destination ".\"

popd