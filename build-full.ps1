param($dir, $dotFile)

. "D:\git-repos\functions-dot-visualizer\functions-dot-visualizer\bin\Debug\FunctionsDotVisualizer.exe" $dir $dotFile

$outputFilename = $dotFile.substring(0, $dotFile.indexOf(".")) + ".svg"

dot -Tsvg $dotFile -o $outputFilename

# start $outputFilename

Write-Host "Done! wrote" $outputFilename