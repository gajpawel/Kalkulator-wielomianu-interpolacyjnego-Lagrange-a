using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using Lagrange;
using System.Runtime.CompilerServices;

namespace Lagrange
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int parameters = 2;
        List<double> x = new List<double>();
        List<double> y = new List<double>();
        List<double> r = new List<double>();
        int threads = Environment.ProcessorCount;
        bool asm = true;

        [DllImport(@"C:\Users\Paweł\Documents\Projekty Visual Studio\Lagrange\x64\Debug\JAAsm.dll")]
        static extern int LagrangeAsm(int a, int b);

        public MainWindow()
        {
            this.InitializeComponent();
            sliderThreads.Value = Environment.ProcessorCount;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.x.Clear();
            this.y.Clear();
            DateTime startTime = DateTime.Now;
            List<System.Windows.Controls.TextBox> tx = new List<System.Windows.Controls.TextBox> {x1, x2, x3, x4, x5 };
            List<System.Windows.Controls.TextBox> ty = new List<System.Windows.Controls.TextBox> { y1, y2, y3, y4, y5 };

            for (int i = 0; i<this.parameters; ++i)
            {
                if (tx[i].Text.Length == 0 || ty[i].Text.Length == 0)
                    return;            
                this.x.Add(double.Parse(tx[i].Text));
                this.y.Add(double.Parse(ty[i].Text));
            }
            int a = 2;
            int b = 3;
            int c = 0;
            if (this.asm)
                c = LagrangeAsm(a, b);
            else
                CalculateCs();
            //result.Text = c.ToString();
            DateTime stopTime = DateTime.Now;
            TimeSpan timeSpan = stopTime - startTime;
            time.Text="Czas wykonania: " + timeSpan.TotalMilliseconds + " ms";
        }

        public void CalculateCs()
        {
            double lokX = y[0] / (x[0] - x[1])+y[1]/(x[1]-x[0]);
            lokX = Math.Round(lokX, 3);
            double w = (y[0] / (x[0] - x[1]))*(-x[1])+(y[1]/ (x[1] - x[0]))*(-x[0]);
            w = Math.Round(w, 3);
            string op = "x";
            if (w >= 0)
                op += " + ";
            else
                op += "";
            if (lokX !=1)
                result.Text = lokX.ToString();
            else
                result.Text = "";
            result.Text+=op+w.ToString();
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            thr.Text = "Liczba wątków: " + sliderThreads.Value.ToString();
            this.threads = int.Parse(sliderThreads.Value.ToString());
        }

        private void RadioButton_Checked_Asm(object sender, RoutedEventArgs e)
        {
            this.asm = true;
        }

        private void RadioButton_Checked_Cs(object sender, RoutedEventArgs e)
        {
            this.asm = false;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (variables.SelectedItem is ComboBoxItem selectedItem && selectedItem.Content != null)
            {
                if (selectedItem.Content.ToString()=="3")
                {
                    x3.Visibility = Visibility.Visible;
                    y3.Visibility=Visibility.Visible;
                    x4.Visibility = Visibility.Hidden;
                    y4.Visibility = Visibility.Hidden;
                    x5.Visibility = Visibility.Hidden;
                    y5.Visibility = Visibility.Hidden;

                    Lx3.Visibility = Visibility.Visible;
                    Ly3.Visibility = Visibility.Visible;
                    Lx4.Visibility = Visibility.Hidden;
                    Ly4.Visibility = Visibility.Hidden;
                    Lx5.Visibility = Visibility.Hidden;
                    Ly5.Visibility = Visibility.Hidden;

                    this.parameters = 3;
                }
                else if(selectedItem.Content.ToString() == "4")
                {
                    x3.Visibility = Visibility.Visible;
                    y3.Visibility = Visibility.Visible;
                    x4.Visibility = Visibility.Visible;
                    y4.Visibility = Visibility.Visible;
                    x5.Visibility = Visibility.Hidden;
                    y5.Visibility = Visibility.Hidden;

                    Lx3.Visibility = Visibility.Visible;
                    Ly3.Visibility = Visibility.Visible;
                    Lx4.Visibility = Visibility.Visible;
                    Ly4.Visibility = Visibility.Visible;
                    Lx5.Visibility = Visibility.Hidden;
                    Ly5.Visibility = Visibility.Hidden;

                    this.parameters = 4;
                }
                else if (selectedItem.Content.ToString() == "5")
                {
                    x3.Visibility = Visibility.Visible;
                    y3.Visibility = Visibility.Visible;
                    x4.Visibility = Visibility.Visible;
                    y4.Visibility = Visibility.Visible;
                    x5.Visibility = Visibility.Visible;
                    y5.Visibility = Visibility.Visible;

                    Lx3.Visibility = Visibility.Visible;
                    Ly3.Visibility = Visibility.Visible;
                    Lx4.Visibility = Visibility.Visible;
                    Ly4.Visibility = Visibility.Visible;
                    Lx5.Visibility = Visibility.Visible;
                    Ly5.Visibility = Visibility.Visible;

                    this.parameters = 5;
                }
                else
                {
                    x3.Visibility = Visibility.Hidden;
                    y3.Visibility = Visibility.Hidden;
                    x4.Visibility = Visibility.Hidden;
                    y4.Visibility = Visibility.Hidden;
                    x5.Visibility = Visibility.Hidden;
                    y5.Visibility = Visibility.Hidden;

                    Lx3.Visibility = Visibility.Hidden;
                    Ly3.Visibility = Visibility.Hidden;
                    Lx4.Visibility = Visibility.Hidden;
                    Ly4.Visibility = Visibility.Hidden;
                    Lx5.Visibility = Visibility.Hidden;
                    Ly5.Visibility = Visibility.Hidden;

                    this.parameters = 2;
                }
            }
        }
    }
}