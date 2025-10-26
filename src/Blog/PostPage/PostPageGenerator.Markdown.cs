using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Markdig;
using OwlCore.Storage;
using YamlDotNet.Serialization;

namespace WindowsAppCommunity.Blog.PostPage
{
    /// <summary>
    /// Markdown processing operations for PostPageGenerator.
    /// Handles front-matter extraction, markdown transformation, and YAML parsing.
    /// </summary>
    public partial class PostPageGenerator
    {
        /// <summary>
        /// Extract YAML front-matter block from markdown file.
        /// Front-matter is delimited by "---" at start and end.
        /// Handles files without front-matter (returns empty string for frontmatter).
        /// </summary>
        /// <param name="file">Markdown file to parse</param>
        /// <returns>Tuple of (frontmatter YAML string, content markdown string)</returns>
        private async Task<(string frontmatter, string content)> ParseMarkdownAsync(IFile file)
        {
            var text = await file.ReadTextAsync();
            
            // Gap #12 resolution: Check for front-matter delimiters
            if (!text.StartsWith("---"))
            {
                // No front-matter present
                return (string.Empty, text);
            }

            // Find the closing delimiter
            var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.None);
            var closingDelimiterIndex = -1;
            
            for (int i = 1; i < lines.Length; i++)
            {
                if (lines[i].Trim() == "---")
                {
                    closingDelimiterIndex = i;
                    break;
                }
            }

            if (closingDelimiterIndex == -1)
            {
                // No closing delimiter found - treat entire file as content
                return (string.Empty, text);
            }

            // Extract front-matter (lines between delimiters)
            var frontmatterLines = lines.Skip(1).Take(closingDelimiterIndex - 1);
            var frontmatter = string.Join(Environment.NewLine, frontmatterLines);

            // Extract content (everything after closing delimiter)
            var contentLines = lines.Skip(closingDelimiterIndex + 1);
            var content = string.Join(Environment.NewLine, contentLines);

            return (frontmatter, content);
        }

        /// <summary>
        /// Transform markdown content to HTML body using Markdig.
        /// Returns HTML without wrapping elements - template controls structure.
        /// Uses Advanced Extensions pipeline for full Markdown feature support.
        /// </summary>
        /// <param name="markdown">Markdown content string</param>
        /// <returns>HTML body content</returns>
        private string TransformMarkdownToHtml(string markdown)
        {
            // Gap #1 resolution: Use Markdig Advanced Extensions pipeline
            var pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .Build();

            return Markdown.ToHtml(markdown, pipeline);
        }

        /// <summary>
        /// Parse YAML front-matter string to arbitrary dictionary.
        /// No schema enforcement - accepts any valid YAML structure.
        /// Handles empty/missing front-matter gracefully.
        /// </summary>
        /// <param name="yaml">YAML string from front-matter</param>
        /// <returns>Dictionary with arbitrary keys and values</returns>
        private Dictionary<string, object> ParseFrontmatter(string yaml)
        {
            // Handle empty front-matter
            if (string.IsNullOrWhiteSpace(yaml))
            {
                return new Dictionary<string, object>();
            }

            // Gap #2 resolution: YamlDotNet with error handling
            try
            {
                var deserializer = new DeserializerBuilder()
                    .Build();

                var result = deserializer.Deserialize<Dictionary<string, object>>(yaml);
                return result ?? new Dictionary<string, object>();
            }
            catch (YamlDotNet.Core.YamlException ex)
            {
                // Gap #4 resolution: Exception-based error handling (no try-catch in caller)
                throw new InvalidOperationException($"Failed to parse YAML front-matter: {ex.Message}", ex);
            }
        }
    }
}
