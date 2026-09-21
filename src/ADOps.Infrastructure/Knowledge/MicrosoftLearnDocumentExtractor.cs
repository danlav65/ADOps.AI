using AngleSharp.Html.Parser;
using AngleSharp.Dom;
using System.Text.RegularExpressions;

namespace ADOps.Infrastructure.Knowledge;

/// <summary>
/// Extracts article content from fetched Microsoft Learn HTML.
/// Does not verify the accuracy of the extracted guidance.
/// </summary>
public sealed class MicrosoftLearnDocumentExtractor
    : IMicrosoftLearnDocumentExtractor
{
    public MicrosoftLearnExtractedDocument Extract(
        MicrosoftLearnDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var parser = new HtmlParser();
        var html = parser.ParseDocument(document.Content);

        var article =
            html.QuerySelector("main article")
            ?? html.QuerySelector("article")
            ?? html.QuerySelector("main");

        if (article is null)
        {
            throw new InvalidOperationException(
                "Microsoft Learn document contains no article content.");
        }

        // Remove elements that are not article guidance.
        foreach (var element in article.QuerySelectorAll(
            "script, style, nav, footer, aside, " +
            "button, form, noscript, " +
            "[aria-hidden='true'], [hidden]"))
        {
            element.Remove();
        }

        var title = Normalize(
            article.QuerySelector("h1")?.TextContent
            ?? html.QuerySelector("title")?.TextContent
            ?? string.Empty);

        var headings = article
            .QuerySelectorAll("h1, h2, h3, h4, h5, h6")
            .Select(element => Normalize(element.TextContent))
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .ToArray();

        var articleText = ExtractArticleText(article);

        if (string.IsNullOrWhiteSpace(articleText))
        {
            throw new InvalidOperationException(
                "Microsoft Learn document contains no usable article text.");
        }

        return new MicrosoftLearnExtractedDocument
        {
            SourceUri = document.SourceUri,
            Title = title,
            ArticleText = articleText,
            Headings = headings,
            RetrievedUtc = document.RetrievedUtc
        };
    }

    private static string ExtractArticleText(IElement article)
    {
        var blocks = new List<string>();

        AppendBlocks(article, blocks);

        return string.Join("\n", blocks);
    }

    private static void AppendBlocks(
        IElement parent,
        List<string> blocks)
    {
        foreach (var child in parent.Children)
        {
            var tag = child.LocalName;

            if (tag == "pre")
            {
                var code = child.TextContent
                    .Replace("\r\n", "\n")
                    .Replace('\r', '\n')
                    .Trim('\n');

                if (!string.IsNullOrWhiteSpace(code))
                {
                    blocks.Add(code);
                }

                continue;
            }

            if (tag is "h1" or "h2" or "h3" or
                "h4" or "h5" or "h6" or "p")
            {
                var text = Normalize(child.TextContent);

                if (!string.IsNullOrWhiteSpace(text))
                {
                    blocks.Add(text);
                }

                continue;
            }

            if (tag is "ol" or "ul")
            {
                AppendList(child, blocks);
                continue;
            }

            if (tag == "table")
            {
                AppendTable(child, blocks);
                continue;
            }

            if (child.Children.Length > 0)
            {
                AppendBlocks(child, blocks);
            }
            else
            {
                var text = Normalize(child.TextContent);

                if (!string.IsNullOrWhiteSpace(text))
                {
                    blocks.Add(text);
                }
            }
        }
    }

    private static void AppendTable(
        IElement table,
        List<string> blocks)
    {
        foreach (var row in table.QuerySelectorAll("tr"))
        {
            var cells = row.Children
                .Where(element =>
                    element.LocalName is "th" or "td")
                .Select(element => Normalize(element.TextContent))
                .ToArray();

            if (cells.Length == 0)
            {
                continue;
            }

            blocks.Add(string.Join(" | ", cells));
        }
    }
    
    private static void AppendList(
        IElement list,
        List<string> blocks,
        int depth = 0)
    {
        var ordered = list.LocalName == "ol";
        var index = 1;
        var indent = new string(' ', depth * 2);

        foreach (var item in list.Children
            .Where(element => element.LocalName == "li"))
        {
            var prefix = ordered
                ? $"{index}. "
                : "- ";

            // Capture the item's own text without flattening nested lists
            // or code blocks into it.
            var ownText = Normalize(string.Join(
                " ",
                item.ChildNodes
                    .Where(node => node is not IElement)
                    .Select(node => node.TextContent)));

            var paragraph = item.Children
                .FirstOrDefault(element => element.LocalName == "p");

            if (paragraph is not null)
            {
                ownText = Normalize(
                    $"{ownText} {paragraph.TextContent}");
            }

            if (!string.IsNullOrWhiteSpace(ownText))
            {
                blocks.Add(indent + prefix + ownText);
            }

            foreach (var child in item.Children)
            {
                if (child.LocalName is "ol" or "ul")
                {
                    AppendList(child, blocks, depth + 1);
                }
                else if (child.LocalName == "pre")
                {
                    AppendIndentedCode(child, blocks, depth + 1);
                }
        }

        index++;
        }
    }

    private static void AppendIndentedCode(
        IElement pre,
        List<string> blocks,
        int depth)
    {
        var code = pre.TextContent
            .Replace("\r\n", "\n")
            .Replace('\r', '\n')
            .Trim('\n');

        if (string.IsNullOrWhiteSpace(code))
        {
            return;
        }

        var indent = new string(' ', depth * 2);

        foreach (var line in code.Split('\n'))
        {
            blocks.Add(indent + line);
        }
    }
    
    private static string Normalize(string value)
    {
        return Regex.Replace(
            value,
            @"\s+",
            " ").Trim();
    }
}