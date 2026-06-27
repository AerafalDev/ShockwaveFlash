# Security Policy

## Supported versions

ShockwaveFlash is published as a pre-1.0 library and is developed on the `main`
branch. Security fixes are applied to `main` and shipped in the next release; only
the **latest published version** is supported.

| Version | Supported |
| --- | --- |
| Latest release | ✅ |
| Older releases | ❌ |

## Reporting a vulnerability

**Please do not report security issues in public GitHub issues or pull requests.**

This library parses untrusted binary input, so the security surface that matters
most is **malformed or hostile SWF files** — for example out-of-bounds reads,
unbounded allocations or decompression bombs, or infinite loops triggered while
parsing. If you find an input that crashes, hangs or over-allocates, that's a
report we want.

Report privately through either channel:

- **GitHub Security Advisories** — open the repository's **Security → Report a
  vulnerability** tab to start a private advisory (preferred).
- **Email** — <aerafal.github@gmail.com>.

Please include:

- a description of the issue and its impact,
- a minimal sample file or byte sequence that reproduces it, if you can share one,
- the version (or commit) you observed it on.

## What to expect

- We aim to acknowledge a report within a few days.
- We'll confirm the issue, keep you updated as we work on a fix, and credit you in
  the release notes unless you prefer to stay anonymous.
- Once a fix is released, the advisory is published.

Thank you for helping keep ShockwaveFlash and its users safe.
