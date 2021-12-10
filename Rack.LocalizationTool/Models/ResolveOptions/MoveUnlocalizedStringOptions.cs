using Rack.LocalizationTool.Models.LocalizationData;
using Rack.LocalizationTool.Models.LocalizationProblem;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Rack.LocalizationTool.Models.ResolveOptions
{
    /// <summary>
    /// Параметры перемещения нелокализованных строк в файл локализации.
    /// </summary>
    public sealed class MoveUnlocalizedStringOptions : ReactiveObject
    {
        public MoveUnlocalizedStringOptions(FileWithUnlocalizedStrings fileWithUnlocalizedString, IUnlocalizedString unlocalizedString)
        {
            FileWithUnlocalizedString = fileWithUnlocalizedString;
            UnlocalizedString = unlocalizedString;
        }

        public FileWithUnlocalizedStrings FileWithUnlocalizedString { get; }

        /// <summary>
        /// Строка, которую необходимо переместить.
        /// </summary>
        public IUnlocalizedString UnlocalizedString { get; }

        /// <summary>
        /// Файл локализации.
        /// </summary>
        [Reactive] public LocalizationFile LocalizationFile { get; set; }

        /// <summary>
        /// Ключ фразы.
        /// </summary>
        [Reactive] public string Key { get; set; }
    }
}