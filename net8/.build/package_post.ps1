# Extension file for the package step in the CDPx build. This will be called after the standard Nuget supplied logic

$Ev2Path = join-path $baseDirectory "build_output\ev2"

Compress-Archive -LiteralPath $Ev2Path\INT\GenevaLogConfig\main.xml, $Ev2Path\INT\GenevaLogConfig\imports -DestinationPath $Ev2Path\INT\GenevaLogConfig\ConfigPackage -Force
Compress-Archive -LiteralPath $Ev2Path\PPE\GenevaLogConfig\main.xml, $Ev2Path\PPE\GenevaLogConfig\imports -DestinationPath $Ev2Path\PPE\GenevaLogConfig\ConfigPackage -Force
Compress-Archive -LiteralPath $Ev2Path\PROD\GenevaLogConfig\main.xml, $Ev2Path\PROD\GenevaLogConfig\imports -DestinationPath $Ev2Path\PROD\GenevaLogConfig\ConfigPackage -Force