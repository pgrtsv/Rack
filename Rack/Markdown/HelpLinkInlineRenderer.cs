using Markdig.Syntax.Inlines;

namespace Rack.Markdown
{
    public sealed class LinkInlineRenderer: Markdig.Renderers.Wpf.Inlines.LinkInlineRenderer
    {
        private readonly string _linkpath;

        public LinkInlineRenderer(string linkpath)
        {
            _linkpath = linkpath;
        }

        protected override void Write(Markdig.Renderers.WpfRenderer renderer, LinkInline link)
        {
            if (link.IsImage)
                link.Url = _linkpath + link.Url;

            base.Write(renderer, link);
        }
    }
}