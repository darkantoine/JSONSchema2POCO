using System;

namespace JSONSchema2POCO
{
    public class OneOf<T1, T2>
    {
       
        private readonly object value;

        public OneOf(object value)
        {
            if (value is T1 || value is T2)
            {
                this.value = value;
            }
            else throw new ArgumentException();
        }

        public object GetValue()
        {
            return value;
        }

        public Type GetUnderlyingType()
        {
            return value.GetType();
        }

    }
}