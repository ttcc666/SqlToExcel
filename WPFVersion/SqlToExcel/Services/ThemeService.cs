using System;
using System.Linq;
using System.Windows;

namespace SqlToExcel.Services
{
    public class ThemeService
    {
        public void ChangeTheme(string skin)
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
}
