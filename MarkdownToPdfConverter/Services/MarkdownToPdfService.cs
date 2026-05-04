using MigraDocCore.DocumentObjectModel;
using MigraDocCore.Rendering;
using PdfSharpCore.Fonts;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Markdig;
using HtmlAgilityPack;

namespace MarkdownToPdfConverter.Services
{
    public class MarkdownToPdfService
    {
        private Document _document;
        private Section? _section;
        private MarkdownPipeline _pipeline;

        private static readonly (string pattern, string color)[] _csharpKeywords = new[]
        {
            ("\\bpublic\\b", "Blue"), ("\\bprivate\\b", "Blue"), ("\\bprotected\\b", "Blue"), ("\\binternal\\b", "Blue"),
            ("\\bclass\\b", "Blue"), ("\\bstruct\\b", "Blue"), ("\\binterface\\b", "Blue"), ("\\benum\\b", "Blue"),
            ("\\bnamespace\\b", "Blue"), ("\\busing\\b", "Blue"), ("\\bvoid\\b", "Blue"), ("\\bvar\\b", "Blue"),
            ("\\bstring\\b", "Blue"), ("\\bint\\b", "Blue"), ("\\bbool\\b", "Blue"), ("\\bdouble\\b", "Blue"),
            ("\\bfloat\\b", "Blue"), ("\\bdecimal\\b", "Blue"), ("\\bchar\\b", "Blue"), ("\\bbyte\\b", "Blue"),
            ("\\bnew\\b", "Blue"), ("\\bthis\\b", "Blue"), ("\\bbase\\b", "Blue"), ("\\bstatic\\b", "Blue"),
            ("\\bconst\\b", "Blue"), ("\\breadonly\\b", "Blue"), ("\\bvirtual\\b", "Blue"), ("\\boverride\\b", "Blue"),
            ("\\babstract\\b", "Blue"), ("\\bsealed\\b", "Blue"), ("\\bpartial\\b", "Blue"), ("\\basync\\b", "Blue"),
            ("\\bawait\\b", "Blue"), ("\\breturn\\b", "Blue"), ("\\bthrow\\b", "Blue"), ("\\btry\\b", "Blue"),
            ("\\bcatch\\b", "Blue"), ("\\bfinally\\b", "Blue"), ("\\bif\\b", "Blue"), ("\\belse\\b", "Blue"),
            ("\\bfor\\b", "Blue"), ("\\bforeach\\b", "Blue"), ("\\bwhile\\b", "Blue"), ("\\bdo\\b", "Blue"),
            ("\\bswitch\\b", "Blue"), ("\\bcase\\b", "Blue"), ("\\bbreak\\b", "Blue"), ("\\bcontinue\\b", "Blue"),
            ("\\bnull\\b", "Blue"), ("\\btrue\\b", "Blue"), ("\\bfalse\\b", "Blue"), ("\\btypeof\\b", "Blue"),
            ("\\bnameof\\b", "Blue"), ("\\bis\\b", "Blue"), ("\\bas\\b", "Blue"), ("\\bin\\b", "Blue"),
            ("\\bout\\b", "Blue"), ("\\bref\\b", "Blue"), ("\\bparams\\b", "Blue"), ("\\bget\\b", "Blue"),
            ("\\bset\\b", "Blue"), ("\\bvalue\\b", "Blue"), ("\\badd\\b", "Blue"), ("\\bremove\\b", "Blue"),
        };

