using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rack.Navigation
{
    public interface IDialogService
    {
        Task<IReadOnlyDictionary<string, object>> ShowAsync<TViewModel>(
            IReadOnlyDictionary<string, object> dialogParameters) 
            where TViewModel : class, IDialogViewModel;

        Task<IReadOnlyDictionary<string, object>> ShowDialogAsync<TViewModel>(
            IReadOnlyDictionary<string, object> dialogParameters) 
            where TViewModel : class, IDialogViewModel;

        Task ShowHelpAsync();

        Task ShowAppSettingsAsync();

        public enum YesNoAnswer
        {
            Cancel,
            No,
            Yes
        }

        public Task<YesNoAnswer> ShowYesNoQuestionDialogAsync(
            string title,
            string content);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dialogService"></param>
        /// <param name="title"></param>
        /// <param name="canSelectMultipleFiles"></param>
        /// <param name="filters"></param>
        /// <param name="callback">При <see cref="canSelectMultipleFiles"/>==true IDialogResult.DelegateParameters "Files": IEnumerable string, в противном случае "File" string. </param>
        public string ShowOpenFileDialog(
            string title,
            IDictionary<string, string> filters);

        public IEnumerable<string> ShowOpenFilesDialog(
            string title,
            IDictionary<string, string> filters);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dialogService"></param>
        /// <param name="title"></param>
        /// <param name="callback">IDialogResult.DelegateParameters содержит "Path": string.</param>
        public string ShowOpenDirectoryDialog(string title);

        /// <summary>
        /// Показывает модальное диалоговое окно для сохранения файла.
        /// Аргумент <paramref name="callback"/> IDialogResult.DelegateParameters содержит string "File".
        /// </summary>
        /// <param name="dialogService"></param>
        /// <param name="title"></param>
        /// <param name="defaultFileName"></param>
        /// <param name="filters"></param>
        /// <param name="callback">IDialogResult.DelegateParameters содержит string "File".</param>
        public string ShowSaveFileDialog(
            string title,
            string defaultFileName,
            IDictionary<string, string> filters);
    }
}