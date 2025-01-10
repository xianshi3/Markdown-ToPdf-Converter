# MarkdownToPdfConverter Project Overview

## Project Summary
MarkdownToPdfConverter is a desktop application built with the Avalonia framework and the .NET technology stack, designed to assist users in converting Markdown formatted text files to PDF format. The application offers a user-friendly interface that supports file upload and conversion operations, making document management and processing simple for users.

![092919](https://github.com/user-attachments/assets/fac4696b-861b-4100-99cb-e9d7eb740c80)

## Tech Stack
- **Avalonia**: A modern, responsive UI framework for cross-platform applications.
- **.NET**: A powerful development platform that supports multiple programming languages and a rich set of libraries.

## Features
- **File Upload**: Users can select and upload Markdown files, and the application will automatically read the file contents.
- **Markdown Editing**: Users can edit the Markdown content directly in a text box.
- **PDF Conversion**: Converts the Markdown text to a PDF file and saves it to a specified location.
- **Status Feedback**: The application provides status messages that give real-time feedback on upload and conversion operations.

## How to Use
1. Launch the application.
2. Click the "Select Markdown File" button to upload a Markdown file.
3. Edit or view the Markdown text content.
4. Click the "Convert to PDF" button to choose a save location and complete the PDF file generation.

## Code Structure
- `MainViewModel` class: Responsible for handling user interface logic, managing file paths, Markdown content, and conversion commands.
- `UploadFileAsync` method: Handles file upload logic, reading the Markdown file content.
- `ConvertToPdfAsync` method: Implements the logic to convert the Markdown content to PDF.

## Conclusion
MarkdownToPdfConverter is a simple yet practical tool aimed at enhancing user efficiency when dealing with documents. By utilizing a modern tech stack, the application is not only aesthetically pleasing but also powerful, capable of meeting users' everyday file conversion needs.
