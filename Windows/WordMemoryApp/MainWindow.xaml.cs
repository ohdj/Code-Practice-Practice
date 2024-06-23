using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WordMemoryApp
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private ObservableCollection<WordEntry> Words { get; set; } = new ObservableCollection<WordEntry>();
        private string FilePath;
        private IntPtr hWnd;

        public MainWindow()
        {
            this.InitializeComponent();
            FilePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "words.json");
            LoadWords();

            hWnd = WindowNative.GetWindowHandle(this);
            ShortcutKeyHandler.RegisterShortcutKey(hWnd);

            this.Closed += MainWindow_Closed;

            // Bind the Words collection to the ListView
            WordListView.ItemsSource = Words;
        }

        private void MainWindow_Closed(object sender, WindowEventArgs args)
        {
            ShortcutKeyHandler.UnregisterShortcutKey(hWnd);
        }

        private void SaveWord_Click(object sender, RoutedEventArgs e)
        {
            string word = WordInput.Text;
            if (!string.IsNullOrEmpty(word))
            {
                var existingEntry = Words.FirstOrDefault(w => w.Word.Equals(word, StringComparison.OrdinalIgnoreCase));
                if (existingEntry != null)
                {
                    existingEntry.Date = DateTime.Now.ToString("g");
                    existingEntry.Count++;
                    Words.Remove(existingEntry);
                    Words.Insert(0, existingEntry);
                }
                else
                {
                    var entry = new WordEntry
                    {
                        Date = DateTime.Now.ToString("g"),
                        Word = word,
                        Translation = TranslateWord(word),
                        Count = 1
                    };
                    Words.Insert(0, entry);
                }
                SaveWords();
                WordInput.Text = string.Empty;
            }
        }

        private void LoadWords()
        {
            if (File.Exists(FilePath))
            {
                string json = File.ReadAllText(FilePath);
                var words = JsonSerializer.Deserialize<ObservableCollection<WordEntry>>(json);
                if (words != null)
                {
                    Words.Clear();
                    foreach (var word in words)
                    {
                        Words.Add(word);
                    }
                }
            }
        }

        private void SaveWords()
        {
            string json = JsonSerializer.Serialize(Words);
            File.WriteAllText(FilePath, json);
        }

        private string TranslateWord(string word)
        {
            // 在此处添加翻译逻辑，可以调用第三方API或者使用内置词典。
            // 为简洁起见，此处仅返回“Translation”字符串。
            return "Translation";
        }

        private async void EditWord_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var wordEntry = button.Tag as WordEntry;

            var dialog = new ContentDialog
            {
                Title = "Edit Word",
                XamlRoot = this.Content.XamlRoot, // 设置 XamlRoot
                Content = new StackPanel
                {
                    Children =
                    {
                        new TextBox { Text = wordEntry.Word, Tag = "Word" },
                        new TextBox { Text = wordEntry.Translation, Tag = "Translation" }
                    }
                },
                PrimaryButtonText = "Save",
                SecondaryButtonText = "Cancel"
            };

            dialog.PrimaryButtonClick += (s, args) =>
            {
                var stackPanel = dialog.Content as StackPanel;
                var wordBox = stackPanel.Children[0] as TextBox;
                var translationBox = stackPanel.Children[1] as TextBox;

                wordEntry.Word = wordBox.Text;
                wordEntry.Translation = translationBox.Text;
                wordEntry.Date = DateTime.Now.ToString("g");

                SaveWords();
                WordListView.ItemsSource = null;
                WordListView.ItemsSource = Words;
            };

            await dialog.ShowAsync();
        }

        private void DeleteWord_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var wordEntry = button.Tag as WordEntry;
            Words.Remove(wordEntry);
            SaveWords();
        }
    }

    public class WordEntry
    {
        public string Date { get; set; }
        public string Word { get; set; }
        public string Translation { get; set; }
        public int Count { get; set; }
    }
}