        private static readonly (string pattern, string color)[] _pythonKeywords = new[]
        {
            ("\\bdef\\b", "Blue"), ("\\bclass\\b", "Blue"), ("\\bif\\b", "Blue"), ("\\belif\\b", "Blue"),
            ("\\belse\\b", "Blue"), ("\\bfor\\b", "Blue"), ("\\bwhile\\b", "Blue"), ("\\breturn\\b", "Blue"),
            ("\\bNone\\b", "Blue"), ("\\bTrue\\b", "Blue"), ("\\bFalse\\b", "Blue"), ("\\bimport\\b", "Blue"),
            ("\\bfrom\\b", "Blue"), ("\\bas\\b", "Blue"), ("\\btry\\b", "Blue"), ("\\bexcept\\b", "Blue"),
            ("\\bfinally\\b", "Blue"), ("\\braise\\b", "Blue"), ("\\bwith\\b", "Blue"), ("\\basync\\b", "Blue"),
            ("\\bawait\\b", "Blue"), ("\\blambda\\b", "Blue"), ("\\bglobal\\b", "Blue"), ("\\bnonlocal\\b", "Blue"),
            ("\\bassert\\b", "Blue"), ("\\byield\\b", "Blue"), ("\\bdel\\b", "Blue"), ("\\bpass\\b", "Blue"),
            ("\\bbreak\\b", "Blue"), ("\\bcontinue\\b", "Blue"), ("\\breturn\\b", "Blue"),
        };

        private static readonly (string pattern, string color)[] _jsKeywords = new[]
        {
            ("\\bfunction\\b", "Blue"), ("\\bvar\\b", "Blue"), ("\\blet\\b", "Blue"), ("\\bconst\\b", "Blue"),
            ("\\bif\\b", "Blue"), ("\\belse\\b", "Blue"), ("\\bfor\\b", "Blue"), ("\\bwhile\\b", "Blue"),
            ("\\breturn\\b", "Blue"), ("\\bbreak\\b", "Blue"), ("\\bcontinue\\b", "Blue"), ("\\bswitch\\b", "Blue"),
            ("\\bcase\\b", "Blue"), ("\\bdefault\\b", "Blue"), ("\\btry\\b", "Blue"), ("\\bcatch\\b", "Blue"),
            ("\\bfinally\\b", "Blue"), ("\\bthrow\\b", "Blue"), ("\\bnew\\b", "Blue"), ("\\bthis\\b", "Blue"),
            ("\\bclass\\b", "Blue"), ("\\bextends\\b", "Blue"), ("\\bsuper\\b", "Blue"), ("\\bimport\\b", "Blue"),
            ("\\bexport\\b", "Blue"), ("\\basync\\b", "Blue"), ("\\bawait\\b", "Blue"), ("\\byield\\b", "Blue"),
            ("\\bnull\\b", "Blue"), ("\\bundefined\\b", "Blue"), ("\\btrue\\b", "Blue"), ("\\bfalse\\b", "Blue"),
            ("\\btypeof\\b", "Blue"), ("\\binstanceof\\b", "Blue"), ("\\bdelete\\b", "Blue"), ("\\bvoid\\b", "Blue"),
        };

        public MarkdownToPdfService()
        {
            _document = new Document();
            _pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .UsePipeTables()
                .UseEmphasisExtras()
                .UseAutoLinks()
                .UseTaskLists()
                .UseEmojiAndSmiley()
                .UseAutoIdentifiers()
                .UseFootnotes()
                .UseDefinitionLists()
                .Build();
        }

        public void ConvertMarkdownToPdf(string markdownContent, Stream outputStream)
        {
            try
            {
                GlobalFontSettings.FontResolver = new CustomFontResolver();
                SetupDocumentStyles();

                string html = Markdown.ToHtml(markdownContent, _pipeline);

                _section = _document.AddSection();
                _section.PageSetup.TopMargin = "2cm";
                _section.PageSetup.BottomMargin = "2cm";
                _section.PageSetup.LeftMargin = "2.5cm";
                _section.PageSetup.RightMargin = "2.5cm";

                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(html);

                if (htmlDoc.DocumentNode.SelectSingleNode("//meta[@name='generator']") != null)
                {
                    htmlDoc.DocumentNode.SelectSingleNode("//meta[@name='generator']")?.Remove();
                }

                htmlDoc.DocumentNode.Descendants()
                    .Where(n => n.NodeType == HtmlNodeType.Text && string.IsNullOrWhiteSpace(n.InnerText))
                    .ToList()
                    .ForEach(n => n.Remove());

                ProcessDocument(htmlDoc.DocumentNode);

                var renderer = new PdfDocumentRenderer(true);
                renderer.Document = _document;
                renderer.RenderDocument();

                renderer.PdfDocument.Save(outputStream);
            }
            catch (Exception ex)
            {
                throw new Exception($"转换失败: {ex.Message}", ex);
            }
        }

