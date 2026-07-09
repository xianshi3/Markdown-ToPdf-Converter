<div align="center">

# Markdown to PDF Converter

> A sleek, cross-platform desktop application for converting Markdown to PDF with live preview, multi-language support, and customizable themes.

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![Avalonia](https://img.shields.io/badge/Avalonia-11.2-8B5CF6?style=for-the-badge&logo=avalonia&logoColor=white)
![Platform](https://img.shields.io/badge/platform-Windows%20|%20Linux%20|%20macOS-4FC08D?style=for-the-badge)
![License](https://img.shields.io/badge/license-MIT-EB5424?style=for-the-badge)

</div>

---

## Overview

**Markdown to PDF Converter** is a modern, feature-rich desktop application that transforms your Markdown documents into beautifully formatted PDFs. Built with the cross-platform **Avalonia UI** framework, it delivers a native experience on Windows, Linux, and macOS from a single codebase.

Beyond conversion, it serves as a full-featured Markdown editor with live preview, syntax formatting tools, multi-language interface, and a refined theme engine — making it the perfect companion for writers, developers, and documentation authors.

---

## Screenshots

<div align="center">
  <table>
    <tr>
      <td align="center"><strong>Dark Theme</strong></td>
      <td align="center"><strong>Light Theme</strong></td>
    </tr>
    <tr>
      <td><img src="https://via.placeholder.com/320x200/0D1117/E6EDF3?text=Dark+Theme" width="320"/></td>
      <td><img src="https://via.placeholder.com/320x200/FFFFFF/24292F?text=Light+Theme" width="320"/></td>
    </tr>
  </table>
</div>

---

## Features

### Editor
| Feature | Description |
|---|---|
| **Live Preview** | Real-time Markdown rendering powered by Markdig AST parsing — switch between edit and preview tabs |
| **Formatting Toolbar** | Bold, Italic, Heading, Link, Image, Code, Bullet List, Block Quote insertion with a single click |
| **Find & Replace** | Case-sensitive search, match count, and Next/Prev navigation |
| **Syntax Support** | Headings H1–H6, paragraphs, code blocks, blockquotes, ordered/unordered lists, tables, horizontal rules, and inline formatting |
| **Statistics** | Live line count, word count, and character count in the sidebar and status bar |

### Export
| Feature | Description |
|---|---|
| **PDF Export** | Generate professional PDF documents with customizable page size (A4, Letter, Legal, A3, A5), margin, and font |
| **HTML Export** | Export fully styled HTML documents with embedded CSS |
| **Chinese Font** | Bundled SimSun font ensures proper CJK character rendering in exported PDFs |

### User Experience
| Feature | Description |
|---|---|
| **Multi-Language** | Seamless switching between English and Chinese — all UI text updates instantly |
| **Three Themes** | Dark, Light, and Gray themes with smooth transition animations |
| **Auto-Save** | Automatic backup every 60 seconds so your work is never lost |
| **Recent Files** | Persistent recent file list (up to 10) stored in `%LOCALAPPDATA%` |
| **Zoom Controls** | Editor font zoom from 50% to 200% with dedicated toolbar buttons and keyboard shortcuts |
| **Drag & Drop** | Drag `.md` files directly onto the upload zone to open instantly |
| **Undo / Redo** | Full undo/redo stack for worry-free editing |

### Keyboard Shortcuts
| Shortcut | Action |
|---|---|
| `Ctrl+N` | New File |
| `Ctrl+O` | Open File |
| `Ctrl+S` | Save File |
| `Ctrl+Z` | Undo |
| `Ctrl+Y` | Redo |
| `Ctrl+F` | Find |
| `Ctrl+B` | Bold |
| `Ctrl+I` | Italic |
| `Ctrl+P` | Preview Toggle |
| `Ctrl++` | Zoom In |
| `Ctrl+-` | Zoom Out |
| `Ctrl+0` | Reset Zoom |

---

## Technology Stack

| Layer | Technology |
|---|---|
| **UI Framework** | [Avalonia UI](https://www.avaloniaui.net/) 11.2.3 — Cross-platform desktop UI |
| **Architecture** | MVVM with [ReactiveUI](https://www.reactiveui.net/) — Command binding and reactive property change notification |
| **Markdown Parsing** | [Markdig](https://github.com/xoofx/markdig) 0.39.1 — Fast, extensible Markdown processor with advanced extensions |
| **PDF Generation** | [PdfSharpCore](https://github.com/ststeiger/PdfSharpCore) + [MigraDocCore](https://github.com/ststeiger/MigraDocCore) 1.3.65 |
| **HTML Parsing** | [HtmlAgilityPack](https://html-agility-pack.net/) 1.12.1 |
| **Runtime** | .NET 8.0 |
| **Platforms** | Windows (x64), Linux (x64), macOS (x64) |

---

## Project Structure

```
MarkdownToPdfConverter/
├── MarkdownToPdfConverter/          # Core class library
│   ├── App.axaml                    # Application entry (styles, Fluent theme)
│   ├── Converters/                  # Value converters (BoolToFontStyle, etc.)
│   ├── Models/                      # Data models (PreviewBlock, PreviewBlockType)
│   ├── Resources/
│   │   ├── Fonts/                   # Bundled fonts (SimSun.ttf)
│   │   └── Strings/                 # Localization JSON files (en-US, zh-CN)
│   ├── Services/                    # Business logic
│   │   ├── ThemeService.cs          # Theme management (Dark/Light/Gray)
│   │   ├── LocalizationService.cs   # Multi-language support
│   │   ├── MarkdownPreviewService.cs# Markdig → PreviewBlock parsing
│   │   └── MarkdownToPdfService.cs  # PDF generation
│   ├── ViewModels/                  # MVVM ViewModels
│   │   └── MainViewModel.cs         # Central ViewModel (~830 lines)
│   └── Views/Components/            # Avalonia UI components
│       ├── EditorComponent.axaml    # Editor tabs + format toolbar + preview
│       ├── ToolBarComponent.axaml   # Main toolbar (file ops, export, zoom)
│       ├── SidebarComponent.axaml   # Settings, statistics, recent files
│       ├── FindReplaceComponent.axaml # Search & replace panel
│       └── MenuBarComponent.axaml   # Application menu
├── MarkdownToPdfConverter.Desktop/  # Desktop executable host
│   └── Program.cs                   # Entry point (Avalonia app builder)
├── MarkdownToPdfConverter.sln       # Solution file
└── Directory.Build.props            # Shared MSBuild properties
```

---

## Getting Started

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- An IDE: [Visual Studio 2022](https://visualstudio.microsoft.com/), [JetBrains Rider](https://www.jetbrains.com/rider/), or [VS Code](https://code.visualstudio.com/) with C# extension

### Build & Run

```bash
# Clone the repository
git clone https://github.com/your-username/MarkdownToPdfConverter.git
cd MarkdownToPdfConverter

# Restore dependencies
dotnet restore

# Run the application
dotnet run --project MarkdownToPdfConverter.Desktop

# Build a release
dotnet publish MarkdownToPdfConverter.Desktop -c Release -r win-x64 --self-contained
```

> Replace `win-x64` with `linux-x64` or `osx-x64` for other platforms.

---

## Usage

1. **Open a file** — Click the folder icon or drag a `.md` file onto the upload zone
2. **Edit** — Switch to the **Edit Markdown** tab and use the formatting toolbar or keyboard shortcuts
3. **Preview** — Switch to the **Preview** tab to see the rendered output in real time
4. **Export** — Click **PDF** or **HTML** in the toolbar, choose your settings, and save
5. **Customize** — Toggle themes (Dark/Light/Gray) via the sidebar or switch language (EN/中文)

---

## Contributing

Contributions are welcome! Feel free to open issues for bug reports or feature requests, and submit pull requests for improvements.

---



<div align="center">

Made with ❤️ using Avalonia UI and .NET

</div>
