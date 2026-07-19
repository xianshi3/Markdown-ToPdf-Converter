using PdfSharpCore;
using PdfSharpCore.Drawing;
using PdfSharpCore.Fonts;
using PdfSharpCore.Pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Markdig;
using HtmlAgilityPack;
using System.Net;

namespace MarkdownToPdfConverter.Services
{
    /// <summary>
    /// Converts Markdown content to PDF using PdfSharpCore and Markdig.
    /// </summary>
    public class MarkdownToPdfService
    {
        private MarkdownPipeline _pipeline;

        /// <summary>
        /// Points-per-millimeter conversion factor (1 mm = 2.83465 pt).
        /// </summary>
        private const double PtPerMm = 2.83465;

        /// <summary>
        /// Static constructor: registers the custom font resolver once.
        /// </summary>
        static MarkdownToPdfService()
        {
            GlobalFontSettings.FontResolver = new CustomFontResolver();
        }

        /// <summary>
        /// Initializes the Markdig pipeline with all advanced extensions enabled.
        /// </summary>
        public MarkdownToPdfService()
        {
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

        private string _currentPageSize = "A4";

        /// <summary>
        /// Converts the given Markdown content to PDF and writes it to the output stream.
        /// </summary>
        /// <param name="markdownContent">Raw Markdown text.</param>
        /// <param name="outputStream">Stream to write the resulting PDF into.</param>
        /// <param name="pageSize">Page size name (e.g. "A4", "Letter").</param>
        /// <param name="pageMargin">Page margin in millimeters.</param>
        /// <param name="exportFont">Font family name to use for text rendering.</param>
        public void ConvertMarkdownToPdf(string markdownContent, Stream outputStream,
            string pageSize = "A4", double pageMargin = 20, string exportFont = "SimSun")
        {
            try
            {
                _currentPageSize = pageSize;

                string html = Markdown.ToHtml(markdownContent, _pipeline);

                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(html);

                var pdf = new PdfDocument();
                var page = pdf.AddPage();
                SetPageSize(page, pageSize);

                double marginPt = pageMargin * PtPerMm;
                var gfx = XGraphics.FromPdfPage(page);
                var layout = new LayoutState(gfx, pdf, page, exportFont, marginPt);

                RenderContent(layout, htmlDoc.DocumentNode);

                pdf.Save(outputStream);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"PDF conversion failed: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Sets the PdfPage size based on the given page size name.
        /// </summary>
        private void SetPageSize(PdfPage page, string pageSize)
        {
            switch (pageSize.ToLower())
            {
                case "a3": page.Size = PageSize.A3; break;
                case "a4": page.Size = PageSize.A4; break;
                case "a5": page.Size = PageSize.A5; break;
                case "letter": page.Size = PageSize.Letter; break;
                case "legal": page.Size = PageSize.Legal; break;
            }
        }

        /// <summary>
        /// Measures the line height for a font using a mixed Latin/CJK sample string.
        /// </summary>
        private static double MeasureLineHeight(XGraphics gfx, XFont font)
        {
            return gfx.MeasureString("A\u4e00g", font).Height;
        }

        /// <summary>
        /// Returns recommended line spacing (line height * 1.35).
        /// </summary>
        private static double LineSpacing(double lineH)
        {
            return lineH * 1.35;
        }

        /// <summary>
        /// Iterates child HTML nodes and renders each element.
        /// </summary>
        private void RenderContent(LayoutState layout, HtmlNode node)
        {
            foreach (var child in node.ChildNodes)
            {
                if (child.NodeType == HtmlNodeType.Element)
                    RenderElement(layout, child);
            }
        }

        /// <summary>
        /// Dispatches an HTML element to the appropriate render method by tag name.
        /// </summary>
        private void RenderElement(LayoutState layout, HtmlNode node)
        {
            switch (node.Name.ToLower())
            {
                case "h1": RenderHeading(layout, node, 20); break;
                case "h2": RenderHeading(layout, node, 16); break;
                case "h3": RenderHeading(layout, node, 14); break;
                case "h4": RenderHeading(layout, node, 12); break;
                case "h5": RenderHeading(layout, node, 11); break;
                case "h6": RenderHeading(layout, node, 10); break;
                case "p": RenderParagraph(layout, node); break;
                case "pre": RenderCodeBlock(layout, node); break;
                case "blockquote": RenderBlockQuote(layout, node); break;
                case "ul": RenderList(layout, node, false); break;
                case "ol": RenderList(layout, node, true); break;
                case "hr": RenderHorizontalRule(layout); break;
                case "table": RenderTable(layout, node); break;
                case "dl": RenderDefinitionList(layout, node); break;
                default: RenderContent(layout, node); break;
            }
        }

        /// <summary>
        /// Adds a new page if the remaining vertical space is insufficient for the needed height.
        /// </summary>
        private void EnsurePage(LayoutState layout, double needed)
        {
            if (layout.Y + needed > layout.PageHeight - layout.Margin)
            {
                var page = layout.Pdf.AddPage();
                SetPageSize(page, _currentPageSize);
                layout.Gfx = XGraphics.FromPdfPage(page);
                layout.Y = layout.Margin;
            }
        }

        /// <summary>
        /// Available content width between left and right margins.
        /// </summary>
        private double ContentWidth(LayoutState layout) =>
            layout.PageWidth - layout.Margin * 2;

        /// <summary>
        /// Renders a heading (h1-h6) with bold font at the specified size.
        /// </summary>
        private void RenderHeading(LayoutState layout, HtmlNode node, double size)
        {
            var text = WebUtility.HtmlDecode(node.InnerText.Trim());
            if (string.IsNullOrEmpty(text)) return;

            var font = new XFont(layout.FontName, size, XFontStyle.Bold);
            double lineH = MeasureLineHeight(layout.Gfx, font);

            layout.Y += LineSpacing(lineH) * 1.2;
            EnsurePage(layout, lineH + 6);

            layout.Gfx.DrawString(text, font, XBrushes.Black,
                new XRect(layout.Margin, layout.Y, ContentWidth(layout), lineH),
                XStringFormats.TopLeft);
            layout.Y += lineH + 4;
        }

        /// <summary>
        /// Renders a paragraph with word-wrapping and automatic page breaks.
        /// </summary>
        private void RenderParagraph(LayoutState layout, HtmlNode node)
        {
            var font = new XFont(layout.FontName, 10, XFontStyle.Regular);
            double lineH = MeasureLineHeight(layout.Gfx, font);
            double lineSpacing = LineSpacing(lineH);

            EnsurePage(layout, lineSpacing);
            layout.Y += 4;

            RenderInlineContent(layout, node, 10, lineH, lineSpacing);

            layout.Y += lineSpacing * 0.4;
        }

        /// <summary>
        /// Renders inline text and elements (bold, italic, code, links, etc.) with word-wrapping.
        /// </summary>
        private void RenderInlineContent(LayoutState layout, HtmlNode node,
            double fontSize, double lineH, double lineSpacing)
        {
            double x = layout.Margin;
            double maxX = layout.PageWidth - layout.Margin;
            double startY = layout.Y;

            foreach (var child in node.ChildNodes)
            {
                if (child.NodeType == HtmlNodeType.Text)
                {
                    var raw = WebUtility.HtmlDecode(child.InnerText);
                    if (string.IsNullOrEmpty(raw)) continue;

                    var lines = raw.Split('\n');
                    for (int li = 0; li < lines.Length; li++)
                    {
                        if (li > 0)
                        {
                            layout.Y += lineSpacing;
                            x = layout.Margin;
                            EnsurePage(layout, lineSpacing);
                        }

                        var text = lines[li];
                        if (string.IsNullOrEmpty(text)) continue;

                        var font = new XFont(layout.FontName, fontSize, XFontStyle.Regular);
                        var words = text.Split(' ');
                        for (int wi = 0; wi < words.Length; wi++)
                        {
                            var word = words[wi];
                            if (wi > 0) word = " " + word;

                            var sz = layout.Gfx.MeasureString(word, font);
                            if (x + sz.Width > maxX)
                            {
                                layout.Y += lineSpacing;
                                x = layout.Margin;
                                EnsurePage(layout, lineSpacing);
                            }
                            layout.Gfx.DrawString(word, font, XBrushes.Black,
                                new XRect(x, layout.Y, sz.Width, sz.Height),
                                XStringFormats.TopLeft);
                            x += sz.Width;
                        }
                    }
                }
                else if (child.NodeType == HtmlNodeType.Element)
                {
                    RenderInlineElement(layout, child, ref x, fontSize, maxX, lineH, lineSpacing);
                }
            }
        }

        /// <summary>
        /// Renders a single inline element (strong, em, code, a, del, etc.) at the current cursor position.
        /// </summary>
        private void RenderInlineElement(LayoutState layout, HtmlNode node,
            ref double x, double fontSize, double maxX, double lineH, double lineSpacing)
        {
            var text = WebUtility.HtmlDecode(node.InnerText);
            if (string.IsNullOrEmpty(text)) return;

            XFontStyle style = XFontStyle.Regular;
            XBrush brush = XBrushes.Black;
            string fontName = layout.FontName;
            double size = fontSize;

            switch (node.Name.ToLower())
            {
                case "strong":
                case "b":
                    style = XFontStyle.Bold;
                    break;
                case "em":
                case "i":
                    style = XFontStyle.Italic;
                    break;
                case "code":
                    fontName = CustomFontResolver.DefaultCodeFont;
                    size = fontSize - 1;
                    break;
                case "a":
                    brush = XBrushes.Blue;
                    break;
                case "del":
                    brush = XBrushes.Gray;
                    break;
                case "mark":
                    brush = XBrushes.Yellow;
                    break;
                case "small":
                    size = fontSize - 2;
                    break;
                case "sup":
                case "sub":
                    size = fontSize - 3;
                    break;
                default:
                    RenderInlineContent(layout, node, fontSize, lineH, lineSpacing);
                    return;
            }

            var font = new XFont(fontName, size, style);
            var measure = layout.Gfx.MeasureString(text, font);
            double textH = measure.Height;

            if (x + measure.Width > maxX)
            {
                layout.Y += lineSpacing;
                x = layout.Margin;
                EnsurePage(layout, lineSpacing);
            }

            if (node.Name.ToLower() == "code")
            {
                layout.Gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(235, 235, 235)),
                    x - 1, layout.Y + 1, measure.Width + 2, textH);
            }

            layout.Gfx.DrawString(text, font, brush,
                new XRect(x, layout.Y, measure.Width, textH),
                XStringFormats.TopLeft);
            x += measure.Width;
        }

        /// <summary>
        /// Renders a code block with a gray background, border, and optional language label.
        /// </summary>
        private void RenderCodeBlock(LayoutState layout, HtmlNode node)
        {
            var codeNode = node.SelectSingleNode("code");
            var code = codeNode?.InnerText ?? node.InnerText;
            code = code.TrimEnd('\r', '\n', ' ');
            if (string.IsNullOrEmpty(code)) return;

            string language = DetectLanguage(codeNode);
            var lines = code.Replace("\r\n", "\n").Split('\n');

            var codeFont = new XFont(CustomFontResolver.DefaultCodeFont, 8, XFontStyle.Regular);
            double lineH = MeasureLineHeight(layout.Gfx, codeFont);
            double lineSpacing = LineSpacing(lineH);
            double padding = 8;

            double totalH = lines.Length * lineSpacing + padding * 2;
            EnsurePage(layout, totalH + 6);

            layout.Y += 4;

            double bgX = layout.Margin;
            double bgY = layout.Y;
            double bgW = ContentWidth(layout);
            double bgH = lines.Length * lineSpacing + padding * 2;

            var bgBrush = new XSolidBrush(XColor.FromArgb(245, 245, 245));
            var borderPen = new XPen(XColor.FromArgb(220, 220, 220), 0.5);

            double codeX = layout.Margin + padding;
            double codeY = layout.Y + padding;
            double maxTextW = ContentWidth(layout) - padding * 2;

            layout.Gfx.DrawRectangle(bgBrush, bgX, bgY, bgW, bgH);
            layout.Gfx.DrawRectangle(borderPen, bgX, bgY, bgW, bgH);

            foreach (var line in lines)
            {
                var displayLine = line;
                layout.Gfx.DrawString(displayLine, codeFont, XBrushes.Black,
                    new XRect(codeX, codeY, maxTextW, lineH),
                    XStringFormats.TopLeft);
                codeY += lineSpacing;
            }

            layout.Y = bgY + bgH + 4;

            if (!string.IsNullOrEmpty(language) && language != "plaintext")
            {
                var langFont = new XFont(layout.FontName, 7, XFontStyle.Italic);
                layout.Gfx.DrawString(language, langFont, XBrushes.Gray,
                    new XRect(layout.Margin + padding + 2, layout.Y, 100, 10),
                    XStringFormats.TopLeft);
                layout.Y += 12;
            }

            layout.Y += 4;
        }

        /// <summary>
        /// Renders a blockquote with an italic font and a vertical bar on the left.
        /// </summary>
        private void RenderBlockQuote(LayoutState layout, HtmlNode node)
        {
            var font = new XFont(layout.FontName, 10, XFontStyle.Italic);
            double lineH = MeasureLineHeight(layout.Gfx, font);
            double lineSpacing = LineSpacing(lineH);

            EnsurePage(layout, lineSpacing);
            layout.Y += 4;

            double barX = layout.Margin;
            double barW = 3;
            double textX = layout.Margin + 14;
            double maxTextW = ContentWidth(layout) - 14;

            double startY = layout.Y;
            double contentY = layout.Y;

            foreach (var child in node.ChildNodes)
            {
                if (child.NodeType == HtmlNodeType.Text)
                {
                    var raw = WebUtility.HtmlDecode(child.InnerText);
                    if (string.IsNullOrEmpty(raw)) continue;

                    var paragraphs = raw.Split('\n');
                    foreach (var para in paragraphs)
                    {
                        if (string.IsNullOrEmpty(para.Trim()))
                        {
                            contentY += lineSpacing;
                            continue;
                        }

                        var words = para.Split(' ');
                        double xp = textX;
                        for (int wi = 0; wi < words.Length; wi++)
                        {
                            var word = wi > 0 ? " " + words[wi] : words[wi];
                            var sz = layout.Gfx.MeasureString(word, font);
                            if (xp + sz.Width > maxTextW && xp > textX)
                            {
                                contentY += lineSpacing;
                                xp = textX;
                                EnsurePage(layout, lineSpacing);
                            }
                            layout.Gfx.DrawString(word, font, XBrushes.DarkSlateGray,
                                new XRect(xp, contentY, sz.Width, sz.Height),
                                XStringFormats.TopLeft);
                            xp += sz.Width;
                        }
                        contentY += lineSpacing;
                    }
                }
                else if (child.NodeType == HtmlNodeType.Element)
                {
                    RenderInlineElement(layout, child, ref textX, 10, maxTextW, lineH, lineSpacing);
                }
            }

            double barHeight = contentY - startY;
            if (barHeight < lineSpacing) barHeight = lineSpacing;

            layout.Gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(210, 210, 210)),
                barX, startY, barW, barHeight);