        private void SetupDocumentStyles()
        {
            var normalStyle = _document.Styles["Normal"];
            normalStyle.Font.Name = "SimSun";
            normalStyle.Font.Size = 10;
            normalStyle.ParagraphFormat.SpaceAfter = 5;

            var heading1 = _document.Styles["Heading1"];
            heading1.Font.Size = 20;
            heading1.Font.Bold = true;
            heading1.ParagraphFormat.SpaceBefore = 15;
            heading1.ParagraphFormat.SpaceAfter = 10;

            var heading2 = _document.Styles["Heading2"];
            heading2.Font.Size = 16;
            heading2.Font.Bold = true;
            heading2.ParagraphFormat.SpaceBefore = 12;
            heading2.ParagraphFormat.SpaceAfter = 8;

            var heading3 = _document.Styles["Heading3"];
            heading3.Font.Size = 14;
            heading3.Font.Bold = true;
            heading3.ParagraphFormat.SpaceBefore = 10;
            heading3.ParagraphFormat.SpaceAfter = 6;

            var heading4 = _document.Styles["Heading4"];
            heading4.Font.Size = 12;
            heading4.Font.Bold = true;
            heading4.ParagraphFormat.SpaceBefore = 8;
            heading4.ParagraphFormat.SpaceAfter = 4;

            var heading5 = _document.Styles["Heading5"];
            heading5.Font.Size = 11;
            heading5.Font.Bold = true;
            heading5.ParagraphFormat.SpaceBefore = 6;
            heading5.ParagraphFormat.SpaceAfter = 3;

            var heading6 = _document.Styles["Heading6"];
            heading6.Font.Size = 10;
            heading6.Font.Bold = true;
            heading6.ParagraphFormat.SpaceBefore = 5;
            heading6.ParagraphFormat.SpaceAfter = 2;
        }

        private void ProcessDocument(HtmlNode node)
        {
            foreach (var child in node.ChildNodes)
            {
                if (child.NodeType == HtmlNodeType.Element)
                {
                    ProcessElement(child);
                }
            }
        }

        private void ProcessElement(HtmlNode node)
        {
            string tag = node.Name.ToLower();

            switch (tag)
            {
                case "h1": AddHeading(node, 1); break;
                case "h2": AddHeading(node, 2); break;
                case "h3": AddHeading(node, 3); break;
                case "h4": AddHeading(node, 4); break;
                case "h5": AddHeading(node, 5); break;
                case "h6": AddHeading(node, 6); break;
                case "p": AddParagraph(node); break;
                case "pre": AddCodeBlock(node); break;
                case "blockquote": AddBlockQuote(node); break;
                case "ul":
                case "ol": AddList(node, tag == "ol"); break;
                case "hr": AddHorizontalLine(); break;
                case "table": AddTable(node); break;
                case "dl": AddDefinitionList(node); break;
                case "figure": AddFigure(node); break;
                case "figcaption": break;
                case "details": AddDetails(node); break;
                case "summary": break;
                case "abbr": AddAbbreviation(node); break;
                case "address": AddAddress(node); break;
                case "cite": AddCitation(node); break;
                case "samp": AddSample(node); break;
                case "kbd": AddKeyboard(node); break;
                case "var": AddVariable(node); break;
                case "time": AddTime(node); break;
                case "mark": AddMarked(node); break;
                case "ruby": AddRuby(node); break;
                case "rt": break;
                case "rp": break;
                case "wbr": AddWordBreak(node); break;
                case "input": AddInput(node); break;
                case "progress": AddProgress(node); break;
                case "meter": AddMeter(node); break;
                case "audio":
                case "video": AddMedia(node); break;
                case "iframe": AddIframe(node); break;
                default: ProcessChildNodes(node); break;
            }
        }

