param ($csproj, $xpath, $innertext);
[xml]$xmlDoc = Get-Content $csproj;
$node = $xmlDoc.SelectSingleNode($xpath);
$node.InnerText = $innertext;
$xmlDoc.Save($csproj);
