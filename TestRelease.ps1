$solutionPath = Get-Location

$version = "0.0.7"
# specifically set this to the "wrong" folder
# to test that relative path will be resolved correctly after cd
$workingDir = "$solutionPath\Releases\$version"

$argList = "-noprofile", "-noexit", "-command & {
    cd PoshDotnetDumpAnalyzeViewerModule
    Import-Module .\PoshDotnetDumpAnalyzeViewerModule.psd1
    Get-DotnetDumpAnalyzeViewer '..\..\test dump.dmp'
}"

start pwsh -WorkingDirectory $workingDir -ArgumentList $argList