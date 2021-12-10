using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Rack.LocalizationTool.Models.LocalizationProblem
{
    /// <summary>
    /// Файл, содержащий нелокализованные строки.
    /// </summary>
    public sealed class FileWithUnlocalizedStrings
    {
        public FileWithUnlocalizedStrings(string path, IEnumerable<IUnlocalizedString> unlocalizedStrings)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException();
            if (unlocalizedStrings == null)
                throw new ArgumentNullException(nameof(unlocalizedStrings));

            Path = path;
            Name = System.IO.Path.GetFileName(path);
            Extension = System.IO.Path.GetExtension(path);
            UnlocalizedStrings = unlocalizedStrings.ToArray();

            if (UnlocalizedStrings.Count == 0) throw new ArgumentException();
        }

        /// <summary>
        /// Путь к файлу.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Имя файла.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Расширение файла.
        /// </summary>
        public string Extension { get; }

        /// <summary>
        /// Нелокализованные строки в файле.
        /// </summary>
        public IReadOnlyCollection<IUnlocalizedString> UnlocalizedStrings { get; }
    }
}