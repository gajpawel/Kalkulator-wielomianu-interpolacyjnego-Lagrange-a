﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lagrange
{
    internal class Equation //Zestaw współrzędnych + ewentualny wielomian interpolacyjny
    {
        public List<float> x = new List<float>();
        public List<float> y = new List<float>();
        public string result = "";
    }
}
