using System.Windows;
using Rpg.Wpf.ViewModels;

namespace Rpg.Wpf
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}