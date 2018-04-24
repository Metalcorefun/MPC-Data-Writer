using System;
using System.Windows;

namespace DataWriter
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Closing += (s, e) => (this.DataContext as IDisposable).Dispose();
        }
    }
}