        private void AddHeading(HtmlNode node, int level)
        {
            var para = _section!.AddParagraph();
            para.Style = $"Heading{level}";
            ProcessInlineElements(para, node);
        }

        private void AddParagraph(HtmlNode node)
        {
            var para = _section!.AddParagraph();
            para.Format.SpaceAfter = 6;
            ProcessInlineElements(para, node);
        }

        private void ProcessInlineElements(Paragraph para, HtmlNode node)
        {
            foreach (var child in node.ChildNodes)
            {
                if (child.NodeType == HtmlNodeType.Text)
                {
                    var text = child.InnerText.Trim();
                    if (!string.IsNullOrEmpty(text))
                        para.AddText(text);
                }
                else if (child.NodeType == HtmlNodeType.Element)
                {
                    ProcessInlineElement(para, child);
                }
            }
        }

        private void ProcessInlineElement(Paragraph para, HtmlNode node)
        {
            string tag = node.Name.ToLower();

            switch (tag)
            {
                case "strong":
                case "b":
                    var bold = para.AddFormattedText(HtmlEntity(node.InnerText.Trim()));
                    bold.Font.Bold = true;
                    break;
                case "em":
                case "i":
                    var italic = para.AddFormattedText(HtmlEntity(node.InnerText.Trim()));
                    italic.Font.Italic = true;
                    break;
                case "code":
                    var code = para.AddFormattedText(node.InnerText.Trim());
                    code.Font.Name = "Consolas";
                    code.Font.Size = 9;
                    break;
                case "a":
                    var link = para.AddFormattedText(HtmlEntity(node.InnerText.Trim()));
                    link.Font.Underline = Underline.Single;
                    break;
                case "br":
                    para.AddLineBreak();
                    break;
                case "del":
                case "s":
                    var strike = para.AddFormattedText(HtmlEntity(node.InnerText.Trim()));
                    strike.Font.Underline = Underline.Single;
                    break;
                case "u":
                    var underline = para.AddFormattedText(HtmlEntity(node.InnerText.Trim()));
                    underline.Font.Underline = Underline.Single;
                    break;
                case "sup":
                    var sup = para.AddFormattedText(HtmlEntity(node.InnerText.Trim()));
                    break;
                case "sub":
                    var sub = para.AddFormattedText(HtmlEntity(node.InnerText.Trim()));
                    break;
                case "small":
                    var small = para.AddFormattedText(HtmlEntity(node.InnerText.Trim()));
                    small.Font.Size = 8;
                    break;
                case "ins":
                    var ins = para.AddFormattedText(HtmlEntity(node.InnerText.Trim()));
                    ins.Font.Underline = Underline.Single;
                    break;
                case "q":
                    var quote = para.AddFormattedText("\"" + HtmlEntity(node.InnerText.Trim()) + "\"");
                    quote.Font.Italic = true;
                    break;
                case "kbd":
                    var kbd = para.AddFormattedText(node.InnerText.Trim());
                    kbd.Font.Name = "Consolas";
                    kbd.Font.Size = 9;
                    break;
                case "var":
                    var variable = para.AddFormattedText(node.InnerText.Trim());
                    variable.Font.Italic = true;
                    variable.Font.Bold = true;
                    break;
                case "samp":
                    var samp = para.AddFormattedText(node.InnerText.Trim());
                    samp.Font.Name = "Consolas";
                    samp.Font.Size = 9;
                    break;
                case "mark":
                    var mark = para.AddFormattedText(node.InnerText.Trim());
                    mark.Font.Color = Colors.Yellow;
                    break;
                case "span":
                    if (node.Attributes["class"]?.Value.Contains("strikethrough") == true)
                    {
                        var ss = para.AddFormattedText(HtmlEntity(node.InnerText.Trim()));
                        ss.Font.Underline = Underline.Single;
                    }
                    else
                    {
                        ProcessInlineElements(para, node);
                    }
                    break;
                case "input":
                    if (node.Attributes["type"]?.Value == "checkbox")
                    {
                        var isChecked = node.Attributes["checked"] != null;
                        para.AddText(isChecked ? "[x] " : "[ ] ");
                    }
                    break;
                default:
                    if (node.HasChildNodes)
                        ProcessInlineElements(para, node);
                    else
                        para.AddText(node.InnerText);
                    break;
            }
        }

