<?php

use Arakne\Swf\SwfFile;
use Arakne\Swf\Error\Errors;

$arakne = $argv[1];
$dir = $argv[2];
$rounds = (int)($argv[3] ?? 3);

spl_autoload_register(static function (string $c) use ($arakne) {
    if (!str_starts_with($c, 'Arakne\\Swf\\')) {
        return;
    }
    $f = $arakne . '/src/' . str_replace('\\', '/', substr($c, 11)) . '.php';
    if (file_exists($f)) {
        require_once $f;
    }
});

$paths = [];
$rii = new RecursiveIteratorIterator(new RecursiveDirectoryIterator($dir, FilesystemIterator::SKIP_DOTS));
foreach ($rii as $f) {
    if ($f->isFile() && strtolower($f->getExtension()) === 'swf') {
        $paths[] = $f->getPathname();
    }
}

$bytes = 0;
foreach ($paths as $p) {
    $bytes += filesize($p);
}

$best = PHP_FLOAT_MAX;
$tags = 0;
for ($r = 0; $r < $rounds; $r++) {
    $start = microtime(true);
    $t = 0;
    foreach ($paths as $p) {
        try {
            $swf = new SwfFile($p, Errors::ALL);
            foreach ($swf->tags() as $tag) {
                $t++;
            }
        } catch (\Throwable $e) {
        }
    }
    $ms = (microtime(true) - $start) * 1000;
    if ($ms < $best) {
        $best = $ms;
    }
    $tags = $t;
}

printf("impl=arakne files=%d bytes=%d parse_ms=%.2f tags=%d\n", count($paths), $bytes, $best, $tags);
