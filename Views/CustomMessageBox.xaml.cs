using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ADTool.Views
{
    /// <summary>
    /// Interaktionslogik für CustomMessageBox.xaml
    /// </summary>
    public partial class CustomMessageBox : Window
    {
        public string UserInput { get; private set; }

        public CustomMessageBox(string message)
        {
            InitializeComponent();
            TextBlockMessage.Text = message;
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            UserInput = TextBoxInput.Text;
            this.Close();
        }
    }
}
