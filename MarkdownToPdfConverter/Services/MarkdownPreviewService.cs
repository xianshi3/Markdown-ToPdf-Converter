using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Markdig.Extensions.Tables;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MarkdownToPdfConverter.Models;

namespace MarkdownToPdfConverter.Services
{
    public class MarkdownPreviewService
    {
        private readonly MarkdownPipeline _pipeline;

        public MarkdownPreviewService()
        {
            _pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .UsePipeTables()
                .UseEmphasisExtras()
                .UseAutoLinks()
                .UseTaskLists()
                .UseAutoIdentifiers()
                .Build();
        }

        public List<PreviewBlock> Parse(string markdown)
        {
            var blocks = new List<PreviewBlock>();

            if (string.IsNullOrWhiteSpace(markdown))
            {
                blocks.Add(new PreviewBlock { Type = PreviewBlockType.Empty, Text = "Preview will appear here..." });
                return blocks;
            }

            var doc = Markdown.Parse(markdown, _pipeline);

            foreach (var block in doc)
            {
                ProcessBlock(block, blocks);
            }

            if (blocks.Count == 0)
            {
                blocks.Add(new PreviewBlock { Type = PreviewBlockType.Empty, Text = "Preview will appear here..." });
            }

            return blocks;
        }

        private void ProcessBlock(Block block, List<PreviewBlock> blocks)
        {
            switch (block)
            {
                case HeadingBlock heading:
                    var text = ExtractInlineText(heading.Inline);
                    blocks.Add(new PreviewBlock
                    {
                        Type = (PreviewBlockType)((int)PreviewBlockType.Heading1 + heading.Level - 1),
                        Text = text,
                        Level = heading.Level
                    });
                    break;

                case ParagraphBlock para:
                    var paraText = ExtractInlineText(para.Inline);
                    if (!string.IsNullOrWhiteSpace(paraText))
                    {
                        blocks.Add(new PreviewBlock { Type = PreviewBlockType.Paragraph, Text = paraText });
                    }
                    break;

                case CodeBlock codeBlock:
                    var code = new StringBuilder();
                    foreach (var line in codeBlock.Lines.Lines)
                    {
                        code.AppendLine(line.ToString());
                    }
                    blocks.Add(new PreviewBlock { Type = PreviewBlockType.CodeBlock, Text = code.ToString().TrimEnd() });
                    break;

                case QuoteBlock:
                    var quoteText = ExtractBlockText(block);
                    blocks.Add(new PreviewBlock { Type = PreviewBlockType.BlockQuote, Text = quoteText });
                    break;

                case ListBlock listBlock:
                    foreach (var item in listBlock)
                    {
                        if (item is ListItemBlock listItem)
                        {
                            var itemText = ExtractBlockText(listItem);
                            if (!string.IsNullOrWhiteSpace(itemText))
                            {
                                blocks.Add(new PreviewBlock
                                {
                                    Type = listBlock.IsOrdered ? PreviewBlockType.OrderedListItem : PreviewBlockType.ListItem,
                                    Text = itemText
                                });
                            }
                        }
                    }
                    break;

                case ThematicBreakBlock:
                    blocks.Add(new PreviewBlock { Type = PreviewBlockType.HorizontalRule });
                    break;

                case Table table:
                    var tableText = RenderTable(table);
                    blocks.Add(new PreviewBlock { Type = PreviewBlockType.Table, Text = tableText });
                    break;
            }
        }

        private static string ExtractInlineText(Inline? inline)
        {
            if (inline == null) return string.Empty;
            var sb = new StringBuilder();
            if (inline is ContainerInline container)
            {
                foreach (var child in container)
                {
                    switch (child)
                    {
                        case LiteralInline literal:
                            sb.Append(literal.Content);
                            break;
                        case CodeInline code:
                            sb.Append(code.Content);
                            break;
                        case LinkInline link:
                            sb.Append(ExtractInlineText(link));
                            break;
                        case LineBreakInline:
                            sb.Append(' ');
                            break;
                    }
                }
            }
            return sb.ToString().Trim();
        }

        private static string ExtractBlockText(Block block)
        {
            var sb = new StringBuilder();
            foreach (var sub in block.Descendants())
            {
                if (sub is ParagraphBlock p)
                {
                    sb.Append(ExtractInlineText(p.Inline));
                    sb.Append(" ");
                }
            }
            return sb.ToString().Trim();
        }

        private static string RenderTable(Table table)
        {
            var sb = new StringBuilder();

            foreach (var row in table)
            {
                if (row is TableRow tableRow)
                {
                    var cells = new List<string>();
                    foreach (var cell in tableRow)
                    {
                        if (cell is TableCell tableCell)
                        {
                            var cellText = ExtractBlockText(tableCell);
                            cells.Add(cellText);
                        }
                    }
                    sb.AppendLine(string.Join(" | ", cells));
                }
            }

            return sb.ToString().TrimEnd();
        }
    }
}
