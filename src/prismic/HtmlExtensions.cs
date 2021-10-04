namespace prismic
{
    public static class HtmlExtensions
    {
        public static string Link(string url, string content, string target = null, string title = null)
        {
            var targetAttr = string.Empty;
            if (!string.IsNullOrWhiteSpace(target))
                targetAttr = $" target=\"{target}\"  rel=\"noopener\"";

            var titleAttr = string.Empty;
            if(!string.IsNullOrWhiteSpace(title))
                titleAttr = $" title=\"{title}\"";

            return $"<a href=\"{url}\"{targetAttr}{titleAttr}>{content}</a>";
        }
    }
}
