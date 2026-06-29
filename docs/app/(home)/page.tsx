import Link from 'next/link';

export default function HomePage() {
  return (
    <main className="flex flex-1 flex-col items-center justify-center text-center px-4">
      <h1 className="text-3xl font-bold sm:text-4xl">ShockwaveFlash</h1>
      <p className="mt-3 max-w-xl text-fd-muted-foreground">
        Read, edit, write and render SWF (Shockwave Flash) files — their AVM1 ActionScript, and render
        them to images or SVG — entirely in code, on .NET 10.
      </p>
      <div className="mt-6 flex gap-3">
        <Link
          href="/docs"
          className="rounded-lg bg-fd-primary px-4 py-2 text-sm font-medium text-fd-primary-foreground"
        >
          Read the docs
        </Link>
        <a
          href="https://github.com/AerafalDev/ShockwaveFlash"
          className="rounded-lg border px-4 py-2 text-sm font-medium"
        >
          GitHub
        </a>
      </div>
    </main>
  );
}
