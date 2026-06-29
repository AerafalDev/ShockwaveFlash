import { createMDX } from 'fumadocs-mdx/next';

const withMDX = createMDX();

// GitHub Pages project site lives under /<repo>. Set DOCS_BASE_PATH='' for a
// user/org page or a custom domain.
const basePath = process.env.DOCS_BASE_PATH ?? '/ShockwaveFlash';

/** @type {import('next').NextConfig} */
const config = {
  output: 'export',
  basePath,
  images: { unoptimized: true },
  reactStrictMode: true,
};

export default withMDX(config);
