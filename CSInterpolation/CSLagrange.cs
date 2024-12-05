using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AngouriMath;

namespace CSInterpolation
{
    public class CSLagrange
    {
        private string result = "";
        public CSLagrange(List<double> x, List<double> y, int threads)
        {
            var xx = MathS.Var("x");
            var addexp = 0 * xx;

            for (int i = 0; i < y.Count; ++i)
            {
                var mulexp = xx / xx;
                Parallel.For(0, x.Count, new ParallelOptions { MaxDegreeOfParallelism = threads }, j =>
                {
                    if (i != j)
                    {
                        var res = (xx - x[j]) / (x[i] - x[j]);
                        mulexp *= res;
                    }
                });
                var partialResult = y[i] * mulexp;
                addexp += partialResult;
            }
            addexp = addexp.Simplify();
            if (addexp.ToString() == "NaN")
                this.result = "Brak wielomianu interpolacyjnego";
            else
                this.result = addexp.ToString();
        }
        public string getResult() { return this.result; }
    }
}
