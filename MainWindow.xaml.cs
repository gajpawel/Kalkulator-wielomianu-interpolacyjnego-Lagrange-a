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
using AngouriMath;
using CSInterpolation;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;

namespace Lagrange
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int parameters = 2;
        List<Equation> equations = new List<Equation>();
        int threads = Environment.ProcessorCount;
        bool asm = false;
        bool stats = false;
        bool error = false;
        TimeSpan timeS;
        string FilePath = "";

        [DllImport(@"C:\Users\Paweł\Documents\Projekty Visual Studio\Lagrange\x64\Debug\JAAsm.dll")]
        static extern int LagrangeAsm(int a, int b);

        public MainWindow()
        {
            InitializeComponent();
            sliderThreads.Value = Environment.ProcessorCount;
        }

        private void ButtonTime_Click(object sender, RoutedEventArgs e)
        {
            if(FilePath=="")
            {
                result.Text = "Nie przesłano pliku";
                return;
            }

            string statsText = "Czasy wykonania programu w zależności od liczby wątków i sposobu implementacji algorytmu.\n\nJęzyk wysokiego poziomu:\n";
            stats = true;
            threads = 1;

            while(threads!=128)
            {
                TimeSpan t = TimeSpan.Zero;
                statsText += "Liczba wątków: " + threads + "\t\t";
                for(int i = 0; i<5; ++i)
                {
                    ButtonCalculate_Click(null, null);
                    if (error)
                        return;
                    t += timeS;
                }
                t /= 5;
                statsText += "średni czas: " + t.TotalMilliseconds + " ms\n";
                threads *= 2;
            }
            statsText += "\nJęzyk niskiego poziomu:\nAlgorytm w trakcie budowy.";
            
            Stats s = new Stats(statsText);
            s.Activate();
            s.Show();
            
            result.Text = "Zakończono generowanie.";
            stats = false;
            threads = int.Parse(sliderThreads.Value.ToString());
        }

        private void ButtonCalculate_Click(object sender, RoutedEventArgs e)
        {
            if (FilePath == "")
            {
                result.Text = "Nie przesłano pliku tekstowego.";
                error = true;
                return;
            }

            //Odczyt z pliku
            this.equations.Clear();   
            try
            {
                string[] lines = File.ReadAllLines(FilePath);

                if (lines.Length == 0)
                {
                    result.Text = "Plik jest pusty.";
                    error = true;
                    return;
                }
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    Equation equation = new Equation();

                    // Regex do wyodrębnienia par (x, y) z linii
                    var matches = Regex.Matches(line, @"\(([-+]?\d*\.?\d+),\s*([-+]?\d*\.?\d+)\)");

                    if (matches.Count == 0)
                    {
                        result.Text = "Nieprawidłowy format danych";
                        error = true;
                        return;
                    }

                    try
                    {
                        foreach (Match match in matches)
                        {
                            // Parsowanie x i y
                            double xValue = double.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
                            double yValue = double.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);

                            // Dodanie do list
                            equation.x.Add(xValue);
                            equation.y.Add(yValue);
                        }

                        // Sprawdzenie spójności danych
                        if (equation.x.Count != equation.y.Count)
                        {
                            result.Text = "Niespójne dane";
                            error = true;
                            return;
                        }

                        // Dodanie równania do listy
                        equations.Add(equation);
                    }
                    catch (FormatException)
                    {
                        result.Text = "Błąd formatowania danych";
                        error = true;
                        return;
                    }
                }
            }
            catch (FileNotFoundException)
            {
                result.Text = "Nie znaleziono pliku. Sprawdź poprawność ścieżki.";
                error = true;
                return;
            }
            catch (InvalidDataException)
            {
                result.Text = "Błąd danych wejściowych";
                error = true;
                return;
            }
            tEquations.Text = "Liczba równań: " + equations.Count.ToString();

            //Obliczenia wielomianów interpolacyjnych
            DateTime startTime = DateTime.Now;
            
            ParallelOptions parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = threads // Ustawienie maksymalnej liczby wątków
            };

            Parallel.For(0, equations.Count, parallelOptions, i =>
            {
                if (asm)
                    CalculateAsm();
                else
                {
                    CSLagrange r = new CSLagrange(equations[i].x, equations[i].y);
                    equations[i].result = r.getResult();
                }
            });

            DateTime stopTime = DateTime.Now;
            TimeSpan timeSpan = stopTime - startTime;
            timeS = timeSpan;
            time.Text = "Czas wykonania: " + timeSpan.TotalMilliseconds + " ms";
            
            if(!stats)
                SaveFile();
        }

        public void SaveFile()
        {
            // Generowanie zawartości pliku
            StringBuilder fileContent = new StringBuilder();
            fileContent.AppendLine($"Wygenerowano dnia: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");

            foreach (var equation in equations)
            {
                fileContent.AppendLine(equation.result);
            }

            // Otwieranie okna dialogowego
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Title = "Zapisz plik wynikowy",
                Filter = "Pliki tekstowe (*.txt)|*.txt",
                FileName = "Wyniki_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt"
            };

            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    // Zapis do pliku
                    File.WriteAllText(saveFileDialog.FileName, fileContent.ToString());
                    result.Text = "Plik zapisany pomyślnie";
                }
                catch (Exception)
                {
                    result.Text = "Błąd przy zapisie pliku";
                }
            }
        }

        public void CalculateAsm()
        {
            //var xx = MathS.Var("x");
            //var addexp = 0 * xx;
            //
            //for (int i = 0; i < y.Count; ++i)
            //{
            //    var mulexp = xx / xx;
            //    Parallel.For(0, x.Count, new ParallelOptions { MaxDegreeOfParallelism = this.threads }, j =>
            //    {
            //        if (i != j)
            //            LagrangeAsm(1, 2);
            //    });
            //    var partialResult = y[i] * mulexp;
            //    addexp += partialResult;
            //}
            //addexp = addexp.Simplify();
            //if (addexp.ToString() == "NaN")
            //    result.Text = "Brak wielomianu interpolacyjnego";
            //else
            //    result.Text = addexp.ToString();
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

        private void ButtonRead_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Pliki tekstowe (*.txt)|*.txt";
            openFileDialog.FilterIndex = 2;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            FilePath = openFileDialog.FileName;
            FileName.Content = openFileDialog.SafeFileName;
        }
    }
}