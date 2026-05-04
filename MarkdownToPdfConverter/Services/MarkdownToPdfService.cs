using MigraDocCore.DocumentObjectModel;
using MigraDocCore.Rendering;
using PdfSharpCore.Fonts;
using System;
using System.IO;
using System.Text.RegularExpressions;
using CommonMark;
using HtmlAgilityPack;

namespace MarkdownToPdfConverter.Services
{
    public class MarkdownToPdfService
    {
        public void ConvertMarkdownToPdf(string markdownContent, Stream outputStream)
        {
            try
            {
                GlobalFontSettings.FontResolver = new CustomFontResolver();

                var document = new Document();
                var style = document.Styles["Normal"];
                style.Font.Name = "SimSun";
                style.Font.Size = 12;

                var htmlContent = CommonMarkConverter.Convert(markdownContent);

                var section = document.AddSection();
                ParseHtmlToPdf(htmlContent, section);

                var renderer = new PdfDocumentRenderer(true);
                renderer.Document = document;
                renderer.RenderDocument();

                renderer.PdfDocument.Save(outputStream);
            }
            catch (Exception ex)
            {
                throw new Exception($"转换失败: {ex.Message}", ex);
            }
        }

        private void ParseHtmlToPdf(string htmlContent, Section section)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);
            ProcessNodes(htmlDoc.DocumentNode, section, null);
        }

        private void ProcessNodes(HtmlNode node, Section section, Paragraph? currentParagraph)
        {
            foreach (var childNode in node.ChildNodes)
            {
                switch (childNode.NodeType)
                {
                    case HtmlNodeType.Element:
                        currentParagraph = ProcessHtmlElement(childNode, section, currentParagraph);
                        break;
                    case HtmlNodeType.Text:
                        currentParagraph = ProcessTextNode(childNode, section, currentParagraph);
                        break;
                }
            }
        }

        private Paragraph? ProcessHtmlElement(HtmlNode node, Section section, Paragraph? currentParagraph)
        {
            string tagName = node.Name.ToLower();

            currentParagraph ??= section.AddParagraph();

            switch (tagName)
            {
                case "h1":
                case "h2":
                case "h3":
                case "h4":
                case "h5":
                case "h6":
                    currentParagraph = section.AddParagraph();
                    if (int.TryParse(node.Name.Substring(1), out int level))
                    {
                        currentParagraph.Format.Font.Bold = true;
                        currentParagraph.Format.Font.Size = 18 - (level * 2);
                        if (level <= 2)
                            currentParagraph.Format.Font.Bold = true;
                    }
                    ProcessNodes(node, section, currentParagraph);
                    return null;

                case "p":
                    currentParagraph = section.AddParagraph();
                    ProcessNodes(node, section, currentParagraph);
                    return null;

                case "strong":
                case "b":
                    currentParagraph.Format.Font.Bold = true;
                    ProcessNodes(node, section, currentParagraph);
                    currentParagraph.Format.Font.Bold = false;
                    break;

                case "em":
                case "i":
                    currentParagraph.Format.Font.Italic = true;
                    ProcessNodes(node, section, currentParagraph);
                    currentParagraph.Format.Font.Italic = false;
                    break;

                case "code":
                    currentParagraph.Format.Font.Name = "Consolas";
                    currentParagraph.Format.Font.Size = 10;
                    ProcessNodes(node, section, currentParagraph);
                    currentParagraph.Format.Font.Name = "SimSun";
                    currentParagraph.Format.Font.Size = 12;
                    break;

                case "pre":
                    currentParagraph = section.AddParagraph();
                    currentParagraph.Format.Font.Name = "Consolas";
                    currentParagraph.Format.Font.Size = 10;
                    currentParagraph.Format.Font.Color = Colors.DarkBlue;
                    currentParagraph.Format.LeftIndent = 15;
                    currentParagraph.Format.RightIndent = 15;
                    var codeContent = node.InnerText.Trim();
                    currentParagraph.AddText(codeContent);
                    return null;

                case "blockquote":
                    currentParagraph = section.AddParagraph();
                    currentParagraph.Format.LeftIndent = 20;
                    currentParagraph.Format.RightIndent = 20;
                    currentParagraph.Format.Font.Italic = true;
                    currentParagraph.Format.Font.Color = Colors.DarkGray;
                    ProcessNodes(node, section, currentParagraph);
                    return null;

                case "ul":
                case "ol":
                    currentParagraph = ProcessList(node, section);
                    return currentParagraph;

                case "li":
                    currentParagraph = section.AddParagraph();
                    currentParagraph.Format.LeftIndent = 15;
                    currentParagraph.AddText("• ");
                    ProcessNodes(node, section, currentParagraph);
                    return null;

                case "br":
                    currentParagraph.AddLineBreak();
                    break;

                case "hr":
                    currentParagraph = section.AddParagraph();
                    currentParagraph.Format.Borders.Top.Width = 1;
                    currentParagraph.Format.Borders.Color = Colors.Gray;
                    return null;

                case "a":
                    ProcessNodes(node, section, currentParagraph);
                    break;

                case "table":
                    currentParagraph = ProcessTable(node, section);
                    return currentParagraph;

                default:
                    ProcessNodes(node, section, currentParagraph);
                    break;
            }
            return currentParagraph;
        }

        private Paragraph? ProcessTextNode(HtmlNode node, Section section, Paragraph? currentParagraph)
        {
            string text = node.InnerText.Trim();
            if (!string.IsNullOrEmpty(text))
            {
                currentParagraph ??= section.AddParagraph();
                currentParagraph.AddText(text);
            }
            return currentParagraph;
        }

        private Paragraph ProcessList(HtmlNode listNode, Section section)
        {
            bool isOrdered = listNode.Name.ToLower() == "ol";
            int itemNumber = 1;
            Paragraph lastParagraph = section.AddParagraph();
            lastParagraph.Format.LeftIndent = 15;

            var liNodes = listNode.SelectNodes("./li");
            if (liNodes == null) return lastParagraph;

            foreach (var liNode in liNodes)
            {
                var paragraph = section.AddParagraph();
                paragraph.Format.LeftIndent = 15;

                if (isOrdered)
                {
                    paragraph.AddText($"{itemNumber}. ");
                    itemNumber++;
                }
                else
                {
                    paragraph.AddText("• ");
                }

                ProcessNodes(liNode, section, paragraph);
            }
            return lastParagraph;
        }

        private Paragraph ProcessTable(HtmlNode tableNode, Section section)
        {
            var rows = tableNode.SelectNodes(".//tr");
            if (rows == null)
                return section.AddParagraph();

            foreach (var row in rows)
            {
                var cells = row.SelectNodes("./th|./td");
                if (cells == null) continue;

                var paragraph = section.AddParagraph();
                paragraph.Format.LeftIndent = 15;

                for (int i = 0; i < cells.Count; i++)
                {
                    var cell = cells[i];
                    string cellText = cell.InnerText.Trim();

                    if (row.SelectNodes(".//th")?.Contains(cells[i]) == true || 
                        cell.ParentNode?.SelectNodes(".//th")?.Count > 0)
                    {
                        paragraph.Format.Font.Bold = true;
                    }

                    paragraph.AddText(cellText);
                    if (i < cells.Count - 1)
                        paragraph.AddText(" | ");
                }
            }
            return section.AddParagraph();
        }
    }

    public class CustomFontResolver : IFontResolver
    {
        public string DefaultFontName => "SimSun";

        public byte[] GetFont(string faceName)
        {
            var baseDirectory = AppContext.BaseDirectory;
            var fontPath = Path.Combine(baseDirectory, "Resources", "Fonts", "SimSun.ttf");

            if (!File.Exists(fontPath))
            {
                throw new FileNotFoundException($"字体文件未找到: {fontPath}");
            }

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