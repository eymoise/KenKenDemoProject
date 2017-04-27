using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Text.RegularExpressions;
using KenKenLogic;

namespace KenKenGUI
{
    /// <summary>
    /// Interaction logic for NewRegion.xaml
    /// </summary>
    public partial class NewRegion : Window
    {
        public NewRegion(int regionCount)
        {
            InitializeComponent(); 
         
            switch (regionCount)
            {
                case 0:
                    throw new ArgumentException("region may not be empty", "region");
                case 1:
                    throw new ArgumentException("This class should not be used for the one square option", "region");
                case 2:
                    operationChoice.ItemsSource = new ObservableCollection<MathOp> { MathOp.Addition, MathOp.Subtraction, MathOp.Multiplication, MathOp.Division };
                    break;
                default:
                    operationChoice.ItemsSource = new ObservableCollection<MathOp> { MathOp.Addition, MathOp.Multiplication };
                    break;                
            }

            operationChoice.SelectedIndex = 0;
        }

        public int Target { get; private set; }

        public MathOp Operation { get; private set; }

        private Regex numericOnly = new Regex("^[0-9]+$");

        private void tbTarget_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !numericOnly.IsMatch(e.Text);
            btnOK.IsEnabled = !e.Handled;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.Target = int.Parse(tbTarget.Text);
            this.Operation = (MathOp)operationChoice.SelectedItem;
            this.DialogResult = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
