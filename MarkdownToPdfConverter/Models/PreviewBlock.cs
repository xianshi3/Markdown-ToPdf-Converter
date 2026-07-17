using Avalonia.Media;

namespace MarkdownToPdfConverter.Models
{
    /// <summary>Represents the type of a parsed Markdown block element.</summary>
    public enum PreviewBlockType
    {
        Heading1, Heading2, Heading3, Heading4, Heading5, Heading6,
        Paragraph, CodeBlock, BlockQuote, ListItem, OrderedListItem,
        HorizontalRule, Table, Empty
    }

    /// <summary>Represents a single block element parsed from Markdown for preview rendering.</summary>
    public class PreviewBlock
    {
        /// <summary>The block type (heading, paragraph, code, etc.).</summary>
        public PreviewBlockType Type { get; set; }
        /// <summary>The raw text content of the block.</summary>
        public string Text { get; set; } = string.Empty;
        /// <summary>The heading level (1-6), zero for non-heading blocks.</summary>
        public int Level { get; set; }

        /// <summary>Gets the font size based on the block type.</summary>
        public double DisplayFontSize => Type switch
        {
            PreviewBlockType.Heading1 => 22,
            PreviewBlockType.Heading2 => 19,
            PreviewBlockType.Heading3 => 16,
            PreviewBlockType.Heading4 => 14,
            PreviewBlockType.Heading5 => 13,
            PreviewBlockType.Heading6 => 12,
            PreviewBlockType.CodeBlock => 11,
            PreviewBlockType.BlockQuote => 12,
            PreviewBlockType.Empty => 12,
            _ => 12
        };

        /// <summary>Gets the font weight — bold for headings, normal otherwise.</summary>
        public FontWeight DisplayFontWeight =>
            Type >= PreviewBlockType.Heading1 && Type <= PreviewBlockType.Heading6
                ? FontWeight.Bold
                : FontWeight.Normal;

        /// <summary>Gets the line height in pixels for the block type.</summary>
        public double LineHeight => Type switch
        {
            PreviewBlockType.Heading1 => 32,
            PreviewBlockType.Heading2 => 28,
            PreviewBlockType.Heading3 => 24,
            PreviewBlockType.Heading4 => 22,
            PreviewBlockType.Heading5 => 20,
            PreviewBlockType.Heading6 => 20,
            PreviewBlockType.Paragraph => 20,
            PreviewBlockType.ListItem or PreviewBlockType.OrderedListItem => 20,
            PreviewBlockType.HorizontalRule => 0,
            PreviewBlockType.Empty => 20,
            _ => 20
        };

        /// <summary>Gets the spacing before the block in pixels.</summary>
        public double SpacingBefore => Type switch
        {
            PreviewBlockType.Heading1 => 12,
            PreviewBlockType.Heading2 => 10,
            PreviewBlockType.Heading3 => 8,
            PreviewBlockType.Heading4 => 6,
            PreviewBlockType.Heading5 => 4,
            PreviewBlockType.Heading6 => 4,
            PreviewBlockType.Paragraph => 2,
            PreviewBlockType.HorizontalRule => 8,
            _ => 0
        };

        /// <summary>Gets the spacing after the block in pixels.</summary>
        public double SpacingAfter => Type switch
        {
            PreviewBlockType.Heading1 => 8,
            PreviewBlockType.Heading2 => 6,
            PreviewBlockType.Heading3 => 4,
            PreviewBlockType.Heading4 => 4,
            PreviewBlockType.Heading5 => 2,
            PreviewBlockType.Heading6 => 2,
            PreviewBlockType.CodeBlock => 8,
            PreviewBlockType.BlockQuote => 8,
            PreviewBlockType.HorizontalRule => 8,
            _ => 2
        };

        /// <summary>Gets the left indent for list items, code blocks, and block quotes.</summary>
        public double LeftIndent => Type switch
        {
            PreviewBlockType.ListItem => 16,
            PreviewBlockType.OrderedListItem => 16,
            PreviewBlockType.CodeBlock => 12,
            PreviewBlockType.BlockQuote => 12,
            _ => 0
        };

        /// <summary>True if the block is a block quote (rendered in italic).</summary>
        public bool IsItalic => Type == PreviewBlockType.BlockQuote;
        /// <summary>True if the block is a code block (rendered in monospace).</summary>
        public bool IsMonospace => Type == PreviewBlockType.CodeBlock;
        /// <summary>True if the block is a horizontal rule.</summary>
        public bool IsHorizontalRule => Type == PreviewBlockType.HorizontalRule;
        /// <summary>True if the block is empty.</summary>
        public bool IsEmpty => Type == PreviewBlockType.Empty;
        /// <summary>True if the block is a code block.</summary>
        public bool IsCode => Type == PreviewBlockType.CodeBlock;

        /// <summary>True if the block is a heading (levels 1-6).</summary>
        public bool IsHeading =>
            Type >= PreviewBlockType.Heading1 && Type <= PreviewBlockType.Heading6;

        /// <summary>True if the block is a paragraph.</summary>
        public bool IsParagraph => Type == PreviewBlockType.Paragraph;
        /// <summary>True if the block is a block quote.</summary>
        public bool IsBlockQuote => Type == PreviewBlockType.BlockQuote;
        /// <summary>True if the block is an unordered list item.</summary>
        public bool IsListItem => Type == PreviewBlockType.ListItem;
        /// <summary>True if the block is an ordered list item.</summary>
        public bool IsOrderedListItem => Type == PreviewBlockType.OrderedListItem;
        /// <summary>True if the block is a table.</summary>
        public bool IsTable => Type == PreviewBlockType.Table;

        /// <summary>Opacity of the decorative line under headings (H1 brighter).</summary>
        public double HeadingLineOpacity => Type == PreviewBlockType.Heading1 ? 0.6 : 0.3;
        /// <summary>The ordinal number for ordered list items.</summary>
        public int ListNumber { get; set; }
    }
}
