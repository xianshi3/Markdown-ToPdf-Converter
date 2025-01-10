using MigraDocCore.DocumentObjectModel;
using MigraDocCore.Rendering;
using PdfSharpCore.Fonts;
using System;
using System.IO;
using CommonMark;

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
                ParseMarkdownToPdf(htmlContent, section);

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
        /// 解析Markdown的HTML内容并将其转化为MigraDoc段落
        /// </summary>
        /// <param name="htmlContent">要解析的HTML内容</param>
        /// <param name="section">要添加内容的PDF文档部分</param>
        private void ParseMarkdownToPdf(string htmlContent, Section section)
        {
            // 对于更复杂的Markdown渲染，后面考虑使用更复杂的HTML解析器或更高级的MigraDoc操作
            // 目前处理简单的Markdown样式（例如标题、加粗、斜体）
            var lines = htmlContent.Split(new[] { "\n", "\r\n" }, StringSplitOptions.None);

            foreach (var line in lines)
            {
                // 识别并处理标题
                if (line.StartsWith("<h1>"))
                {
                    var paragraph = section.AddParagraph(line.Replace("<h1>", "").Replace("</h1>", ""));
                    paragraph.Format.Font.Bold = true;
                    paragraph.Format.Font.Size = 16;
                }
                else if (line.StartsWith("<h2>"))
                {
                    var paragraph = section.AddParagraph(line.Replace("<h2>", "").Replace("</h2>", ""));
                    paragraph.Format.Font.Bold = true;
                    paragraph.Format.Font.Size = 14;
                }
                else if (line.StartsWith("<h3>"))
                {
                    var paragraph = section.AddParagraph(line.Replace("<h3>", "").Replace("</h3>", ""));
                    paragraph.Format.Font.Bold = true;
                    paragraph.Format.Font.Size = 12;
                }
                // 处理加粗文本
                else if (line.Contains("<strong>"))
                {
                    var paragraph = section.AddParagraph(line.Replace("<strong>", "").Replace("</strong>", ""));
                    paragraph.Format.Font.Bold = true;
                }
                // 处理斜体文本
                else if (line.Contains("<em>"))
                {
                    var paragraph = section.AddParagraph(line.Replace("<em>", "").Replace("</em>", ""));
                    paragraph.Format.Font.Italic = true;
                }
                // 处理引用
                else if (line.StartsWith("<blockquote>"))
                {
                    var paragraph = section.AddParagraph(line.Replace("<blockquote>", "").Replace("</blockquote>", ""));
                    paragraph.Format.LeftIndent = 10;
                    paragraph.Format.Font.Italic = true;
                }
                else
                {
                    // 普通段落
                    section.AddParagraph(line);
                }
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
