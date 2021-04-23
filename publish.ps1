#1.0.69
$packages = @("maxbl4.RfidDotNet.AlienTech.Simulator","maxbl4.RfidDotNet","maxbl4.RfidDotNet.AlienTech","maxbl4.RfidDotNet.GenericSerial")

function Main()
{
    $version = GetNextVersion

    dotnet test --filter Hardware!=true
    if (-not $?) { exit $? }

    foreach ($p in $packages){
        Pack $p $version
    }

    UpdateVersion $version
}

function Pack($name, $version)
{    
    dotnet pack -c Release /p:Version=$version .\$name\$name.csproj
    if (-not $?) { exit; }    
    nuget push -Source NugetLocal .\$name\bin\Release\$name.$version.nupkg
    if (-not $?) { exit; }
    nuget push .\$name\bin\Release\$name.$version.nupkg
    if (-not $?) { exit; }
}

function GetNextVersion()
{
    $lines = Get-Content $MyInvocation.ScriptName
    $version = [System.Version]::Parse($lines[0].Substring(1))
    return "$($version.Major).$($version.Minor).$($version.Build + 1)"
}

function UpdateVersion($version)
{
    $lines = Get-Content $MyInvocation.ScriptName
    $lines[0] = "#$version"
    $lines > $MyInvocation.ScriptName
}

Main
