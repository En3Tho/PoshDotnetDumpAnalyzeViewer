$solutionPath = Get-Location

$version = "0.0.8"
$workingDir = "$solutionPath\Releases\$version\PoshDotnetDumpAnalyzeViewerModule"
$argList =
    "-noprofile",
    "-noexit",
    "-command & { $solutionPath\TestScript.ps1 }"

start pwsh -WorkingDirectory $workingDir -ArgumentList $argList