        private void AddCodeBlock(HtmlNode node)
        {
            var codeNode = node.SelectSingleNode("code");
            var code = codeNode?.InnerText ?? node.InnerText;
            code = code.Trim();

            string language = DetectLanguage(codeNode);

            var lines = code.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var para = _section!.AddParagraph();
                para.Format.LeftIndent = 20;
                para.Format.RightIndent = 20;
                
                if (i == 0)
                {
                    para.Format.SpaceBefore = 8;
                }
                if (i == lines.Length - 1)
                {
                    para.Format.SpaceAfter = 8;
                }

                para.Format.Font.Name = "Consolas";
                para.Format.Font.Size = 9;
                para.Format.Font.Color = Colors.DarkBlue;

                AddSyntaxHighlight(para, line, language);
            }
        }

        private string DetectLanguage(HtmlNode? codeNode)
        {
            if (codeNode?.Attributes["class"] == null) return "plaintext";

            var classAttr = codeNode.Attributes["class"].Value;
            if (classAttr.Contains("csharp") || classAttr.Contains("language-cs") || classAttr.Contains("cs"))
                return "csharp";
            if (classAttr.Contains("python") || classAttr.Contains("py"))
                return "python";
            if (classAttr.Contains("javascript") || classAttr.Contains("js"))
                return "javascript";
            if (classAttr.Contains("java"))
                return "java";
            if (classAttr.Contains("cpp") || classAttr.Contains("c++"))
                return "cpp";
            if (classAttr.Contains("go"))
                return "go";
            if (classAttr.Contains("rust"))
                return "rust";
            if (classAttr.Contains("php"))
                return "php";
            if (classAttr.Contains("ruby"))
                return "ruby";
            if (classAttr.Contains("swift"))
                return "swift";
            if (classAttr.Contains("kotlin"))
                return "kotlin";
            if (classAttr.Contains("sql"))
                return "sql";
            if (classAttr.Contains("bash") || classAttr.Contains("sh") || classAttr.Contains("shell"))
                return "bash";
            if (classAttr.Contains("json"))
                return "json";
            if (classAttr.Contains("xml") || classAttr.Contains("html"))
                return "xml";
            if (classAttr.Contains("css"))
                return "css";
            if (classAttr.Contains("yaml"))
                return "yaml";
            if (classAttr.Contains("markdown") || classAttr.Contains("md"))
                return "markdown";

            return "plaintext";
        }

        private void AddSyntaxHighlight(Paragraph para, string line, string language)
        {
            if (string.IsNullOrEmpty(line))
            {
                para.AddText(" ");
                return;
            }

            var keywords = language switch
            {
                "csharp" => _csharpKeywords,
                "python" => _pythonKeywords,
                "javascript" => _jsKeywords,
                _ => Array.Empty<(string, string)>()
            };

            if (keywords.Length == 0 || language == "plaintext")
            {
                para.AddText(line);
                return;
            }

            var pattern = string.Join("|", keywords.Select(k => k.Item1));
            pattern = "(" + pattern + ")|((//|#).*)";

            try
            {
                var regex = new Regex(pattern, RegexOptions.IgnoreCase);
                var lastEnd = 0;

                foreach (Match match in regex.Matches(line))
                {
                    if (match.Index > lastEnd)
                    {
                        var beforeText = line.Substring(lastEnd, match.Index - lastEnd);
                        if (!string.IsNullOrWhiteSpace(beforeText))
                        {
                            para.AddText(beforeText);
                        }
                    }

                    var isComment = match.Value.StartsWith("//") || match.Value.StartsWith("#");
                    var formatted = para.AddFormattedText(match.Value);

                    if (isComment)
                    {
                        formatted.Font.Color = Colors.ForestGreen;
                        formatted.Font.Italic = true;
                    }
                    else if (IsKeyword(match.Value, keywords))
                    {
                        formatted.Font.Color = Colors.Blue;
                        formatted.Font.Bold = true;
                    }
                    else
                    {
                        formatted.Font.Color = Colors.Black;
                    }

                    lastEnd = match.Index + match.Length;
                }

                if (lastEnd < line.Length)
                {
                    para.AddText(line.Substring(lastEnd));
                }
            }
            catch
            {
                para.AddText(line);
            }
        }

