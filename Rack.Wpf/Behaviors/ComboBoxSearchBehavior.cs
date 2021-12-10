using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace Rack.Wpf.Behaviors
{
    /// <summary>
    /// Поведение комбобокса для поиска и фильтрации элементов.
    /// </summary>
    public class ComboBoxSearchBehavior : Behavior<ComboBox>
    {
        public enum SearchPattern
        {
            StartsWith,
            Contains,
            ContainsFromStart
        }

        public static readonly DependencyProperty PatternProperty = DependencyProperty.Register(
            "Pattern", typeof(SearchPattern), typeof(ComboBoxSearchBehavior),
            new PropertyMetadata(SearchPattern.Contains));

        public static readonly DependencyProperty AutoPickItemOnLostFocusProperty = DependencyProperty.Register(
            "AutoPickItemOnLostFocus", typeof(bool), typeof(ComboBoxSearchBehavior));

        private object _initialSelectedItem;

        private string _searchText;

        private bool _wasTextSearchEnabled;

        public bool AutoPickItemOnLostFocus
        {
            get => (bool) GetValue(AutoPickItemOnLostFocusProperty);
            set => SetValue(AutoPickItemOnLostFocusProperty, value);
        }

        public SearchPattern Pattern
        {
            get => (SearchPattern) GetValue(PatternProperty);
            set => SetValue(PatternProperty, value);
        }

        protected override void OnAttached()
        {
            _wasTextSearchEnabled = AssociatedObject.IsTextSearchEnabled;
            AssociatedObject.IsTextSearchEnabled = false;

            AssociatedObject.KeyUp += ComboBoxOnKeyUp;
            AssociatedObject.LostFocus += ComboBoxOnLostFocus;
            AssociatedObject.GotFocus += ComboBoxOnGotFocus;
        }

        private void ComboBoxOnGotFocus(object sender, RoutedEventArgs e)
        {
            _initialSelectedItem = AssociatedObject.SelectedItem;
            AssociatedObject.IsDropDownOpen = true;
        }

        private void ComboBoxOnKeyUp(object sender, KeyEventArgs e)
        {
            if (AssociatedObject.ItemsSource == null)
                return;

            AssociatedObject.IsDropDownOpen = true;

            if (e.Key == Key.Down || e.Key == Key.Up) return;

            if (e.Key == Key.Escape)
            {
                AssociatedObject.SelectedItem = _initialSelectedItem;
                return;
            }

            if (_searchText == AssociatedObject.Text)
                return;

            _searchText = AssociatedObject.Text;

            var itemsView = CollectionViewSource.GetDefaultView(AssociatedObject.ItemsSource);

            if (itemsView.Filter != Filter) itemsView.Filter = Filter;

            var textbox = (TextBox) AssociatedObject.Template
                .FindName("PART_EditableTextBox", AssociatedObject);
            var caretIndex = textbox.CaretIndex;

            itemsView.Refresh();

            if (Pattern == SearchPattern.ContainsFromStart)
                if (itemsView is ListCollectionView view)
                    view.CustomSort = new DefaultComparer(_searchText, AssociatedObject.DisplayMemberPath);

            AssociatedObject.Text = _searchText;
            textbox.CaretIndex = caretIndex;
        }

        private bool Filter(object item)
        {
            if (string.IsNullOrEmpty(_searchText))
                return true;
            if (item == null) return false;
            if (string.IsNullOrEmpty(AssociatedObject.DisplayMemberPath))
            {
                if (Pattern == SearchPattern.StartsWith)
                    return item.ToString().StartsWith(_searchText, StringComparison.CurrentCultureIgnoreCase);
                return CultureInfo.CurrentCulture.CompareInfo
                           .IndexOf(item.ToString(), _searchText, CompareOptions.IgnoreCase) >= 0;
            }

            var displayTextProperty = item.GetType()
                .GetProperty(AssociatedObject.DisplayMemberPath);
            if (Pattern == SearchPattern.StartsWith)
                return displayTextProperty.GetValue(item).ToString()
                    .StartsWith(_searchText, StringComparison.CurrentCultureIgnoreCase);
            return CultureInfo.CurrentCulture.CompareInfo
                       .IndexOf(
                           displayTextProperty.GetValue(item)
                               .ToString(),
                           _searchText,
                           CompareOptions.IgnoreCase) >= 0;
        }

        private void ComboBoxOnLostFocus(object sender, RoutedEventArgs e)
        {
            if (AssociatedObject.ItemsSource == null) return;

            if (string.IsNullOrEmpty(AssociatedObject.Text))
            {
                AssociatedObject.SelectedItem = null;
                return;
            }

            if (!AutoPickItemOnLostFocus) return;

            var itemsView = CollectionViewSource.GetDefaultView(AssociatedObject.ItemsSource);

            if (itemsView.IsEmpty)
            {
                AssociatedObject.SelectedItem = null;
                return;
            }

            if (string.IsNullOrEmpty(AssociatedObject.DisplayMemberPath))
            {
                if (AssociatedObject.SelectedItem == null ||
                    !AssociatedObject.Text.Equals(AssociatedObject.SelectedItem.ToString()))
                    AssociatedObject.SelectedItem = itemsView.Cast<object>().First();
            }
            else
            {
                var displayTextProperty = AssociatedObject.Items[0].GetType()
                    .GetProperty(AssociatedObject.DisplayMemberPath);
                if (AssociatedObject.SelectedItem == null ||
                    !AssociatedObject.Text.Equals(
                        displayTextProperty.GetValue(AssociatedObject.SelectedItem).ToString()))
                    AssociatedObject.Text = displayTextProperty.GetValue(AssociatedObject.SelectedItem)
                        .ToString();
            }

            if (string.IsNullOrEmpty(AssociatedObject.DisplayMemberPath))
            {
                AssociatedObject.Text = AssociatedObject.SelectedItem.ToString();
            }
            else
            {
                var displayTextProperty = AssociatedObject.Items[0].GetType()
                    .GetProperty(AssociatedObject.DisplayMemberPath);
                AssociatedObject.Text = displayTextProperty.GetValue(AssociatedObject.SelectedItem)
                    .ToString();
            }

            _searchText = AssociatedObject.Text;

            itemsView.Refresh();
        }


        protected override void OnDetaching()
        {
            AssociatedObject.IsTextSearchEnabled = _wasTextSearchEnabled;

            AssociatedObject.KeyUp -= ComboBoxOnKeyUp;
            AssociatedObject.LostFocus -= ComboBoxOnLostFocus;
            AssociatedObject.GotFocus -= ComboBoxOnGotFocus;
        }

        /// <summary>
        /// Компаратор для сортировки объектов при Pattern == SearchPattern.ContainsFromStart.
        /// </summary>
        private class DefaultComparer : IComparer
        {
            private readonly string _displayMemberPath;
            private readonly string _searchText;

            public DefaultComparer(string searchText, string displayMemberPath)
            {
                _searchText = searchText;
                _displayMemberPath = displayMemberPath;
            }

            public int Compare(object x, object y)
            {
                int indexInX;
                int indexInY;

                if (string.IsNullOrEmpty(_displayMemberPath))
                {
                    indexInX = CultureInfo.CurrentCulture.CompareInfo
                        .IndexOf(x.ToString(), _searchText, CompareOptions.IgnoreCase);
                    indexInY = CultureInfo.CurrentCulture.CompareInfo
                        .IndexOf(y.ToString(), _searchText, CompareOptions.IgnoreCase);
                    return indexInX - indexInY;
                }

                indexInX = CultureInfo.CurrentCulture.CompareInfo
                    .IndexOf(x.GetType().GetProperty(_displayMemberPath).GetValue(x).ToString(),
                        _searchText, CompareOptions.IgnoreCase);
                indexInY = CultureInfo.CurrentCulture.CompareInfo
                    .IndexOf(y.GetType().GetProperty(_displayMemberPath).GetValue(y).ToString(),
                        _searchText, CompareOptions.IgnoreCase);
                return indexInX - indexInY;
            }
        }
    }
}