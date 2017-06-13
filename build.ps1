param($filename)

$outputFilename = $filename.substring(0, $filename.indexOf(".")) + ".svg"

dot -Tsvg $filename -o $outputFilename

# start $outputFilename

Write-Host "Done! wrote" $outputFilename