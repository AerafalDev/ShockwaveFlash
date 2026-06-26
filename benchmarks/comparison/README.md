# Cross-implementation benchmarks

Parse-throughput comparison of three SWF parsers over the same corpus:

| Implementation | Language | Harness |
|----------------|----------|---------|
| **ShockwaveFlash** (this repo) | C# / .NET 10 | `src/ShockwaveFlash.Benchmarks` (`-- throughput`) |
| **Ruffle** `swf` crate | Rust | `rust/` (pulls `swf` from crates.io) |
| **ArakneSwf** | PHP | `arakne.php` (reads the local `context/ArakneSwf-master`) |

Each harness loads every `.swf` under the corpus directory, fully decompresses and
parses **all tags** for N rounds, and reports the best wall-clock time. Methodology
and measured results are in [`../RESULTS.md`](../RESULTS.md).

## Run

```powershell
# all three (needs dotnet, cargo, php on PATH; Arakne under context/)
pwsh benchmarks/comparison/run.ps1 -Rounds 5

# individually
dotnet run -c Release --project src/ShockwaveFlash.Benchmarks -- throughput data 5
cargo run --release --manifest-path benchmarks/comparison/rust/Cargo.toml -- data 5
php benchmarks/comparison/arakne.php context/ArakneSwf-master data 3
```

## Notes

- The Ankama SWF corpus lives in `data/` (committed). The Ruffle and Arakne source
  checkouts are git submodules under `context/` — run `git submodule update --init`
  before the PHP/oracle steps (the Rust harness is self-contained, fetching Ruffle's
  published `swf` crate from crates.io).
- `Errors::ALL` is used for Arakne so all three parse strictly (stop on malformed
  data), matching our reader.
- Numbers are wall-clock, best-of-N, files pre-listed; disk I/O is warmed by the
  first round. They measure parsing only, not rendering.