        private bool IsKeyword(string word, (string pattern, string color)[] keywords)
        {
            foreach (var (pattern, _) in keywords)
            {
                if (Regex.IsMatch(word, pattern, RegexOptions.IgnoreCase))
                    return true;
            }
            return false;
        }

        private void AddBlockQuote(HtmlNode node)
        {
            var text = node.InnerText.Trim();
            var para = _section!.AddParagraph();
            para.Format.LeftIndent = 25;
            para.Format.RightIndent = 25;
            para.Format.SpaceBefore = 8;
            para.Format.SpaceAfter = 8;
            para.Format.Font.Italic = true;
            para.Format.Font.Size = 10;
            para.Format.Font.Color = Colors.DarkSlateGray;
            para.Format.Borders.Left.Width = 3;
            para.Format.Borders.Left.Color = Colors.Gray;
            para.AddText(text);
        }

        private void AddList(HtmlNode node, bool ordered)
        {
            var items = node.SelectNodes(".//li");
            if (items == null) return;

            int num = 1;
            foreach (var item in items)
            {
                var para = _section!.AddParagraph();
                para.Format.LeftIndent = 20;
                para.Format.FirstLineIndent = -15;
                para.Format.SpaceBefore = 3;
                para.Format.SpaceAfter = 2;

                var inputCheck = item.SelectSingleNode(".//input[@type='checkbox']");
                if (inputCheck != null)
                {
                    var isChecked = inputCheck.Attributes["checked"] != null;
                    para.AddText(isChecked ? "[x] " : "[ ] ");
                }
                else if (ordered)
                {
                    para.AddText($"{num}. ");
                    num++;
                }
                else
                {
                    para.AddText("• ");
                }

                ProcessInlineElements(para, item);
            }
        }

        private void AddTable(HtmlNode tableNode)
        {
            var headerRow = tableNode.SelectSingleNode(".//thead/tr");
            if (headerRow != null)
                AddTableHeader(headerRow);

            var bodyRows = tableNode.SelectNodes(".//tbody/tr");
            if (bodyRows == null)
                bodyRows = tableNode.SelectNodes(".//tr");

            if (bodyRows != null)
            {
                int rowIndex = 0;
                foreach (var row in bodyRows)
                {
                    if (headerRow != null && row == headerRow) continue;
                    AddTableRow(row, rowIndex % 2 == 1);
                    rowIndex++;
                }
            }
        }

        private void AddTableHeader(HtmlNode row)
        {
            var cells = row.SelectNodes("./th|./td");
            if (cells == null) return;

            var para = _section!.AddParagraph();
            para.Format.SpaceBefore = 5;
            para.Format.SpaceAfter = 2;
            para.Format.Font.Bold = true;
            para.Format.Font.Size = 9;

            for (int i = 0; i < cells.Count; i++)
            {
                var cellText = HtmlEntity(cells[i].InnerText.Trim());
                para.AddText(cellText);
                if (i < cells.Count - 1)
                    para.AddText(" | ");
            }
        }

        private void AddTableRow(HtmlNode row, bool alternate)
        {
            var cells = row.SelectNodes("./td|./th");
            if (cells == null) return;

            var para = _section!.AddParagraph();
            para.Format.SpaceBefore = 1;
            para.Format.SpaceAfter = 1;
            para.Format.Font.Size = 9;

            for (int i = 0; i < cells.Count; i++)
            {
                var cellText = HtmlEntity(cells[i].InnerText.Trim());
                para.AddText(cellText);
                if (i < cells.Count - 1)
                    para.AddText(" | ");
            }
        }

