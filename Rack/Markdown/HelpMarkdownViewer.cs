using System.Windows;

namespace Rack.Markdown
{
    public sealed class MarkdownViewer: Markdig.Wpf.MarkdownViewer
    {
        /// <summary>
        /// Путь к изображениям справки.
        /// </summary>
        public string UCRootPath
        {
            get => (string)GetValue(UCRootPathProperty);
            set => SetValue(UCRootPathProperty, value);
        }

        public static readonly DependencyProperty UCRootPathProperty =
            DependencyProperty.Register(
                nameof(UCRootPath), 
                typeof(string), 
                typeof(MarkdownViewer), 
                new PropertyMetadata(string.Empty));


        protected override void RefreshDocument()
        {
            GetBindingExpression(UCRootPathProperty)?.UpdateTarget();

            var path = UCRootPath;
            if (!string.IsNullOrEmpty(path) && !path.EndsWith("/"))
                path = path.Remove(path.LastIndexOf('/') + 1);
            Document = Markdown != null 
                ? Markdig.Wpf.Markdown.ToFlowDocument(Markdown, 
                    Pipeline ?? DefaultPipeline, 
                    new WpfRenderer(path)) 
                : null;
        }
    }
}