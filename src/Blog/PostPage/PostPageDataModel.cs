using System;
using System.Collections.Generic;

namespace WindowsAppCommunity.Blog.PostPage
{
    /// <summary>
    /// Data model for Scriban template rendering in Post/Page scenario.
    /// Provides the data contract that templates can access via dot notation.
    /// </summary>
    public class PostPageDataModel
    {
        /// <summary>
        /// Transformed HTML content from markdown body.
        /// Generated via Markdig pipeline, ready to insert into template.
        /// No wrapping elements - template controls structure.
        /// </summary>
        public string Body { get; set; } = string.Empty;

        /// <summary>
        /// Arbitrary key-value pairs from YAML front-matter.
        /// Keys are user-defined field names, values can be string, number, boolean, or structured data.
        /// No required keys, no filtering - entirely user-defined.
        /// Template accesses via frontmatter.key or frontmatter["key"] syntax.
        /// </summary>
        public Dictionary<string, object> Frontmatter { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Original markdown filename without path or extension.
        /// Useful for debugging, display, or conditional logic.
        /// Null if not available or not provided.
        /// </summary>
        public string? Filename { get; set; }

        /// <summary>
        /// File creation timestamp from filesystem metadata.
        /// May not be available on all platforms.
        /// Null if unavailable.
        /// </summary>
        public DateTime? Created { get; set; }

        /// <summary>
        /// File modification timestamp from filesystem metadata.
        /// More reliable than creation time.
        /// Null if unavailable.
        /// </summary>
        public DateTime? Modified { get; set; }
    }
}
