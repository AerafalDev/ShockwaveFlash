# Benchmark results

Absolute numbers are hardware-dependent (measured on the development machine, .NET 10,
Release, ServerGC); the **ratios** are the takeaway. Reproduce with
[`comparison/run.ps1`](comparison/README.md) and the `ShockwaveFlash.Benchmarks` project.

Corpus: the committed Ankama set under `data/` — **348 SWF files, 18.9 MiB compressed,
~56,900 top-level tags**.

## 1. Cross-implementation parse throughput

Decompress + fully parse every tag of all 348 files, best of N rounds (in-memory).

| Parser | Language | Time | Throughput | vs ours |
|--------|----------|-----:|-----------:|--------:|
| **ShockwaveFlash** | C# / .NET 10 | **132.7 ms** | **142 MiB/s** | 1.00× |
| Ruffle `swf` 0.2 | Rust | 167.4 ms | 113 MiB/s | 1.26× slower |
| ArakneSwf | PHP 8.4 | 94,539 ms | 0.20 MiB/s | ~712× slower |

ShockwaveFlash parses the corpus **~26 % faster than Ruffle's Rust `swf` crate** and
roughly **700× faster than ArakneSwf** (interpreted PHP). All three parse strictly
(stop on malformed data). Tag counts differ slightly by convention (ours 56,969,
Ruffle 56,621, Arakne 51,027).

## 2. Round-trip (parse + re-assemble)

ShockwaveFlash and Ruffle have writers; Arakne does not.

| Operation | ShockwaveFlash |
|-----------|---------------:|
| Parse whole corpus | 132.7 ms |
| Parse + Assemble whole corpus | 776.2 ms |

The writer is the slower half (canonicalising bit-widths); see the per-size breakdown
below. Faithful round-trip is validated for correctness by the test suite
(re-parsing the assembled output yields a model deeply equal to the original, 348/348).

## 3. Internal micro-benchmarks (BenchmarkDotNet)

Per-file by size class — `small` = an 8 KB mount, `medium` = retro `core.swf`,
`large` = `DofusInvoker.swf` (21 MB decompressed). ShortRun job, `[MemoryDiagnoser]`.

| Operation | small | medium | large |
|-----------|------:|-------:|------:|
| **Parse** | 75 µs | 12.8 ms | 31.9 ms |
| Parse — allocated | 161 KB | 17.9 MB | 25.1 MB |
| **Assemble** | 145 µs | 44.2 ms | 252.8 ms |
| Assemble — allocated | 63 KB | 11.7 MB | 60.3 MB |
| **Parse + Assemble** | 223 µs | 58.4 ms | 281.9 ms |

Observations:

- Parsing is allocation-light relative to the decompressed size (the model holds raw
  payloads as `ReadOnlyMemory` slices rather than copies).
- The writer (`Assemble`) is ~8× slower than the reader on the large file and is the
  main optimisation target — it currently grows buffers and copies per tag.
