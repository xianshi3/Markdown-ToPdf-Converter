# MarkdownToPdfConverter Project Introduction



## Project Overview

**MarkdownToPdfConverter** is a cross-platform desktop application built with the Avalonia UI framework, designed to convert Markdown documents into PDF files effortlessly. The project features a clean and intuitive user interface alongside a comprehensive set of functionalities.

![App Preview](https://github.com/user-attachments/assets/8fedc5a9-7257-4260-8928-142376278a58)



## Key Features

### 1. File Conversion

> 1. Upload Markdown files or directly edit content  
> 2. Convert Markdown content into PDF format  
> 3. Save generated PDF files locally

### 2. Multi-Language Support

> 1. Switch seamlessly between Chinese and English interfaces  
> 2. Automatic update of all UI text elements

### 3. Theme Customization

> 1. Offers three theme modes:  
> &nbsp;&nbsp;&nbsp;&nbsp;• Gray (default)  
> &nbsp;&nbsp;&nbsp;&nbsp;• Dark  
> &nbsp;&nbsp;&nbsp;&nbsp;• Light  
> 2. Dynamically applies theme styles across the interface

### 4. Edit and Preview

> 1. Built-in Markdown editor  
> 2. Real-time preview rendering on the side  
> 3. Adjustable font size (12–24px)

### 5. Advanced Markdown Support

> 1. Supports common Markdown syntax:  
> &nbsp;&nbsp;&nbsp;&nbsp;• Headings (H1–H6)  
> &nbsp;&nbsp;&nbsp;&nbsp;• Paragraphs and line breaks  
> &nbsp;&nbsp;&nbsp;&nbsp;• Bold and italic text  
> &nbsp;&nbsp;&nbsp;&nbsp;• Ordered and unordered lists  
> &nbsp;&nbsp;&nbsp;&nbsp;• Blockquotes  
> 2. Uses SimSun font to ensure proper Chinese character rendering


## Technology Stack

| Category           | Description                                      |
| ------------------ | ------------------------------------------------|
| **Frontend**       | Avalonia UI (cross-platform .NET desktop UI)    |
| **Core Components**| ReactiveUI, MigraDocCore, PdfSharpCore, CommonMark.NET, HtmlAgilityPack |
| **Special Features**| Custom Chinese font parser, HTML→PDF recursive conversion, reactive data binding |



## Usage Instructions

> 1. **Upload File**  
> &nbsp;&nbsp;&nbsp;&nbsp;Click the “Upload Markdown” button and select a `.md` file; the content will automatically load into the editor.

> 2. **Edit Content**  
> &nbsp;&nbsp;&nbsp;&nbsp;Modify the Markdown text directly in the “Edit Markdown” tab; real-time preview is rendered on the right side.

> 3. **Convert to PDF**  
> &nbsp;&nbsp;&nbsp;&nbsp;After ensuring the content is valid, click “Convert PDF” and choose the save location and filename.


## Dependencies

| Technology  | Minimum Version    |
| ----------- | ------------------ |
| .NET        | 6+                 |
| Avalonia   | 11+                 |

---
