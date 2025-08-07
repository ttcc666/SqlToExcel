using System.Windows;

namespace SqlToExcel;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public void UpdateTheme(string skin)
    {
        var dictionaries = Application.Current.Resources.MergedDictionaries;
        var oldSkin = dictionaries.FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("Skin"));
        if (oldSkin != null)
        {
            dictionaries.Remove(oldSkin);
        }

        var newSkin = new ResourceDictionary
        {
            Source = new Uri($"pack://application:,,,/HandyControl;component/Themes/Skin{skin}.xaml", UriKind.Absolute)
        };
        dictionaries.Add(newSkin);
    }
}