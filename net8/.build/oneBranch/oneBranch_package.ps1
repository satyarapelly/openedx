echo 'mkdir'

mkdir '.\Package\Ev2\bin'
mkdir '.\Package\EV2-RA\bin'

echo '7z'

7z a -r '.\Package\Ev2\bin\pxservice.zip' '.\Debug\PXService\*'
7z a -r '.\Package\Ev2\bin\pxdependencyemulators.zip' '.\Debug\PXDependencyEmulators\*'

echo '7z'
7z a -r '.\Package\Ev2-RA\bin\pxservice.zip' '.\Debug\PXService\*'
7z a -r '.\Package\Ev2-RA\bin\pxdependencyemulators.zip' '.\Debug\PXDependencyEmulators\*'

# TODO - need to support genevaLogConfig deployment in bicep
Compress-Archive -LiteralPath .\Package\Ev2\INT\GenevaLogConfig\main.xml, .\Package\Ev2\INT\GenevaLogConfig\imports -DestinationPath .\Package\Ev2\INT\GenevaLogConfig\ConfigPackage -Force
Compress-Archive -LiteralPath .\Package\Ev2\PPE\GenevaLogConfig\main.xml, .\Package\Ev2\PPE\GenevaLogConfig\imports -DestinationPath .\Package\Ev2\PPE\GenevaLogConfig\ConfigPackage -Force
Compress-Archive -LiteralPath .\Package\Ev2\PROD\GenevaLogConfig\main.xml, .\Package\Ev2\PROD\GenevaLogConfig\imports -DestinationPath .\Package\Ev2\PROD\GenevaLogConfig\ConfigPackage -Force