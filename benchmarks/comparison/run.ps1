#requires -Version 7
# Cross-implementation SWF parse-throughput comparison: ShockwaveFlash (C#) vs
# Ruffle (Rust) vs ArakneSwf (PHP), over the same corpus.
#
# Ruffle's `swf` crate is pulled from crates.io (needs cargo). Arakne is read from
# the local context/ checkout (gitignored); install Ruffle/Arakne sources there or
# point -Context elsewhere. PHP must be on PATH.

param(
    [string]$Corpus = "$PSScriptRoot/../../data",
    [string]$Context = "$PSScriptRoot/../../context",
    [int]$Rounds = 5
)

$ErrorActionPreference = 'Stop'

Write-Host "Corpus: $Corpus" -ForegroundColor Cyan

Write-Host "`n[1/3] ShockwaveFlash (C#)..." -ForegroundColor Yellow
$ours = dotnet run -c Release --project "$PSScriptRoot/../../src/ShockwaveFlash.Benchmarks" -- throughput $Corpus $Rounds |
    Select-String '^impl=ours'

Write-Host "[2/3] Ruffle (Rust, crates.io swf)..." -ForegroundColor Yellow
Push-Location "$PSScriptRoot/rust"
try
{
    cargo build --release --quiet
    $ruffle = & "./target/release/ruffle-parse-bench" $Corpus $Rounds
}
finally
{
    Pop-Location
}

Write-Host "[3/3] ArakneSwf (PHP)..." -ForegroundColor Yellow
$arakne = php "$PSScriptRoot/arakne.php" "$Context/ArakneSwf-master" $Corpus ([Math]::Min($Rounds, 3)) |
    Select-String '^impl=arakne'

Write-Host "`nResults:" -ForegroundColor Green
$ours
$ruffle
$arakne
