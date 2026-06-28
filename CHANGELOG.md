# CHANGELOG

## 2026.06.28

- Added support for `index.jpg` and `index.png` as recognised cover image filenames.
- Extracted hardcoded filenames (`folder.jpg`, `cover.jpg`, `front.jpg`, `desktop.ini`, etc.) to named constants for maintainability.
- Fixed platform detection: replaced deprecated `PlatformID.Unix` check and `ie4uinit.exe` call with `RuntimeInformation.IsOSPlatform(OSPlatform.Windows)`, fixing incorrect behaviour on macOS.
- Removed unused SkiaSharp dependency, reducing binary size.
- Translated `IDEAS.md` and `TODO.md` to English.

## 2025.05.29

- Improved algorithm for folderjpg-XXXX.ico generation so same folder names will always generate same icon filename.

## 2024.10.10

- Improved error handling for invalid image paths.
- Optimized performance for large directories.
- Fixed a bug where non-image files caused unexpected crashes.
- Updated documentation for better clarity.
- Refactored code for maintainability and readability.

## 2024.10.05

- Multilang help support using --lang xx (English by default).
- Added support for cover.jpg if no folder.jpg image is found.
- Added support for front.jpg if no folder.jpg image is found.
- No longer works if no correct path is provided as an argument (to avoid accidental executions).

