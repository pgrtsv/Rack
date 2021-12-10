﻿[TOC]

# Rack

## Что это такое?

Rack — это программный комплекс, разработанный для того, чтобы объединить и унифицировать технологии, инструменты и методы разработки программного обеспечения в лаборатории геологии нефти и газа. Rack поставляется пользователям как единая программа, однако разрабатывается как совокупность независимых (в общем случае) друг от друга частей, что, по замыслу, должно упростить процессы разработки и поддержки (для изменения какой-либо части/модуля вам не нужно читать, изменять, проверять и даже видеть какой-либо код, не относящийся к этой части напрямую).

Rack — это также программа-оболочка, являющаяся точкой входа в программу.

Rack использует программную платформу .NET Core и написан на языке C#. 

## Что должно быть установлено?

Убедитесь, что у вас установлены следующие компоненты:

- .NET Core 3.1 SDK;
- Visual Studio 2019 (любое издание). Также должны быть установлены инструменты для разработки на C#. **Крайне рекомендуется установить ReSharper от JetBrains;**
- *Для редактирования файлов Markdown и JSON, работы со внешними инструментами вроде ANTLR также рекомендуется установить Visual Studio Code.*

Помните о лицензировании программного обеспечения, которое используете!

## Что нужно знать?

Перед началом работы с комплексом разработчику рекомендуется хотя бы поверхностно ознакомиться с нижеприведёнными инструментальными средствами, подходами и технологиями.

Пользовательский интерфейс реализуется с помощью графической системы Windows Presentation Foundation (WPF).  
https://docs.microsoft.com/ru-ru/dotnet/framework/wpf/

Используется схема разделения данных Model-View-ViewModel (MVVM).  
https://ru.wikipedia.org/wiki/Model-View-ViewModel  

Программный комплекс строится с применением внедрения зависимостей (dependency injection). Используется контейнер внедрения зависимостей DryIoc. Рекомендуется ознакомиться с основными принципами внедрения зависимостей и часто используемыми конструкциями (Composition Root), способами внедрения зависимостей (внедрение в конструктор), временами жизни зависимостей (Transient, Singletone, Scoped).  
https://ru.wikipedia.org/wiki/Внедрение_зависимости  
https://github.com/dadhi/DryIoc  
Марк Симан, Внедрение зависимостей в .NET (книга)

Для работы с событиями рекомендуется использование реактивных расширений: библиотеки Reactive Extensions (Rx). Для работы с изменяющимися коллекциями рекомендуется использовать библиотеку DynamicData, разработанную на основе Rx. Для создания элементов UI (представлений) и вьюмоделей этих представлений используется фреймворк ReactiveUI, разработанный на основе Rx и DynamicData.  
https://reactivex.io/  
https://github.com/dotnet/reactive  
https://dynamic-data.org/  
https://github.com/reactiveui/DynamicData  
https://www.reactiveui.net/  
https://github.com/reactiveui/reactiveui

Взаимодействие с СУБД PostgreSQL осуществляется с использованием драйвера Npgsql.  
https://www.npgsql.org/

Обратите внимание, что в модулях приложения для взаимодействия с данными БД используется технология объектно-реляционного отображения (Object-relational mapping, ORM) с использованием фреймворка Entity Framework Core. Для написания эффективных SQL-запросов "руками", особенно в read-подсистемах, часто используется micro-ORM Dapper.  
https://docs.microsoft.com/ru-ru/ef/core/
https://github.com/StackExchange/Dapper  

Справочные материалы пишутся с помощью языка разметки Markdown. Рекомендуется использование Visual Studio Code с плагином Markdown All in One.  
https://ru.wikipedia.org/wiki/Markdown  
https://github.com/yzhang-gh/vscode-markdown

----------

Далее описаны менее важные библиотеки.

Для расширения возможностей (в том числе и эстетических) графического интерфейса используется библиотека MaterialDesignInXaml. Рекомендуется скачать последнюю версию демо-приложения с репозитория GitHub (раздел releases, архив DemoApp.zip) и использовать её в ходе знакомства с возможностями библиотеки.  
http://materialdesigninxaml.net/  
https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit/wiki  
https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit/releases

