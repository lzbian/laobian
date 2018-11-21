using System;
using Markdig;

namespace Laobian.Common.Base
{
    /// <summary>
    /// Converter for Markdown
    /// </summary>
    public class MarkdownConverter
    {
        /// <summary>
        /// Convert markdown to HTML
        /// </summary>
        /// <param name="md">The given markdown string</param>
        /// <param name="trimP">Remove 'p' tag at the beginning and ending</param>
        /// <returns>Converted HTML string</returns>
        public static string ToHtml(string md, bool trimP = false)
        {
            var html = Markdown.ToHtml(md).Trim(Environment.NewLine.ToCharArray());
            if (trimP)
            {
                if (html.StartsWith("<p>"))
                {
                    html = html.Remove(0, 3);
                }
                
                if(html.EndsWith("</p>"))
                {
                    html = html.Remove(html.Length - 4);
                }
            }

            return html;
        }
    }
}
