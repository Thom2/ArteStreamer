using System;
using System.Collections.Generic;
using System.Text;

namespace arte_7
{
    public class General
    {
        public delegate void Action();
        public delegate void Action<T1, T2>(T1 val1, T2 val2);
    }
}
