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
using System.Threading.Tasks;
using MathNet.Symbolics;
using Expr = MathNet.Symbolics.SymbolicExpression;
using System.Linq.Expressions;
using CSInterpolation;
using System.Reflection;

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

        string libraryPath = @"C:\Users\Paweł\Documents\Projekty Visual Studio\Lagrange\CSInterpolation\bin\Debug\CSInterpolation.dll";

        public MainWindow()
        {
            this.InitializeComponent();
            sliderThreads.Value = Environment.ProcessorCount;
        }

        private void ButtonTime_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.x.Clear();
            this.y.Clear();
            List<System.Windows.Controls.TextBox> tx = new List<System.Windows.Controls.TextBox> {x1, x2, x3, x4, x5 };
            List<System.Windows.Controls.TextBox> ty = new List<System.Windows.Controls.TextBox> { y1, y2, y3, y4, y5 };
            try
            {
                for (int i = 0; i < this.parameters; ++i)
                {
                    if (tx[i].Text.Length == 0 || ty[i].Text.Length == 0)
                        return;
                    this.x.Add(double.Parse(tx[i].Text));
                    this.y.Add(double.Parse(ty[i].Text));
                }
            }catch (System.FormatException)
            {
                result.Text = "Nieprawidłowe argumenty.";
                return;
            }
            DateTime startTime = DateTime.Now;
            if (this.asm)
                CalculateAsm();
            else
                CalculateCs();
            DateTime stopTime = DateTime.Now;
            TimeSpan timeSpan = stopTime - startTime;
            time.Text="Czas wykonania: " + timeSpan.TotalMilliseconds + " ms";
        }

        public void CalculateCs()
        {
            Assembly assembly = Assembly.LoadFrom(libraryPath);
            Type mathOperationsType = assembly.GetType("CSInterpolation.CSLagrange");
            object mathOperationsInstance = Activator.CreateInstance(mathOperationsType);
            MethodInfo addMethod = mathOperationsType.GetMethod("licz");
            //int res = (int)addMethod.Invoke(mathOperationsInstance, new object[] { 5, 7 });

            var xx = Expr.Variable("x");
            var addexp = 0 * xx;

            for (int i = 0; i < y.Count; ++i)
            {
                var mulexp = xx/xx;
                Parallel.For(0, x.Count, new ParallelOptions { MaxDegreeOfParallelism = this.threads }, j =>
                {
                    if (i != j)
                        mulexp *= (xx - x[j]) / (x[i] - x[j]);
                });
                var partialResult = y[i] * mulexp;
                addexp += partialResult;
            }
            //var simplified = MathNet.Symbolics.Simplification.Full(addexp);
            result.Text = addexp.ToString();
        }

        public void CalculateAsm()
        {
            var xx = Expr.Variable("x");
            var addexp = 0 * xx;

            for (int i = 0; i < y.Count; ++i)
            {
                var mulexp = xx / xx;
                Parallel.For(0, x.Count, new ParallelOptions { MaxDegreeOfParallelism = this.threads }, j =>
                {
                    if (i != j)
                        LagrangeAsm(1, 2);
                });
                var partialResult = y[i] * mulexp;
                addexp += partialResult;
            }
            result.Text = addexp.ToString();
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
                if (selectedItem.Content.ToString() == "3")
                {
                    x3.Visibility = Visibility.Visible;
                    y3.Visibility = Visibility.Visible;
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
                else if (selectedItem.Content.ToString() == "4")
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