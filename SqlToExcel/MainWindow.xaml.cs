using SqlToExcel.ViewModels;
using System.Windows;
using HandyControl.Controls;

namespace SqlToExcel
{
    public partial class MainWindow : BlurWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            var viewModel = new MainViewModel();
            DataContext = viewModel;

            // This ensures that the event is raised after the window is fully loaded
            this.Loaded += (s, e) => 
            {
                viewModel.CheckDatabaseConfiguration();
            };
        }
    }
}
