Param (
 [Parameter(Mandatory=$true)][string]$officialBinaryPath, #"D:\code\pm2\Drop\Debug"
 [Parameter(Mandatory=$true)][string]$testBinaryPath #"D:\code\prototypes\corext_msbuild\test\drop\Debug"
)


$diff = "diff_temp.txt"
$diffFinal = "diff.txt"
$officialBinaryPath = [System.IO.Path]::GetFullPath($officialBinaryPath)
$testBinaryPath = [System.IO.Path]::GetFullPath($testBinaryPath)

if (Test-path $diff) { del $diff }

"Test -> Official"
$a = $testBinaryPath
$b = $officialBinaryPath
$tag = "new"

$aPrefix = $a.Length
$bPrefix = $b.Length

gci $a -Recurse | % {
    $len = $_.FullName.Length
    $f = $_.FullName.Substring($aPrefix, $len - $aPrefix)
    $t = $b + $f
    if (!(Test-Path $t)) { "$f ($tag)" }
} | Out-File $diff -Encoding utf8 -Append

"Official -> Test"
$a = $officialBinaryPath
$b = $testBinaryPath
$tag = "old"

$aPrefix = $a.Length
$bPrefix = $b.Length

gci $a -Recurse | % {
    #test does not copy the extra .config and .xml files filter them for a bit
    if ($_.Name.EndsWith(".config")) { return; }
    if ($_.Name.EndsWith(".xml")) { return; }
    $len = $_.FullName.Length
    $f = $_.FullName.Substring($aPrefix, $len - $aPrefix)
    $t = $b + $f
    if (!(Test-Path $t)) { "$f ($tag)" }
} | Out-File $diff -Encoding utf8 -Append

"Sort"
cat $diff | sort | Out-File $diffFinal -Encoding utf8

"Result in diff.txt"