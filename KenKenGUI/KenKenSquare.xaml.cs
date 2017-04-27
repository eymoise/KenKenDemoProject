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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KenKenGUI
{
    /// <summary>
    /// Interaction logic for KenKenSquare.xaml
    /// </summary>
    public partial class KenKenSquare : Button
    {
        public KenKenSquare()
        {
            InitializeComponent();
        }

        public string CentreValue
        {
            get { return (string)GetValue(CentreValueProperty); }
            set { SetValue(CentreValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CentreValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CentreValueProperty =
            DependencyProperty.Register("CentreValue",
                typeof(string),
                typeof(KenKenSquare),
                new PropertyMetadata("",
                    new PropertyChangedCallback(CentreValueChanged)));

        private static void CentreValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            KenKenSquare square = (KenKenSquare)d;
            square.txtCentre.Text = e.NewValue?.ToString();
        }

        public string TopLeftValue
        {
            get { return (string)GetValue(TopLeftValueProperty); }
            set { SetValue(TopLeftValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TopLeftValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TopLeftValueProperty =
            DependencyProperty.Register("TopLeftValue",
                typeof(string),
                typeof(KenKenSquare),
                new PropertyMetadata("",
                    new PropertyChangedCallback(TopLeftValueChanged)));

        private static void TopLeftValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            KenKenSquare square = (KenKenSquare)d;
            square.txtTopLeft.Text = e.NewValue.ToString();
        }

        public Color ColorIn
        {
            get { return (Color)GetValue(ColorInProperty); }
            set { SetValue(ColorInProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ColorIn.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColorInProperty =
            DependencyProperty.Register("ColorIn",
                typeof(Color),
                typeof(KenKenSquare),
                new PropertyMetadata(Brushes.Orange.Color,
                    new PropertyChangedCallback(ColorInChanged)));

        private static void ColorInChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ColorChanged(d, e);

        private static void ColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            KenKenSquare square = (KenKenSquare)d;
            square.Background = new SolidColorBrush((Color)e.NewValue);
        }

        public Color ColorOut
        {
            get { return (Color)GetValue(ColorOutProperty); }
            set { SetValue(ColorOutProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ColorOut.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColorOutProperty =
            DependencyProperty.Register("ColorOut",
                typeof(Color),
                typeof(KenKenSquare),
                new PropertyMetadata(Brushes.Red.Color,
                    new PropertyChangedCallback(ColorOutChanged)));

        private static void ColorOutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ColorChanged(d, e);

        public bool IsDefined
        {
            get { return (bool)GetValue(IsDefinedProperty); }
            set { SetValue(IsDefinedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsDefined.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsDefinedProperty =
            DependencyProperty.Register("IsDefined", typeof(bool), typeof(KenKenSquare), new PropertyMetadata(false));

        private ColorAnimation anAnimation = new ColorAnimation();

        private void AnimateBackground(Color startColor, Color endColor)
        {
            SolidColorBrush myBrush = new SolidColorBrush();

            anAnimation.From = startColor;
            anAnimation.To = endColor;
            anAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(300));

            // Apply the animation to the brush's Color property.
            myBrush.BeginAnimation(SolidColorBrush.ColorProperty, anAnimation);
            this.Background = myBrush;
        }

        public void button_MouseEnter(object sender, MouseEventArgs e)
        {
            AnimateBackground(ColorIn, ColorOut);
        }

        public void button_MouseLeave(object sender, MouseEventArgs e)
        {
            AnimateBackground(ColorOut, ColorIn);
        }

        private void button_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            bool newVal = (bool)e.NewValue;
            if (newVal)
            {
                this.Background = Brushes.Orange;
            }
            else
            {
                this.Background = Brushes.Gray;
            }
        }

        public void Reset()
        {
            IsDefined = false;
            ColorIn = Brushes.Orange.Color;
            ColorOut = Brushes.Red.Color;
            TopLeftValue = String.Empty;
            txtCentre.Text = String.Empty;
        }

        public void RotateSquare(double endAngle, double centreX, double centreY)
        {
            var dblAnim = new DoubleAnimation(0, endAngle, new Duration(TimeSpan.FromMilliseconds(500)));
            dblAnim.AutoReverse = true;
            var rt = new RotateTransform();
            rt.CenterX = centreX;
            rt.CenterY = centreY;
            this.RenderTransform = rt;
            rt.BeginAnimation(RotateTransform.AngleProperty, dblAnim);
        }
    }
}