        private void AddDefinitionList(HtmlNode node)
        {
            var terms = node.SelectNodes("./dt");
            var definitions = node.SelectNodes("./dd");

            if (terms == null) return;

            for (int i = 0; i < terms.Count; i++)
            {
                var termPara = _section!.AddParagraph();
                termPara.Format.Font.Bold = true;
                termPara.Format.SpaceBefore = 5;
                termPara.Format.SpaceAfter = 2;
                termPara.AddText(HtmlEntity(terms[i].InnerText.Trim()));

                if (definitions != null && i < definitions.Count)
                {
                    var defPara = _section!.AddParagraph();
                    defPara.Format.LeftIndent = 15;
                    defPara.Format.SpaceAfter = 5;
                    ProcessInlineElements(defPara, definitions[i]);
                }
            }
        }

        private void AddHorizontalLine()
        {
            var para = _section!.AddParagraph();
            para.Format.SpaceBefore = 10;
            para.Format.SpaceAfter = 10;
            para.Format.Borders.Top.Width = 0.5;
            para.Format.Borders.Top.Color = Colors.LightGray;
        }

        private void AddFigure(HtmlNode node)
        {
            var img = node.SelectSingleNode(".//img");
            if (img != null)
            {
                var para = _section!.AddParagraph();
                para.Format.Alignment = ParagraphAlignment.Center;
                para.Format.SpaceBefore = 10;
                para.Format.SpaceAfter = 10;
                para.AddText($"[图片: {img.Attributes["alt"]?.Value ?? "Image"}]");
            }
            var caption = node.SelectSingleNode(".//figcaption");
            if (caption != null)
            {
                var para = _section!.AddParagraph();
                para.Format.Alignment = ParagraphAlignment.Center;
                para.Format.Font.Italic = true;
                para.Format.Font.Size = 9;
                para.AddText(caption.InnerText.Trim());
            }
        }

        private void AddDetails(HtmlNode node)
        {
            var summary = node.SelectSingleNode(".//summary");
            var content = node.SelectSingleNode(".//p");

            if (summary != null)
            {
                var para = _section!.AddParagraph();
                para.Format.Font.Bold = true;
                para.Format.SpaceBefore = 5;
                para.AddText("▸ " + summary.InnerText.Trim());
            }

            if (content != null)
            {
                var para = _section!.AddParagraph();
                para.Format.LeftIndent = 15;
                ProcessInlineElements(para, content);
            }
        }

        private void AddAbbreviation(HtmlNode node)
        {
            var title = node.Attributes["title"]?.Value;
            var text = node.InnerText.Trim();
            var para = _section!.AddParagraph();
            
            var formatted = para.AddFormattedText(text);
            formatted.Font.Bold = true;
            
            if (!string.IsNullOrEmpty(title))
            {
                para.AddText($" ({title})");
            }
        }

        private void AddAddress(HtmlNode node)
        {
            var para = _section!.AddParagraph();
            para.Format.Font.Italic = true;
            para.Format.SpaceBefore = 5;
            para.Format.SpaceAfter = 5;
            ProcessInlineElements(para, node);
        }

        private void AddCitation(HtmlNode node)
        {
            var para = _section!.AddParagraph();
            para.Format.Font.Italic = true;
            var cite = para.AddFormattedText(node.InnerText.Trim());
        }

        private void AddSample(HtmlNode node)
        {
            var para = _section!.AddParagraph();
            var samp = para.AddFormattedText(node.InnerText.Trim());
            samp.Font.Name = "Consolas";
            samp.Font.Size = 9;
        }

        private void AddKeyboard(HtmlNode node)
        {
            var para = _section!.AddParagraph();
            var kbd = para.AddFormattedText(node.InnerText.Trim());
            kbd.Font.Name = "Consolas";
            kbd.Font.Size = 9;
        }

        private void AddVariable(HtmlNode node)
        {
            var para = _section!.AddParagraph();
            var variable = para.AddFormattedText(node.InnerText.Trim());
            variable.Font.Italic = true;
            variable.Font.Bold = true;
        }