            layout.Y = contentY + 4;
        }

        /// <summary>
        /// Renders an ordered or unordered list with appropriate bullet or numbering.
        /// </summary>
        private void RenderList(LayoutState layout, HtmlNode node, bool ordered)
        {
            var items = node.SelectNodes("./li");
            if (items == null) return;

            var font = new XFont(layout.FontName, 10, XFontStyle.Regular);
            double lineH = MeasureLineHeight(layout.Gfx, font);
            double lineSpacing = LineSpacing(lineH);

            int num = 1;
            foreach (var item in items)
            {
                var text = WebUtility.HtmlDecode(item.InnerText.Trim());
                if (string.IsNullOrEmpty(text)) continue;

                var prefix = ordered ? $"{num}. " : "\u2022 ";
                EnsurePage(layout, lineSpacing + 4);

                double px = layout.Margin + 10;
                layout.Gfx.DrawString(prefix, font, XBrushes.Black,
                    new XRect(px, layout.Y + 2, 25, lineH),
                    XStringFormats.TopLeft);

                double textX = layout.Margin + 30;
                double maxTextW = ContentWidth(layout) - 30;
                double xp = textX;
                double yp = layout.Y + 2;

                var words = text.Split(' ');
                for (int wi = 0; wi < words.Length; wi++)
                {
                    var word = wi > 0 ? " " + words[wi] : words[wi];
                    var sz = layout.Gfx.MeasureString(word, font);
                    if (xp + sz.Width > maxTextW && xp > textX)
                    {
                        yp += lineSpacing;
                        xp = textX;
                        EnsurePage(layout, lineSpacing);
                    }
                    layout.Gfx.DrawString(word, font, XBrushes.Black,
                        new XRect(xp, yp, sz.Width, sz.Height),
                        XStringFormats.TopLeft);
                    xp += sz.Width;
                }

                layout.Y = yp + lineSpacing + 2;
                if (ordered) num++;
            }
        }

