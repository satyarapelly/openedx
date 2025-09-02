
$serviceIdentifier = "e50abb8e-e976-4311-b12b-85156f4abc0e"
$serviceGroup = "Microsoft.CFS.PC.PX.INT"
$config = @{
    int = @{
        ev2Infra = "test"
        subid = "230ef3cc-8fdd-4f26-bf9c-10131b4080e5"
        subkey = "STORECORE-PST-PX-INT"
        regions = "westus,westus2"
    }
}

$config.GetEnumerator() | ForEach-Object {
    $env = $_.Name
    $ev2Infra = $_.Value.ev2Infra
    $subid = $_.Value.subid
    $subkey = $_.Value.subkey
    $regions = $_.Value.regions

    Write-Host "env: $env"
    Write-Host "ev2Infra: $ev2Infra"
    Write-Host "subid: $subid"
    Write-Host "subkey: $subkey"
    Write-Host "regions: $regions"

    Write-Host "Setting up $env..."
    Write-Host "...servicegroup"
    ev2 servicegroup new `
        --serviceidentifier $serviceIdentifier `
        --filepath ServiceGroupSpecINT.json `
        --rolloutinfra $ev2Infra

    Write-Host "...subscription"
    ev2 subscription register `
        --serviceidentifier $serviceIdentifier `
        --servicegroup $serviceGroup `
        --id $subid `
        --key $subkey `
        --rolloutinfra $ev2Infra

    Write-Host "...presence"
    ev2 service presence register `
        --serviceidentifier $serviceIdentifier `
        --servicegroup $serviceGroup `
        --rolloutinfra $ev2Infra `
        --locations $regions
}

Write-Host "Done"