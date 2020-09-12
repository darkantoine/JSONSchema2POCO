using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassGenerator
{
    public class A: List<string>
    {
        int i;

        public int I { get
            {
                return i;
            }
         }

        private A() { }

       public A(IEnumerable<string> collection)
        {
            if(collection.Count() < 1)
            {
                throw new ArgumentException();
            }
        }

        public static implicit operator A(int i)
        {
            return new A { i = i };
        }

        public static implicit operator int(A a)
        {
            return a.I;
        }
    }
}
