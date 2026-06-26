// Parse-throughput harness for the SWF corpus using Ruffle's `swf` crate (crates.io).
// Mirrors the C# and PHP harnesses: load every .swf under a directory, decompress and
// fully parse all tags for N rounds, and report the best wall-clock time.
//
//   cargo run --release -- <corpus-dir> [rounds]

use std::path::Path;
use std::time::Instant;

fn collect(dir: &Path, out: &mut Vec<Vec<u8>>) {
    for entry in std::fs::read_dir(dir).unwrap() {
        let path = entry.unwrap().path();
        if path.is_dir() {
            collect(&path, out);
        } else if path.extension().map_or(false, |e| e == "swf") {
            out.push(std::fs::read(&path).unwrap());
        }
    }
}

fn main() {
    let args: Vec<String> = std::env::args().collect();
    let dir = &args[1];
    let rounds: usize = args.get(2).and_then(|s| s.parse().ok()).unwrap_or(5);

    let mut files = Vec::new();
    collect(Path::new(dir), &mut files);
    let total_bytes: usize = files.iter().map(|f| f.len()).sum();

    let mut best = f64::MAX;
    let mut tag_count = 0usize;

    for _ in 0..rounds {
        let start = Instant::now();
        let mut tags = 0usize;

        for data in &files {
            if let Ok(buf) = swf::decompress_swf(&data[..]) {
                if let Ok(swf) = swf::parse_swf(&buf) {
                    tags += swf.tags.len();
                }
            }
        }

        let ms = start.elapsed().as_secs_f64() * 1000.0;
        if ms < best {
            best = ms;
        }
        tag_count = tags;
    }

    println!(
        "impl=ruffle files={} bytes={} parse_ms={:.2} tags={}",
        files.len(),
        total_bytes,
        best,
        tag_count
    );
}
