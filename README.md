# MarkdownToPdfConverter Project Introduction

## Project Overview
MarkdownToPdfConverter is a cross-platform desktop application developed based on the Avalonia UI framework, capable of converting Markdown documents into PDF files. This project features an intuitive user interface and a rich set of functional characteristics.

![QQ20250617-162158](https://github.com/user-attachments/assets/019e35b0-fc6c-4e2e-b4fd-0e69011f90ee)

## Main Features

### 1. File Conversion Function
- Supports uploading Markdown files or directly editing content
- Converts Markdown content into PDF format
- Supports saving PDF files locally

### 2. Multi-Language Support
- Switch between Chinese and English interfaces
- Automatically updates all interface element texts

### 3. Theme Customization
- Provides three theme modes:
  - Gray (default gray theme)
  - Dark (dark theme)
  - Light (light theme)
- Dynamically switches all interface element styles

### 4. Edit and Preview
- Built-in Markdown editor
- Real-time preview rendering effects
- Adjustable font size (12-24px)

### 5. Advanced Features
- Supports common Markdown syntax:
  - Headings (H1-H6)
  - Paragraphs and line breaks
  - Bold, Italic
  - Lists (ordered and unordered)
  - Blockquotes
- Uses SimSun font to ensure Chinese compatibility

## Technical Stack

### Frontend Framework
- Avalonia UI (cross-platform .NET UI framework)

### Core Components
- ReactiveUI (reactive programming framework)
- MigraDocCore (PDF generation library)
- PdfSharpCore (PDF processing library)
- CommonMark.NET (Markdown parser)
- HtmlAgilityPack (HTML parser)

### Specialized Implementations
- Custom font parser (supports Chinese)
- Recursive conversion algorithm from HTML to PDF
- Reactive UI binding and data-driven updates

## Usage Instructions

1. **Upload File**:
   - Click the "Upload Markdown" button to select a .md file
   - The file content will be automatically loaded into the editing area

2. **Edit Content**:
   - Directly modify the content in the "Edit Markdown" tab
   - Real-time preview rendering effects on the right side

3. **Convert to PDF**:
   - Ensure there is valid content and then click "Convert PDF"
   - Choose the save location and file name

## Dependencies

- .NET 6+
- Avalonia 11+