        /// <summary>
        /// Renders a horizontal rule as a thin line across the content width.
        /// </summary>
        private void RenderHorizontalRule(LayoutState layout)
        {
            EnsurePage(layout, 4);
            layout.Y += 10;
            layout.Gfx.DrawLine(new XPen(XColor.FromArgb(200, 200, 200), 1),
                layout.Margin, layout.Y,
                layout.PageWidth - layout.Margin, layout.Y);
            layout.Y += 10;
        }

        private static readonly string[] _languages = new[]
        {
            "csharp", "cs", "python", "py", "javascript", "js", "java",
            "cpp", "c++", "go", "rust", "php", "ruby", "swift", "kotlin",
            "sql", "bash", "sh", "shell", "json", "xml", "html", "css", "yaml"
        };

        /// <summary>
        /// Detects the programming language from the code node's CSS class attribute.
        /// </summary>
        private string DetectLanguage(HtmlNode? codeNode)
        {
            if (codeNode?.Attributes["class"] == null)
                return "plaintext";
            var cls = codeNode.Attributes["class"].Value;
            foreach (var lang in _languages)
            {
                if (cls.Contains(lang)) return lang;
            }
            if (cls.Contains("language-"))
            {
                var idx = cls.IndexOf("language-") + 9;
                var end = cls.IndexOf(' ', idx);
                return end > idx ? cls[idx..end] : cls[idx..];
            }
            return "plaintext";
        }

