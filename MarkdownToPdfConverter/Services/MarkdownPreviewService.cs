using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web;
using Markdig;
using Avalonia.Media;
using ReactiveUI;

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
                .UseEmojiAndSmiley()
                .Build();
        }

        public List<PreviewBlock> ParseToBlocks(string markdown)
        {
            var blocks = new List<PreviewBlock>();
            if (string.IsNullOrWhiteSpace(markdown))
                return blocks;

            var lines = markdown.Split('\n');
            bool inCodeBlock = false;
            string codeLanguage = "";
            var codeLines = new List<string>();
            List<string>? listItems = null;
            bool inList = false;

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var trimmed = line.TrimStart();

                if (trimmed.StartsWith("```"))
                {
                    if (!inCodeBlock)
                    {
                        if (inList && listItems != null && listItems.Count > 0)
                        {
                            blocks.Add(new PreviewBlock
                            {
                                Type = BlockType.UnorderedList,
                                Items = new List<string>(listItems)
                            });
                            listItems = null;
                            inList = false;
                        }
                        inCodeBlock = true;
                        codeLanguage = trimmed.Substring(3).Trim();
                        codeLines.Clear();
                    }
                    else
                    {
                        blocks.Add(new PreviewBlock
                        {
                            Type = BlockType.CodeBlock,
                            Content = string.Join("\n", codeLines),
                            Language = codeLanguage
                        });
                        inCodeBlock = false;
                        codeLines.Clear();
                    }
                    continue;
                }

                if (inCodeBlock)
                {
                    codeLines.Add(line);
                    continue;
                }

                if (trimmed.StartsWith("|") && trimmed.Contains("|"))
                {
                    if (inList && listItems != null && listItems.Count > 0)
                    {
                        blocks.Add(new PreviewBlock
                        {
                            Type = BlockType.UnorderedList,
                            Items = new List<string>(listItems)
                        });
                        listItems = null;
                        inList = false;
                    }
                    continue;
                }

                var listMatch = Regex.Match(trimmed, @"^(\* |\+ |- |\d+\. )\s*(.*)");
                if (listMatch.Success)
                {
                    if (!inList)
                    {
                        inList = true;
                        listItems = new List<string>();
                    }
                    listItems.Add(listMatch.Groups[2].Value);
                    continue;
                }
                else if (inList && listItems != null && listItems.Count > 0)
                {
                    blocks.Add(new PreviewBlock
                    {
                        Type = trimmed.StartsWith("1.") || Regex.IsMatch(trimmed, @"^\d+\.\s") 
                            ? BlockType.OrderedList 
                            : BlockType.UnorderedList,
                        Items = new List<string>(listItems)
                    });
                    listItems = null;
                    inList = false;
                }

                if (trimmed.StartsWith("#"))
                {
                    var hashMatch = Regex.Match(trimmed, @"^(#{1,6})\s+(.*)");
                    if (hashMatch.Success)
                    {
                        int level = hashMatch.Groups[1].Value.Length;
                        string content = hashMatch.Groups[2].Value;
                        blocks.Add(new PreviewBlock
                        {
                            Type = (BlockType)((int)BlockType.H1 + level - 1),
                            Content = content
                        });
                    }
                }
                else if (trimmed.StartsWith("> "))
                {
                    blocks.Add(new PreviewBlock { Type = BlockType.Blockquote, Content = trimmed.Substring(2) });
                }
                else if (trimmed == "---" || trimmed == "***" || trimmed == "___")
                {
                    blocks.Add(new PreviewBlock { Type = BlockType.HorizontalRule });
                }
                else if (trimmed.StartsWith("![]") || trimmed.StartsWith("["))
                {
                    var imgMatch = Regex.Match(trimmed, @"!\[(.*?)\]\((.+?)\)");
                    if (imgMatch.Success)
                    {
                        blocks.Add(new PreviewBlock
                        {
                            Type = BlockType.Image,
                            Content = imgMatch.Groups[1].Value,
                            ImageUrl = imgMatch.Groups[2].Value
                        });
                    }
                    else
                    {
                        var linkMatch = Regex.Match(trimmed, @"\[(.+?)\]\((.+?)\)");
                        if (linkMatch.Success)
                        {
                            blocks.Add(new PreviewBlock
                            {
                                Type = BlockType.Link,
                                Content = linkMatch.Groups[1].Value,
                                LinkUrl = linkMatch.Groups[2].Value
                            });
                        }
                        else if (trimmed.Length > 0)
                        {
                            blocks.Add(new PreviewBlock { Type = BlockType.Paragraph, Content = trimmed });
                        }
                    }
                }
                else if (trimmed.Length > 0)
                {
                    blocks.Add(new PreviewBlock { Type = BlockType.Paragraph, Content = trimmed });
                }
                else
                {
                    blocks.Add(new PreviewBlock { Type = BlockType.Empty });
                }
            }

            if (inList && listItems != null && listItems.Count > 0)
            {
                blocks.Add(new PreviewBlock
                {
                    Type = BlockType.UnorderedList,
                    Items = new List<string>(listItems)
                });
            }

            return blocks;
        }

        public string ParseInline(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            text = HttpUtility.HtmlEncode(text);
            text = Regex.Replace(text, @"\*\*(.+?)\*\*", "**$1**");
            text = Regex.Replace(text, @"\*(.+?)\*", "*$1*");
            text = Regex.Replace(text, @"__(.+?)__", "__$1__");
            text = Regex.Replace(text, @"_(.+?)_", "_$1_");
            text = Regex.Replace(text, @"~~(.+?)~~", "~~$1~~");
            text = Regex.Replace(text, @"`(.+?)`", "`$1`");
            text = Regex.Replace(text, @"\[(.+?)\]\((.+?)\)", "[$1]($2)");

            return text;
        }
    }

    public enum BlockType
    {
        H1, H2, H3, H4, H5, H6,
        Paragraph, CodeBlock, Blockquote,
        UnorderedList, OrderedList,
        HorizontalRule, Empty, Image, Link
    }

    public class PreviewBlock : ReactiveObject
    {
        private bool _isExpanded = true;

        public BlockType Type { get; set; }
        public string Content { get; set; } = "";
        public string Language { get; set; } = "";
        public List<string>? Items { get; set; }
        public string ImageUrl { get; set; } = "";
        public string LinkUrl { get; set; } = "";
        
        public bool IsExpanded
        {
            get => _isExpanded;
            set => this.RaiseAndSetIfChanged(ref _isExpanded, value);
        }

        public double FontSize => Type switch
        {
            BlockType.H1 => 28,
            BlockType.H2 => 24,
            BlockType.H3 => 20,
            BlockType.H4 => 18,
            BlockType.H5 => 16,
            BlockType.H6 => 14,
            BlockType.CodeBlock => 13,
            _ => 14
        };

        public string FontWeight => Type switch
        {
            BlockType.H1 or BlockType.H2 or BlockType.H3 or BlockType.H4 => "Bold",
            _ => "Normal"
        };

        public string ForegroundColor => Type switch
        {
            BlockType.H1 => "#E6EDF3",
            BlockType.H2 => "#E6EDF3",
            BlockType.H3 => "#E6EDF3",
            BlockType.H4 => "#E6EDF3",
            BlockType.H5 => "#E6EDF3",
            BlockType.H6 => "#E6EDF3",
            BlockType.CodeBlock => "#7EE787",
            BlockType.Blockquote => "#8B949E",
            _ => "#E6EDF3"
        };

        public string MarginTop => Type switch
        {
            BlockType.H1 => "16",
            BlockType.H2 => "14",
            BlockType.H3 => "12",
            BlockType.H4 => "10",
            BlockType.H5 => "8",
            BlockType.H6 => "6",
            BlockType.CodeBlock => "8",
            BlockType.Blockquote => "8",
            _ => "4"
        };

        public string MarginBottom => Type switch
        {
            BlockType.H1 => "12",
            BlockType.H2 => "10",
            BlockType.H3 => "8",
            BlockType.H4 => "6",
            BlockType.H5 => "4",
            BlockType.H6 => "4",
            BlockType.CodeBlock => "8",
            BlockType.Blockquote => "8",
            _ => "4"
        };
    }
}