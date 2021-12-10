namespace Rack.Shared.Help
{
    public sealed class HelpPage
    {
        public string Header { get; }
        public string Content { get; }
        public string ModuleName { get; }
        public string Language { get; }

        public HelpPage(string header, string content, string moduleName, string language)
        {
            Header = header;
            Content = content;
            ModuleName = moduleName;
            Language = language;
        }
    }
}