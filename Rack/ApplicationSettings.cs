using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Rack.Shared.Configuration;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Rack
{
    [DataContract]
    public sealed class ApplicationSettings : ReactiveObject, IConfiguration
    {
        public enum FontModeIdentifier
        {
            Regular,
            Big
        }

        /// <summary>
        /// Режим отображения шрифтов.
        /// </summary>
        public sealed class FontMode
        {
            public static FontMode Regular { get; } = new FontMode(
                16, 
                14,
                16,
                14,
                12,
                FontModeIdentifier.Regular);

            public static FontMode Big { get; } = new FontMode(
                18, 
                16,
                18,
                16,
                14,
                FontModeIdentifier.Big);

            public static IEnumerable<FontMode> GetAll() =>
                new[] {Regular, Big};

            public static FontMode Get(FontModeIdentifier identifier) =>
                GetAll().First(x => x.Identifier == identifier);

            private FontMode(
                double body1FontSize, 
                double body2FontSize,
                double subtitle1FontSize,
                double subtitle2FontSize,
                double toolTipSize,
                FontModeIdentifier identifier)
            {
                Body1FontSize = body1FontSize;
                Body2FontSize = body2FontSize;
                ToolTipSize = toolTipSize;
                Identifier = identifier;
                Subtitle1FontSize = subtitle1FontSize;
                Subtitle2FontSize = subtitle2FontSize;
            }

            /// <summary>
            /// Размер шрифтов основного текста.
            /// </summary>
            public double Body1FontSize { get; }

            public double Body2FontSize { get; }

            public double Subtitle1FontSize { get; }

            public double Subtitle2FontSize { get; }

            /// <summary>
            /// Размер шрифтов подсказок.
            /// </summary>
            public double ToolTipSize { get; }

            /// <summary>
            /// Идентификатор режима, используемый при сериализации и десериализации.
            /// </summary>
            public FontModeIdentifier Identifier { get; }
        }

        /// <inheritdoc />
        public ApplicationSettings()
        {
            _mode = this.WhenAnyValue(
                    x => x.ModeIdentifier,
                    FontMode.Get)
                .ToProperty(this, nameof(Mode));
        }

        [Reactive, DataMember] public FontModeIdentifier ModeIdentifier { get; set; } 
            = FontModeIdentifier.Regular;

        private readonly ObservableAsPropertyHelper<FontMode> _mode;
        public FontMode Mode => _mode.Value;

        [Reactive, DataMember] public string Language { get; set; } = "Русский";

        [Reactive, DataMember] public string Username { get; set; } = string.Empty;

        [DataMember] public Version Version { get; } = new Version(1, 1);

        public void Map(ApplicationSettings settings)
        {
            ModeIdentifier = settings.ModeIdentifier;
            Language = settings.Language;
            Username = settings.Username;
        }

        public ApplicationSettings Clone() => new ApplicationSettings
        {
            ModeIdentifier = ModeIdentifier,
            Language = Language,
            Username = Username
        };
    }
}