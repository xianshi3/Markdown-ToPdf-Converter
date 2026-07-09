using System;
using MarkdownToPdfConverter.Services;

namespace MarkdownToPdfConverter.Converters
{
    public static class BlockTypeConverters
    {
        public static bool H1(object? value) => value is PreviewBlock block && block.Type == BlockType.H1;
        public static bool H2(object? value) => value is PreviewBlock block && block.Type == BlockType.H2;
        public static bool H3(object? value) => value is PreviewBlock block && block.Type == BlockType.H3;
        public static bool H4(object? value) => value is PreviewBlock block && block.Type == BlockType.H4;
        public static bool H5(object? value) => value is PreviewBlock block && block.Type == BlockType.H5;
        public static bool H6(object? value) => value is PreviewBlock block && block.Type == BlockType.H6;
        public static bool Paragraph(object? value) => value is PreviewBlock block && block.Type == BlockType.Paragraph;
        public static bool CodeBlock(object? value) => value is PreviewBlock block && block.Type == BlockType.CodeBlock;
        public static bool Blockquote(object? value) => value is PreviewBlock block && block.Type == BlockType.Blockquote;
        public static bool List(object? value) => value is PreviewBlock block && (block.Type == BlockType.UnorderedList || block.Type == BlockType.OrderedList);
        public static bool HorizontalRule(object? value) => value is PreviewBlock block && block.Type == BlockType.HorizontalRule;
        public static bool Empty(object? value) => value is PreviewBlock block && block.Type == BlockType.Empty;
        public static bool Image(object? value) => value is PreviewBlock block && block.Type == BlockType.Image;
        public static bool Link(object? value) => value is PreviewBlock block && block.Type == BlockType.Link;
    }
}