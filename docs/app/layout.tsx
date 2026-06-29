import type { Metadata } from 'next';
import { Inter } from 'next/font/google';
import { Provider } from '@/components/provider';
import './global.css';

const inter = Inter({
  subsets: ['latin'],
});

export const metadata: Metadata = {
  metadataBase: new URL('https://aerafaldev.github.io/ShockwaveFlash'),
  title: {
    default: 'ShockwaveFlash',
    template: '%s · ShockwaveFlash',
  },
  description: 'Read, edit, write and render SWF (Shockwave Flash) files in pure .NET 10.',
};

export default function Layout({ children }: LayoutProps<'/'>) {
  return (
    <html lang="en" className={inter.className} suppressHydrationWarning>
      <body className="flex flex-col min-h-screen">
        <Provider>{children}</Provider>
      </body>
    </html>
  );
}
