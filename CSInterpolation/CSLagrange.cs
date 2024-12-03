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
        public Entity Caclulate(Entity.Variable xx, double xi, double xj)
        {
            return (xx - xj) / (xi - xj);
        }
    }
}
