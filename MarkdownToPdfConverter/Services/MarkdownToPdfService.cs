using MigraDocCore.DocumentObjectModel;
using MigraDocCore.Rendering;
using PdfSharpCore.Fonts;
using System;
using System.IO;
using CommonMark;
using HtmlAgilityPack;
using System.Xml;

namespace MarkdownToPdfConverter.Services
{
    /// <summary>
    /// 提供将Markdown内容转换为PDF文件的服务
    /// </summary>
    public class MarkdownToPdfService
    {
        /// <summary>
        /// 将Markdown内容转换为PDF并写入指定的流
        /// </summary>
        /// <param name="markdownContent">要转换的Markdown内容</param>
        /// <param name="outputStream">PDF文件的输出流</param>
        /// <exception cref="Exception">转换过程中发生错误时抛出</exception>
        public void ConvertMarkdownToPdf(string markdownContent, Stream outputStream)
        {
            try
            {
                // 注册自定义字体解析器
                GlobalFontSettings.FontResolver = new CustomFontResolver();

                // 创建新文档
                var document = new Document();
                var style = document.Styles["Normal"];
                style.Font.Name = "SimSun"; // 使用支持中文的字体
                style.Font.Size = 12;

                // 将Markdown转换为HTML
                var htmlContent = CommonMarkConverter.Convert(markdownContent);

                // 添加内容到文档
                var section = document.AddSection();

                // 解析HTML并将其添加到PDF
                ParseHtmlToPdf(htmlContent, section);

                // 渲染 PDF 文件
                var renderer = new PdfDocumentRenderer(true);
                renderer.Document = document;
                renderer.RenderDocument();

                // 保存为 PDF 文件到流
                renderer.PdfDocument.Save(outputStream);
            }
            catch (Exception ex)
            {
                throw new Exception($"转换失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 解析HTML内容并将其转化为MigraDoc段落
        /// </summary>
        /// <param name="htmlContent">要解析的HTML内容</param>
        /// <param name="section">要添加内容的PDF文档部分</param>
        private void ParseHtmlToPdf(string htmlContent, Section section)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);

            ProcessNodes(htmlDoc.DocumentNode, section);
        }

        /// <summary>
        /// 递归处理HTML节点
        /// </summary>
        private void ProcessNodes(HtmlNode node, Section section, Paragraph currentParagraph = null)
        {
            foreach (var childNode in node.ChildNodes)
            {
                switch (childNode.NodeType)
                {
                    case HtmlNodeType.Element:
                        ProcessHtmlElement(childNode, section, ref currentParagraph);
                        break;

                    case HtmlNodeType.Text:
                        ProcessTextNode(childNode, section, ref currentParagraph);
                        break;
                }
            }
        }

        /// <summary>
        /// 处理HTML元素节点
        /// </summary>
        private void ProcessHtmlElement(HtmlNode node, Section section, ref Paragraph currentParagraph)
        {
            // 如果当前没有段落，创建一个新的
            if (currentParagraph == null)
            {
                currentParagraph = section.AddParagraph();
            }

            switch (node.Name.ToLower())
            {
                case "h1":
                case "h2":
                case "h3":
                case "h4":
                case "h5":
                case "h6":
                    // 标题单独成段
                    currentParagraph = section.AddParagraph();
                    int level = int.Parse(node.Name.Substring(1));
                    currentParagraph.Format.Font.Bold = true;
                    currentParagraph.Format.Font.Size = 16 - (level * 2);
                    ProcessNodes(node, section, currentParagraph);
                    currentParagraph = null; // 标题后重置段落
                    break;

                case "p":
                    // 段落
                    currentParagraph = section.AddParagraph();
                    ProcessNodes(node, section, currentParagraph);
                    currentParagraph = null; // 段落结束后重置
                    break;

                case "strong":
                case "b":
                    // 加粗文本
                    currentParagraph.Format.Font.Bold = true;
                    ProcessNodes(node, section, currentParagraph);
                    currentParagraph.Format.Font.Bold = false;
                    break;

                case "em":
                case "i":
                    // 斜体文本
                    currentParagraph.Format.Font.Italic = true;
                    ProcessNodes(node, section, currentParagraph);
                    currentParagraph.Format.Font.Italic = false;
                    break;

                case "blockquote":
                    // 引用
                    currentParagraph = section.AddParagraph();
                    currentParagraph.Format.LeftIndent = 10;
                    currentParagraph.Format.Font.Italic = true;
                    ProcessNodes(node, section, currentParagraph);
                    currentParagraph = null;
                    break;

                case "ul":
                case "ol":
                    // 列表处理
                    ProcessList(node, section);
                    break;

                case "li":
                    // 列表项
                    currentParagraph = section.AddParagraph();
                    currentParagraph.Format.LeftIndent = 10;
                    currentParagraph.AddText("• "); // 无序列表符号
                    ProcessNodes(node, section, currentParagraph);
                    currentParagraph = null;
                    break;

                case "br":
                    // 换行
                    currentParagraph.AddLineBreak();
                    break;

                default:
                    // 默认处理
                    ProcessNodes(node, section, currentParagraph);
                    break;
            }
        }

        /// <summary>
        /// 处理文本节点
        /// </summary>
        private void ProcessTextNode(HtmlNode node, Section section, ref Paragraph currentParagraph)
        {
            string text = node.InnerText.Trim();
            if (!string.IsNullOrEmpty(text))
            {
                if (currentParagraph == null)
                {
                    currentParagraph = section.AddParagraph();
                }
                currentParagraph.AddText(text);
            }
        }

        /// <summary>
        /// 处理列表
        /// </summary>
        private void ProcessList(HtmlNode listNode, Section section)
        {
            bool isOrdered = listNode.Name.ToLower() == "ol";
            int itemNumber = 1;

            foreach (var liNode in listNode.SelectNodes("./li"))
            {
                var paragraph = section.AddParagraph();
                paragraph.Format.LeftIndent = 10;

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
        }
    }

    /// <summary>
    /// 自定义字体解析器，用于加载自定义字体文件
    /// </summary>
    public class CustomFontResolver : IFontResolver
    {
        /// <summary>
        /// 获取默认字体名称
        /// </summary>
        public string DefaultFontName => "SimSun";

        /// <summary>
        /// 根据字体名称获取字体文件的字节数组
        /// </summary>
        /// <param name="faceName">字体名称</param>
        /// <returns>字体文件的字节数组</returns>
        /// <exception cref="FileNotFoundException">字体文件未找到时抛出</exception>
        public byte[] GetFont(string faceName)
        {
            // 获取项目根目录
            var baseDirectory = AppContext.BaseDirectory;

            // 拼接字体文件路径
            var fontPath = Path.Combine(baseDirectory, "Resources", "Fonts", "SimSun.ttf");

            // 检查路径是否存在
            if (!File.Exists(fontPath))
            {
                throw new FileNotFoundException($"字体文件未找到: {fontPath}");
            }

            // 加载字体文件
            return File.ReadAllBytes(fontPath);
        }

        /// <summary>
        /// 解析字体类型信息
        /// </summary>
        /// <param name="familyName">字体家族名称</param>
        /// <param name="isBold">是否为粗体</param>
        /// <param name="isItalic">是否为斜体</param>
        /// <returns>字体解析信息</returns>
        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            if (familyName.Equals("SimSun", StringComparison.OrdinalIgnoreCase))
                return new FontResolverInfo("SimSun");
            return null;
        }
    }
}