Для того, чтобы не писать много однотипных и многословных реализаций интерфейса INotifyPropertyChanged, используется средство посткомпиляции Fody и библиотека Fody.PropertyChanged.  
https://github.com/Fody/Fody  
https://github.com/Fody/PropertyChanged

Пример:
```csharp
public class ContainerMaterial : INotifyPropertyChanged
{
    public ContainerMaterial()
    {
    }

    public string Name { get; set; } // Вызов сеттера с новым значением автоматически вызовет событие PropertyChanged.
    public ObservableCollection<Sample> Samples { get; set; }

    public event PropertyChangedEventHandler PropertyChanged;
}
```

Для классов, наследующихся от ReactiveObject, используется библиотека ReactiveUI.Fody.  
https://github.com/kswoll/ReactiveUI.Fody  

Пример:
```csharp
public class SomeViewModel: ReactiveObject
{
    [Reactive] public string Text { get; set; } // Вызов сеттера у свойств, помеченных атрибутом [Reactive], автоматически вызовет this.RaiseAndSetIfChanged().
}
```

Для валидации вводимых пользователями данных используется библиотека FluentValidation. В Rack.Shared/FluentValidation/ создан класс ReactiveValidationTemplate, позволяющий валидировать объекты при заданных реактивных событиях.  
https://fluentvalidation.net/

Пример:
```csharp
public class ContainerMaterial : INotifyPropertyChanged, INotifyDataErrorInfo
{
    private readonly ReactiveValidationTemplate<ContainerMaterial> _validationTemplate;

    public ContainerMaterial()
    {
        _validationTemplate = new ReactiveValidationTemplate<ContainerMaterial>(
            new ContainerMaterialValidator(), 
            this.WhenAnyPropertyChanged()
                .Merge(Samples
                    .ObserveCollectionChanges()
                    .Select(_ => this)));
    }

    public string Name { get; set; }
    public ObservableCollection<Sample> Samples { get;  } = new ObservableCollection<Sample>();

    public IEnumerable GetErrors(string propertyName)
    {
        return _validator.GetErrors(propertyName);
    }

    public bool HasErrors => _validator.HasErrors;

    public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged
    {
        add => _validator.ErrorsChanged += value;
        remove => _validator.ErrorsChanged -= value;
    }

    public event PropertyChangedEventHandler PropertyChanged;
}
```
```csharp
    public class ContainerMaterialValidator: AbstractValidator<ContainerMaterial>
    {
        private readonly ILocalizationService _localizationService;

        private ILocalization Localization => _localizationService.GetLocalization("Localization.Key");

        public ContainerMaterialValidator(ILocalizationService localizationService)
        {
            _localization = LocalizationService.Instance.GetLocalization("Chemistry.UI");
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage(_ => Localization["InsertValidContainerMaterial"]);
            
            RuleFor(x => x.Samples)
                .NotEmpty()
                .WithMessage(_ => Localization["SamplesMustNotBeEmpty"]);
        }
    }
```

Для взаимодействия с пространственными данными используются библиотеки NetTopologySuite. Также рекомендуется использовать проекты Rack.GeoTools и Rack.GeoTools.Wpf.  
https://github.com/NetTopologySuite/NetTopologySuite  

---------

Оболочка содержит точку входа в приложение (App.xaml), файл конфигурации приложения (App.config), словарь ресурсов для стилизации пользовательского интерфейса (Override.xaml), основные элементы графического пользовательского интерфейса (Views\ и ViewModels\\), файлы локализации (Localizations\\) и файлы справки пользователя (HelpFiles\\).

Именно в загрузчике происходит конфигурация всех основных компонентов приложения (также используются метаданные из конфигурационного файла).

Большая часть переиспользуемого кода расположена в библиотеке Rack.Shared или в других, более узконаправленных библиотеках (например, Rack.GeoTools).

## Как создать модуль?

