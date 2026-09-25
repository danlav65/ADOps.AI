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

        var blocks = new List<MicrosoftLearnContentBlock>();

        AppendBlocks(article, blocks);

        var articleText = string.Join(
            "\n",
            blocks.Select(block => block.Content));

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
            RetrievedUtc = document.RetrievedUtc,
            Blocks = blocks
        };
    }

    private static void AppendBlocks(
        IElement parent,
        List<MicrosoftLearnContentBlock> blocks)
    {
        foreach (var child in parent.Children)
        {
            var tag = child.LocalName;

            if (tag == "pre")
            {
                var code = ExtractCode(child);

                AddBlock(
                    blocks,
                    MicrosoftLearnContentBlockType.Code,
                    code);

                continue;
            }

            if (tag is "h1" or "h2" or "h3" or
                "h4" or "h5" or "h6")
            {
                AddBlock(
                    blocks,
                    MicrosoftLearnContentBlockType.Heading,
                    Normalize(child.TextContent));

                continue;
            }

            if (tag == "p")
            {
                AddBlock(
                    blocks,
                    MicrosoftLearnContentBlockType.Paragraph,
                    Normalize(child.TextContent));

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
                AddBlock(
                    blocks,
                    MicrosoftLearnContentBlockType.Paragraph,
                    Normalize(child.TextContent));
            }
        }
    }

    private static void AppendTable(
        IElement table,
        List<MicrosoftLearnContentBlock> blocks)
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

            AddBlock(
                blocks,
                MicrosoftLearnContentBlockType.TableRow,
                string.Join(" | ", cells));
        }
    }

    private static void AppendList(
        IElement list,
        List<MicrosoftLearnContentBlock> blocks,
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

            // Capture the item's own text without flattening
            // nested lists or code blocks into it.
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
                AddBlock(
                    blocks,
                    MicrosoftLearnContentBlockType.ListItem,
                    indent + prefix + ownText);
            }

            foreach (var child in item.Children)
            {
                if (child.LocalName is "ol" or "ul")
                {
                    AppendList(child, blocks, depth + 1);
                }
                else if (child.LocalName == "pre")
                {
                    AppendIndentedCode(
                        child,
                        blocks,
                        depth + 1);
                }
            }

            index++;
        }
    }

    private static void AppendIndentedCode(
        IElement pre,
        List<MicrosoftLearnContentBlock> blocks,
        int depth)
    {
        var code = ExtractCode(pre);

        if (string.IsNullOrWhiteSpace(code))
        {
            return;
        }

        var indent = new string(' ', depth * 2);

        var indentedCode = string.Join(
            "\n",
            code.Split('\n')
                .Select(line => indent + line));

        AddBlock(
            blocks,
            MicrosoftLearnContentBlockType.Code,
            indentedCode);
    }

    private static string ExtractCode(IElement pre)
    {
        return pre.TextContent
            .Replace("\r\n", "\n")
            .Replace('\r', '\n')
            .Trim('\n');
    }

    private static void AddBlock(
        List<MicrosoftLearnContentBlock> blocks,
        MicrosoftLearnContentBlockType type,
        string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return;
        }

        blocks.Add(new MicrosoftLearnContentBlock
        {
            Type = type,
            Content = content,
            Sequence = blocks.Count
        });
    }

    private static string Normalize(string value)
    {
        return Regex.Replace(
            value,
            @"\s+",
            " ").Trim();
    }
}