        /// <summary>
        /// Renders an HTML table with alternating row colors and a header row.
        /// </summary>
        private void RenderTable(LayoutState layout, HtmlNode tableNode)
        {
            var rows = tableNode.SelectNodes(".//tr");
            if (rows == null || rows.Count == 0) return;

            int cols = rows.Max(r => r.SelectNodes("./th|./td")?.Count ?? 0);
            if (cols == 0) return;

            var font = new XFont(layout.FontName, 9, XFontStyle.Regular);
            double lineH = MeasureLineHeight(layout.Gfx, font);
            double rowHeight = lineH + 8;
            double cellPad = 4;

            double totalH = rows.Count * rowHeight + 8;
            EnsurePage(layout, totalH);

            double tableY = layout.Y + 4;
            double cellW = ContentWidth(layout) / cols;

            for (int ri = 0; ri < rows.Count; ri++)
            {
                var cells = rows[ri].SelectNodes("./th|./td");
                if (cells == null) continue;

                bool isHeader = ri == 0;
                var rowFont = new XFont(layout.FontName, 9, isHeader ? XFontStyle.Bold : XFontStyle.Regular);
                double xp = layout.Margin;

                for (int ci = 0; ci < cells.Count && ci < cols; ci++)
                {
                    var text = WebUtility.HtmlDecode(cells[ci].InnerText.Trim());
                    var bg = isHeader
                        ? new XSolidBrush(XColor.FromArgb(230, 230, 230))
                        : (ri % 2 == 0
                            ? XBrushes.White
                            : new XSolidBrush(XColor.FromArgb(248, 248, 248)));

                    layout.Gfx.DrawRectangle(bg, xp, tableY, cellW, rowHeight);
                    layout.Gfx.DrawRectangle(XPens.LightGray, xp, tableY, cellW, rowHeight);
                    layout.Gfx.DrawString(text, rowFont, XBrushes.Black,
                        new XRect(xp + cellPad, tableY + 3, cellW - cellPad * 2, rowHeight - 6),
                        XStringFormats.TopLeft);
                    xp += cellW;
                }
                tableY += rowHeight;
            }

            layout.Y = tableY + 6;
        }