1\. Создайте директорию для разработки модуля. Новые модули именуются следующим образом: Rack.\*ИмяВашегоМодуля\*;  
2\. создайте в директории git-репозиторий (здесь и далее для работы с git используется SourceTree);  
3\. клонируйте репозиторий Rack в качестве подмодуля в поддиректорию /Shell (Репозиторий - Добавить подмодуль... - Исходный путь: https://Hatebearz@bitbucket.org/Hatebearz/rack.git, локальный относительный путь: Shell);  
4\. создайте поддиректорию /Modules/\*НазваниеМодуля\*;  
5\. в Visual Studio создайте новый проект библиотеки .NET Core, в качестве расположения укажите директорию /Modules/\*Название_модуля\*;  
6\. в обозревателе решений вызовите контекстное меню созданного решения и выберите Добавить - Существующий проект. В диалоговом окне выберите фильтр "Файлы решений (.sln)", затем выберите файл /Shell/Rack.sln. Добавлять проекты оболочки рекомендуется в отдельную папку решения;  
7\. добавьте в загруженный проект Rack зависимость от вашего модуля. Никогда не добавляйте эту зависимость в коммит!  
8\. выберите загруженный проект Rack в качестве автозагружаемого, восстановите пакеты NuGet;         
9\. постройте решение (Ctrl+Shift+B) и/или запустите отладку. 

### Дополнительные шаги:

#### Создание справки пользователя.

Если модуль должен содержать справку пользователя, создайте в проекте модуля директорию HelpFiles. В директории создайте пустой текстовый файл, представляющий страницу справки, например, \*Название_модуля\*.Help.md. Этот Markdown файл откройте и заполните содержимым в любом удобном редакторе, например, Visual Studio Code. Если в справке необходимо отображать изображения, также поместите их в директорию HelpFiles. В свойствах всех файлов в директории HelpFiles установите параметр "Копировать в выходной каталог": "Копировать более позднюю версию". Это можно сделать, добавив в файл проекта следующую секцию:

```xml
<ItemGroup>
    <None Update="HelpFiles\*">
        <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
</ItemGroup>
```

В точке входа в ваш модуль получите IHelpService и зарегистрируйте в нём созданную страницу справки.

```csharp
public void OnInitialized(IContainerProvider containerProvider)
{
    var helpService = containerProvider.Resolve<IHelpService>();
    helpService.RegisterPage(
        "Название страницы справки",
        File.ReadAllText(@"HelpFiles/Название_модуля.Help.md"),
        "Название_модуля",
        "Русский");
}
```

#### Создание ченджлога.

Модуль должен содержать список изменений. Создайте в проекте модуля директорию Changelogs. В директории создайте пустой текстовый файл *НазваниеМодуля*.md. Добавьте в файл первую строчку-заголовок "# Список изменений".

Для каждого нового обновления модуля в ченджлоге должна быть запись, описывающая изменения для пользователя. Каждая запись должна начинаться с подзаголовка с номером версии модуля. Каждая новая запись добавляется сверху.

Аналогично справке пользователя, необходимо копировать файл ченджлога в выходную директорию.

```markdown
# Список изменений

## v2.1.0

- Дополнена и обновлена пользовательская справка. Название страницы "Подготовка данных" изменено на "Подготовка данных в Excel". Название страницы "Загрузка данных изменено на "Загрузка данных в Rack". Обновлён раздел "Разбивки". Добавлен раздел "Определение нефтеносных пластов".

## v2.0.2

- Дополнены и исправлены страницы пользовательской справки "5. Утилита GrdToShpfile" и "6. Оформление разреза в ArcMap".
```

#### Локализация.

Все сообщения, адресованные пользователю (тексты модальных окон, элементы интерфейса и пр.), рекомендуется выносить в отдельные файлы локализации. Это позволяет иметь единую точку доступа к любому сообщению и изменять локализации в рантайме.

В проекте модуля создайте директорию Localizations, в ней создайте файл \*Название_модуля\*-Russian.json. В свойствах файла выставьте "Копировать в выходной каталог"-"Копировать более позднюю версию". Структура файла должна быть следующей:

```json
{
"Key": "*Название_модуля*",
"Language": "Русский",
"LocalizedValues": {
    "Value1": "Строка на русском языке 1.",
    "Value2": "Строка на русском языке 2."
    }
}
```

После этого локализацию можно использовать из любой вью-модели с помощью свойства Localization. Не забудьте выставить во вью-модели соответствующий LocalizationKey:

```csharp
public class ModuleViewModel : ReactiveViewModel 
{
    ...
    public override IEnumrable<string> LocalizationKeys { get; } = new[] { "*Название_модуля*" };

    public void SendUserMessage()
    {
        // Посылает пользователю сообщение "Строка на русском языке 1.".
        mainWindowService.SendMessage(new Message(Localization[Value1])); 
    }
}
```

Следовательно, локализацию также можно использовать из представлений:

```xml
<UserControl x:Class="*Название_модуля*.Views.Module"
            xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
            xmlns:prism="http://prismlibrary.com/"
            prism:ViewModelLocator.AutoWireViewModel="True">
    <TextBlock Text="{Binding Localization[Value2]}">
</UserControl>
```

Локализацию можно использовать из любого класса с помощью внедрения зависимостей:

```csharp
public class SomeService
{
    private readonly ILocalizationService _localizationService;

    public SomeService(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
    }

    public ILocalization Localization => _localizationService.GetLocalization("*Название_модуля*");
}
```

Если по каким-то причинам для класса невозможно реализовать внедрение через конструктор, можно воспользоваться статическим инстансом:

```csharp
private readonly ILocalizationService _localizationService = LocalizationService.Instance;
```

В качестве основного языка программного комплекса считается русский язык. Если необходимо реализовать поддержку другого языка, добавьте в Localizations новый Json-файл, аналогичный русской локализации. Например, для английского языка необходимо создать файл \*Название_модуля\*\\Localizations\\\*Название_модуля\*-English.json:

```json
{
"Key": "*Название_модуля*",
"Language": "English",
"LocalizedValues": {
    "Value1": "String in English 1.",
    "Value2": "String in English 2."
    }
}
```

#### Создание переносимых конфигураций.

Если для работы модуля необходимо наличие настроек, значения которых не должны сбрасываться при обновлении приложения, рекомендуется использовать сервис IConfigurationService:

1\. Создайте класс с настройками, реализующий интерфейс IConfiguration. Укажите версию 1.0:
```csharp
public class ApplicationSettings : IConfiguration, INotifyPropertyChanged
{
    public int FontSize { get; set; } = 16;

    public ObservableCollection<string> EnabledModules { get; set; } = new ObservableCollection<string>();

    public string Language { get; set; } = "Русский";

    public string Username { get; set; } = string.Empty;

    public Version Version { get; } = new Version(1, 0);

    public event PropertyChangedEventHandler PropertyChanged;
}
```

2\. В классе модуля получите сервис IConfigurationService. С его помощью зарегистрируйте делегат создания настроек по умолчанию:
```csharp
[Module(OnDemand = true, ModuleName = Name)]
public class ChemistryModule : IModule
{
    public void OnInitialized(IContainerProvider containerProvider)
    {
        containerProvider
            .Resolve<IConfigurationService>()
            .RegisterDefaultConfiguration(() => new ApplicationSettings())
        ;
    }
}
```

3\. Получите сервис с помощью внедрения зависимостей и используйте настройки: 
```csharp
public sealed class AppSettingsViewModel : ViewModel, IDialogAware
{
    private readonly IConfigurationService _configurationService;

    public AppSettingsViewModel(
        IConfigurationService configurationService)
    {
        _configurationService = configurationService;
        ApplicationSettings = _configurationService.GetConfiguration<ApplicationSettings>();
    }

    public ApplicationSettings ApplicationSettings { get; }

    public void SaveSettings() 
    {
        _configurationService.SaveSettings(ApplicationSettings);
    }
}
```

4\. Если класс в класс настроек необходимо добавить новые свойства, не требуется вносить никаких изменений, сервис самостоятельно выставит у новых свойств значения по умолчанию. Если в классе изменилось какое-либо из свойств, необходимо увеличить версию класса и добавить миграцию, содержащую предыдущую версию класса настроек и порядок действий, необходимых для преобразования старых настроек:
```csharp
public class ApplicationSettings : IConfiguration, INotifyPropertyChanged
{
    public int FontSize { get; set; } = 16;

    public ObservableCollection<string> EnabledModules { get; set; } = new ObservableCollection<string>();

    public bool IsLanguageRussian { get; set; } = true;

    public string Username { get; set; } = string.Empty;

    public Version Version { get; } = new Version(1, 1);

    public event PropertyChangedEventHandler PropertyChanged;
}
```
```csharp
[Module(OnDemand = true, ModuleName = Name)]
public class ChemistryModule : IModule
{
    public void OnInitialized(IContainerProvider containerProvider)
    {
        containerProvider
            .Resolve<IConfigurationService>()
            .RegisterDefaultConfiguration(() => new ApplicationSettings())
            .AddMigration<ApplicationSettings>(new Version(1, 0), jObject =>
                {
                    var oldValue = jObject.Value<string>("Language");
                    jObject.Remove("Language");
                    jObject.Add("IsLanguageRussian", oldValue == "Русский");
                })
        ;
    }
}
```

На данный момент IConfigurationService работает **только с одним** экземпляром настроек одного типа.

#### Версионирование.

В программном комплексе для задания версий используется  [семантическое версионирование](https://semver.org/lang/ru/). Краткая выдержка о правилах формирования версии:
>Учитывая номер версии МАЖОРНАЯ.МИНОРНАЯ.ПАТЧ, следует увеличивать:
>
>1. МАЖОРНУЮ версию, когда сделаны обратно несовместимые изменения API.
>
>2. МИНОРНУЮ версию, когда вы добавляете новую функциональность, не нарушая обратной совместимости.
>
>3. ПАТЧ-версию, когда вы делаете обратно совместимые исправления.
>
>Дополнительные обозначения для предрелизных и билд-метаданных возможны как дополнения к МАЖОРНАЯ.МИНОРНАЯ.ПАТЧ формату.

Для автоматизации задания версии используется утилита [GitVersion](https://github.com/GitTools/GitVersion/).

Существует несколько вариантов поставки утилиты, в нашем случае применяется MSBuildTask, поскольку в разработке и развёртывании применяется MSBuild, а NuGet-пакет не рекомендуется использовать в документации. Для автоматизации версионирования модуля необходимо выполнить следующие действия для каждой сборки (*в рамках модуля все сборки должны иметь эквивалентные версии, в независимости от того, применены изменения ко всем или отдельным сборкам.*):

1\. Добавить в проект NuGet-пакет GitVersionTask (проверьте актуальную версию пакета):

```
<PackageReference Include="GitVersionTask" Version="5.3.7">
    <PrivateAssets>all</PrivateAssets>
    <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
</PackageReference>
```

2\. Убедиться, что в файле проекта **нет** настройки, запрещающей генерировать информацию о сборке:

```
<GenerateAssemblyInfo>false</GenerateAssemblyInfo>
```

3\. Удалить информацию о версиях из файла AssemblyInfo (если такой имеется в сборке):
   
```
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
```

На этом настройка утилиты завершена. Далее в ходе работы необходимо оформлять коммиты в соответствии с принятыми соглашениями работы с Git.

#### Соглашения по ведению Git-а.

Для работы с Git-ом используется процесс [GitHubFlow](https://guides.github.com/introduction/flow/) ([перевод на русский](https://habr.com/ru/post/346066/)). Вот основные принципы GitHubFlow, которые необходимо учитывать:

* всё, что находится в ветке master — гарантированно стабильно и готово к деплою в любой момент;
* для разработки новой функциональности создавайте новую ветвь от master; 
* вливайте изменения из master в свою ветвь как можно чаще, чтобы она всегда оставалась актуальной и готовой к обратному слиянию;
* при слиянии и отправке изменений в master необходимо производить деплой.
  
Сообщения к коммитам пишутся в соответствии с [соглашениями по ссылке](https://www.conventionalcommits.org/en/v1.0.0/). Каждое сообщение должно начинаться с +semver: *major/breaking/minor/feature/fix/patch*. Для коммитов, увеличивающих мажорные версии сборок, обязательно ставить тэги с соответствующей версией. То же самое рекомендуется для минорных версий.

## Общие положения по разработке:

### Использование графических кнопок (т. е. кнопок с иконками вместо текста).

При разработке Rack используется библиотека MaterialDesign. Помимо прочего, она предоставляет набор иконок, которые можно использовать следующим образом:

```xml
<Button Command="{Binding AddSampleGiverCommand}">
    <materialDesign:PackIcon Kind="Add" />
</Button>
```

Такое использование иконок в кнопках особенно удачно подходит для обозначения типовых команд (+ - добавить элемент, x - удалить элемент и т. д.), а также для уменьшения размера кнопок. Для каждой такой кнопки необходимо указывать ToolTip, в котором текстом описывается команда, которая выполняется при нажатии на кнопку, чтобы пользователь мог однозначно понять её смысл. Также необходимо выставить небольшое значение (например, 100 мс) свойства ToolTipService.InitialShowDelay, чтобы пользователь получал описание команды при наведении на кнопку как можно быстрее. Для кнопок в панелях инструментах (ToolBar) вручную выставлять TollTipService.InitialShowDelay не требуется, так как это уже сделано в Rack/Override.xaml.

Примеры использования кнопок с иконками:

```xml
<!-- Использование кнопок в меню. Для некоторых кнопок (например, FileSelectorButton) необходимо вручную указывать стиль. -->
<ToolBarTray DockPanel.Dock="Top">
    <ToolBar>
        <Button Command="{Binding AddSampleCommand}"
                ToolTip="{Binding Localization[AddSample]}">
            <materialDesign:PackIcon Kind="Add" />
        </Button>
        <Button Command="{Binding EditSampleCommand}"
                ToolTip="{Binding Localization[EditSample]}">
            <materialDesign:PackIcon Kind="Edit" />
        </Button>
        <Button Command="{Binding DeleteSampleCommand}"
                ToolTip="{Binding Localization[DeleteSample]}">
            <materialDesign:PackIcon Kind="Delete" />
        </Button>
    </ToolBar>
</ToolBarTray>
```

```xml
<!-- Использование кнопки в связке с комбобоксом. Рекомендуется использовать стиль MaterialDesignFloatingActionMiniButton. -->
<DockPanel KeyboardNavigation.TabNavigation="Local">
	<Button DockPanel.Dock="Right"
			Command="{Binding DataContext.AddSampleGiverCommand, ElementName=Root}"
			Style="{StaticResource MaterialDesignFloatingActionMiniButton}" 
            TabIndex="2"
			ToolTip="{Binding DataContext.Localization[AddSampleGiver], ElementName=Root}"
            ToolTipService.InitialShowDelay="100">
		<materialDesign:PackIcon Kind="Add" />
	</Button>
	<ComboBox
		TabIndex="1"
		IsEditable="True"
		SelectedItem="{Binding Giver}"
		ItemsSource="{Binding DataContext.SampleGivers, ElementName=Root}"
		DisplayMemberPath="Name"
	    materialDesign:HintAssist.Hint="{Binding DataContext.Localization[SampleGiver], ElementName=Root}" />
</DockPanel>
```

### Очистка полей ввода

Если необходимо очистить ComboBox или TextBox, можно воспользоваться attached property TextFieldAssist.HasClearButton из MaterialDesign. При нажатии на кнопку SelectedItem/SelectedValue в ComboBox сбросится в default.

```xml
<ComboBox
    ItemsSource="{Binding Notes}"
    SelectedItem="{Binding AccreditationAreaItem.Note}"
    DisplayMemberPath="Note"
    materialDesign:HintAssist.Hint="{Binding Localization[Note]}"
    materialDesign:TextFieldAssist.HasClearButton="True" />
```

По умолчанию кнопка очистки является элементом навигации. Во многих ситуациях такое поведение нежелательно, поэтому где-либо уровнем выше в визуальном дереве можно переопределить стиль, который используют кнопки очистки:

```xml
<Style
    x:Key="MaterialDesignToolButton"
    BasedOn="{StaticResource MaterialDesignToolButton}"
    TargetType="Button">
    <Setter Property="IsTabStop" Value="False" />
</Style>
```
