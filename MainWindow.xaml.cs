using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using CSInterpolation;
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
        List<Equation> equations = new List<Equation>(); //lista przetwarzanych równań
        int threads = Environment.ProcessorCount;
        bool asm = true; //czy liczymy w asemblerze
        bool stats = false; //czy wywołano okno statystyk
        bool error = false; //czy wystąpił błąd w odczycie pliku
        TimeSpan timeS; //czas obliczenia wszystkich równań
        string FilePath = ""; //ścieżka do pliku wejściowego

        [DllImport(@"C:\Users\Paweł\Documents\Projekty Visual Studio\Lagrange\x64\Debug\JAAsm.dll")]
        static extern void LagrangeAsm(float[] liCoefficients, float[] x, float[] newCoefficients, int j, int i, int degree);

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

            string statsText = "Czasy wykonania programu w zależności od liczby wątków i sposobu implementacji algorytmu.\n\nJęzyk wysokiego poziomu (C#):\n";
            stats = true;
            threads = 1;
            asm = false;

            //Obliczenia w C#
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

            statsText += "\nJęzyk niskiego poziomu (ASM):\n";
            threads = 1;
            asm = true;

            //Obliczenia w asm
            while (threads != 128)
            {
                TimeSpan t = TimeSpan.Zero;
                statsText += "Liczba wątków: " + threads + "\t\t";
                for (int i = 0; i < 5; ++i)
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

            //Generowanie okna ze statystykami
            Stats s = new Stats(statsText);
            s.Activate();
            s.Show();

            time.Text = "Czas wykonania: Wyniki w drugim oknie.";
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
                            float xValue = float.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
                            float yValue = float.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);

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
                {
                    int degree = equations[i].x.Count-1;
                    float[] coefficients = new float[degree + 1];

                    for (int l = 0; l < equations[i].y.Count; ++l)
                    {
                        // Oblicz wielomian L_i(x) i pomnóż przez y[i]
                        float[] liCoefficients = new float[degree + 1];
                        liCoefficients[0] = 1; // L_i(x) zaczyna się od 1 jako stała

                        for (int j = 0; j < equations[i].x.Count; ++j)
                        {
                            if (l != j)
                            {
                                float[] xArray = equations[i].x.ToArray();
                                float[] newCoefficients = new float[degree + 1];
                                LagrangeAsm(liCoefficients, xArray, newCoefficients, j, l, degree);
                                liCoefficients = newCoefficients;
                            }
                        }

                        // Dodajemy y[i] * L_i(x) do końcowego wielomianu
                        for (int k = 0; k <= degree; ++k)
                        {
                            coefficients[k] += equations[i].y[l] * liCoefficients[k];
                        }
                    }

                    for (int j = coefficients.Length - 1; j > 1; --j)
                    {
                        float coefR = (float)Math.Round(coefficients[j], 2);
                        if (coefR != 0)
                        {
                            if (coefR != 1)
                                equations[i].result += coefR;
                            equations[i].result += "x^" + j + " + ";
                            if (coefR.ToString() == "NaN")
                            {
                                equations[i].result = "B";
                                break;
                            }
                        }
                    }
                    equations[i].result += Math.Round(coefficients[1], 2) + "x + ";
                    equations[i].result += Math.Round(coefficients[0], 2);
                    if (equations[i].result.First() == 'B')
                        equations[i].result = "Brak wielomianu interpolacyjnego";
                }
                else
                {
                    CSLagrange r = new CSLagrange(equations[i].x, equations[i].y); //klasa w bibliotece dll
                    equations[i].result = r.getResult();
                }
            });

            DateTime stopTime = DateTime.Now;
            TimeSpan timeSpan = stopTime - startTime;
            timeS = timeSpan;
            time.Text = "Czas wykonania: " + timeSpan.TotalMilliseconds + " ms";
            
            if(!stats) //nie zapisujemy pliku gdy chcemy wygenerować statystyki
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

        //Zmiana liczby wątków
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

        //Odczyt punktów z pliku
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

        //Generowanie przykładowych punktów do obliczeń (wywoływane w konstruktorze w razie potrzeby)
        private void RandFile(string path, int numEquations, int minEq, int maxEq)
        {
            string fileName = path+".txt";
            Random random = new Random();

            // Lista przechowująca linie do zapisania w pliku
            List<string> lines = new List<string>();

            for (int i = 0; i < numEquations; i++)
            {
                // Liczba punktów w równaniu (np. od 2 do 5)
                int numPoints = random.Next(minEq, maxEq+1);

                List<string> points = new List<string>();
                for (int j = 0; j < numPoints; j++)
                {
                    // Generowanie losowych współrzędnych (x, y)
                    double x = Math.Round(random.NextDouble() * 100 - 50, 1); // Losowe x od -50 do 50
                    double y = Math.Round(random.NextDouble() * 100 - 50, 1); // Losowe y od -50 do 50
                    points.Add($"({x.ToString(CultureInfo.InvariantCulture)}, {y.ToString(CultureInfo.InvariantCulture)})");
                }

                // Tworzenie jednej linii z punktami
                lines.Add(string.Join(" ", points) + ",");
            }
            File.WriteAllLines(fileName, lines);
        }
    }
}