        /// <summary>
        /// Renders a definition list with bold terms and indented definitions.
        /// </summary>
        private void RenderDefinitionList(LayoutState layout, HtmlNode node)
        {
            var terms = node.SelectNodes("./dt");
            var defs = node.SelectNodes("./dd");
            if (terms == null) return;

            var font = new XFont(layout.FontName, 10, XFontStyle.Regular);
            double lineH = MeasureLineHeight(layout.Gfx, font);
            double lineSpacing = LineSpacing(lineH);

            for (int i = 0; i < terms.Count; i++)
            {
                var termText = WebUtility.HtmlDecode(terms[i].InnerText.Trim());
                if (string.IsNullOrEmpty(termText)) continue;

                var boldFont = new XFont(layout.FontName, 10, XFontStyle.Bold);
                EnsurePage(layout, lineSpacing);
                layout.Gfx.DrawString(termText, boldFont, XBrushes.Black,
                    new XRect(layout.Margin, layout.Y, ContentWidth(layout), lineH),
                    XStringFormats.TopLeft);
                layout.Y += lineSpacing;

                if (defs != null && i < defs.Count)
                {
                    var defText = WebUtility.HtmlDecode(defs[i].InnerText.Trim());
                    if (string.IsNullOrEmpty(defText)) continue;

                    EnsurePage(layout, lineSpacing);
                    layout.Gfx.DrawString(defText, font, XBrushes.DimGray,
                        new XRect(layout.Margin + 15, layout.Y, ContentWidth(layout) - 15, lineH),
                        XStringFormats.TopLeft);
                    layout.Y += lineSpacing;
                }
            }
        }

