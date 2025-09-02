param (
    $src
)
[xml]$xmldata = get-content $src
$xmldata.SelectNodes("//processing-instruction()") | %{$_.ParentNode.RemoveChild($_)}
$xmldata.Save($src)
