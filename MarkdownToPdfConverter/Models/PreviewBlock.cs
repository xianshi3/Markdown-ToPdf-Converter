using Avalonia.Media;

namespace MarkdownToPdfConverter.Models
{
    public enum PreviewBlockType
    {
        Heading1, Heading2, Heading3, Heading4, Heading5, Heading6,
        Paragraph, CodeBlock, BlockQuote, ListItem, OrderedListItem,
        HorizontalRule, Table, Empty
    }

    public class PreviewBlock
    {
        public PreviewBlockType Type { get; set; }
        public string Text { get; set; } = string.Empty;
        public int Level { get; set; }

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

        public FontWeight DisplayFontWeight =>
            Type >= PreviewBlockType.Heading1 && Type <= PreviewBlockType.Heading6
                ? FontWeight.Bold
                : FontWeight.Normal;

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

        public double LeftIndent => Type switch
        {
            PreviewBlockType.ListItem => 16,
            PreviewBlockType.OrderedListItem => 16,
            PreviewBlockType.CodeBlock => 12,
            PreviewBlockType.BlockQuote => 12,
            _ => 0
        };

        public bool IsItalic => Type == PreviewBlockType.BlockQuote;
        public bool IsMonospace => Type == PreviewBlockType.CodeBlock;
        public bool IsHorizontalRule => Type == PreviewBlockType.HorizontalRule;
        public bool IsEmpty => Type == PreviewBlockType.Empty;
        public bool IsCode => Type == PreviewBlockType.CodeBlock;

        public bool IsHeading =>
            Type >= PreviewBlockType.Heading1 && Type <= PreviewBlockType.Heading6;

        public bool IsParagraph => Type == PreviewBlockType.Paragraph;
        public bool IsBlockQuote => Type == PreviewBlockType.BlockQuote;
        public bool IsListItem => Type == PreviewBlockType.ListItem;
        public bool IsOrderedListItem => Type == PreviewBlockType.OrderedListItem;
        public bool IsTable => Type == PreviewBlockType.Table;

        public double HeadingLineOpacity => Type == PreviewBlockType.Heading1 ? 0.6 : 0.3;
        public int ListNumber { get; set; }
    }
}