        /// <summary>
        /// Tracks the current rendering position, graphics context, font, and page dimensions.
        /// </summary>
        private class LayoutState
        {
            public XGraphics Gfx { get; set; }
            public PdfDocument Pdf { get; }
            public PdfPage Page { get; set; }
            public double Y { get; set; }
            public string FontName { get; }
            public double Margin { get; }

            public double PageWidth => Page.Width.Point;
            public double PageHeight => Page.Height.Point;

            public LayoutState(XGraphics gfx, PdfDocument pdf, PdfPage page,
                string fontName, double marginPt)
            {
                Gfx = gfx;
                Pdf = pdf;
                Page = page;
                FontName = fontName;
                Margin = marginPt;
                Y = marginPt;
            }
        }
    }

    /// <summary>
    /// Provides font resolution for PdfSharpCore, using a bundled SimSun font file.
    /// </summary>
    public class CustomFontResolver : IFontResolver
    {
        public const string DefaultCodeFont = "SimSun";

        private static byte[]? _simSunCache;
        private static readonly object _lock = new();

        private static string FontPath =>
            Path.Combine(AppContext.BaseDirectory, "Resources", "Fonts", "SimSun.ttf");

        public string DefaultFontName => DefaultCodeFont;

        /// <summary>
        /// Always resolves to "SimSun" regardless of the requested family.
        /// </summary>
        public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            return new FontResolverInfo("SimSun", isBold, isItalic);
        }

        /// <summary>
        /// Reads and caches the SimSun font bytes from the Resources/Fonts directory.
        /// </summary>
        public byte[] GetFont(string faceName)
        {
            // Return cached bytes if already loaded.
            if (_simSunCache != null) return _simSunCache;

            var fontPath = FontPath;
            if (!File.Exists(fontPath))
                throw new FileNotFoundException(
                    $"SimSun font not found at: {fontPath}. " +
                    $"Base directory: {AppContext.BaseDirectory}. " +
                    "Ensure the font is copied to the output directory (Resources/Fonts/SimSun.ttf).");

            // Thread-safe lazy loading of the font file.
            lock (_lock)
            {
                _simSunCache ??= File.ReadAllBytes(fontPath);
            }

            return _simSunCache;
        }

    }
}
