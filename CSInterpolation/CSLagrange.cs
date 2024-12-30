using System;
using System.Collections.Concurrent;
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
        public CSLagrange(List<double> x, List<double> y)
        {
            var xx = MathS.Var("x");
            var partialResults = new ConcurrentBag<Entity>();
            for(int i = 0; i<y.Count; ++i)
            { 
                var mulexp = xx/xx;
                for (int j = 0; j < x.Count; ++j)
                {
                    if (i != j)
                    {
                        var res = (xx - x[j]) / (x[i] - x[j]);
                        mulexp *= res;
                    }
                }
                var partialResult = y[i] * mulexp;
                partialResults.Add(partialResult);
            }
            var addexp = partialResults.Aggregate(xx * 0, (sum, item) => sum + item);
            addexp = addexp.Simplify();
            if (addexp.ToString() == "NaN")
                this.result = "Brak wielomianu interpolacyjnego";
            else
                this.result = addexp.ToString();
        }
        public string getResult() { return this.result; }
    }
}