        private void AddTime(HtmlNode node)
        {
            var datetime = node.Attributes["datetime"]?.Value;
            var para = _section!.AddParagraph();
            
            if (!string.IsNullOrEmpty(datetime))
            {
                para.AddText($"[{datetime}] ");
            }
            para.AddText(node.InnerText.Trim());
        }

        private void AddMarked(HtmlNode node)
        {
            var para = _section!.AddParagraph();
            var marked = para.AddFormattedText(node.InnerText.Trim());
            marked.Font.Color = Colors.Yellow;
        }

        private void AddRuby(HtmlNode node)
        {
            var text = node.SelectSingleNode("./rb")?.InnerText ?? node.InnerText;
            var rt = node.SelectSingleNode("./rt")?.InnerText;

            var para = _section!.AddParagraph();
            para.AddText(text);

            if (!string.IsNullOrEmpty(rt))
            {
                para.AddText($" ({rt})");
            }
        }

        private void AddWordBreak(HtmlNode node)
        {
            var para = _section!.AddParagraph();
            para.AddText("");
        }

        private void AddInput(HtmlNode node)
        {
            if (node.Attributes["type"]?.Value == "checkbox")
            {
                var isChecked = node.Attributes["checked"] != null;
                var para = _section!.AddParagraph();
                para.AddText(isChecked ? "[x]" : "[ ]");
            }
        }

        private void AddProgress(HtmlNode node)
        {
            var value = node.Attributes["value"]?.Value ?? "0";
            var max = node.Attributes["max"]?.Value ?? "100";
            var para = _section!.AddParagraph();
            para.AddText($"[{value}/{max}]");
        }

        private void AddMeter(HtmlNode node)
        {
            var value = node.Attributes["value"]?.Value ?? "0";
            var para = _section!.AddParagraph();
            para.AddText($"[{value}]");
        }

        private void AddMedia(HtmlNode node)
        {
            var para = _section!.AddParagraph();
            para.Format.Alignment = ParagraphAlignment.Center;
            var src = node.Attributes["src"]?.Value;
            if (!string.IsNullOrEmpty(src))
            {
                para.AddText($"[{node.Name.ToUpper()}: {Path.GetFileName(src)}]");
            }
        }

        private void AddIframe(HtmlNode node)
        {
            var src = node.Attributes["src"]?.Value;
            var para = _section!.AddParagraph();
            para.Format.Alignment = ParagraphAlignment.Center;
            if (!string.IsNullOrEmpty(src))
            {
                para.AddText($"[嵌入内容: {src}]");
            }
        }

        private void ProcessChildNodes(HtmlNode node)
        {
            foreach (var child in node.ChildNodes)
            {
                if (child.NodeType == HtmlNodeType.Element)
                    ProcessElement(child);
            }
        }

        private string HtmlEntity(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            return text.Replace("&nbsp;", " ").Replace("&lt;", "<").Replace("&gt;", ">")
                      .Replace("&amp;", "&").Replace("&quot;", "\"").Replace("&#39;", "'")
                      .Replace("&copy;", "©").Replace("&reg;", "®").Replace("&trade;", "™")
                      .Replace("&ndash;", "–").Replace("&mdash;", "—").Replace("&hellip;", "…");
        }
    }

    public class CustomFontResolver : IFontResolver
    {
        public string DefaultFontName => "SimSun";

        public byte[] GetFont(string faceName)
        {
            var fontPath = Path.Combine(AppContext.BaseDirectory, "Resources", "Fonts", "SimSun.ttf");
            if (!File.Exists(fontPath))
                throw new FileNotFoundException($"字体文件未找到: {fontPath}");
            return File.ReadAllBytes(fontPath);
        }

        public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            if (familyName.Equals("SimSun", StringComparison.OrdinalIgnoreCase))
                return new FontResolverInfo("SimSun");
            if (familyName.Equals("Consolas", StringComparison.OrdinalIgnoreCase))
                return new FontResolverInfo("Consolas");
            return null;
        }
    }
}