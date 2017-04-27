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

namespace KenKenGUI
{
    /// <summary>
    /// Interaction logic for SingleSquare.xaml
    /// </summary>
    public partial class SingleSquare : Window
    {
        public SingleSquare()
        {
            InitializeComponent();
        }

        public int Target { get; private set; }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            Target = int.Parse(cbTarget.Text);
            this.DialogResult = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
