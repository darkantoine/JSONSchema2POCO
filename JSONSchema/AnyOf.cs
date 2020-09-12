using System;

namespace JSONSchema2POCO
{
    public class AnyOf<T1, T2>
    {
       
        private object value;

        public object Value
        {
            get
            {
                return value;
            }
        }

        public AnyOf(object value)
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

        public void ChangeValue(object newValue)
        {
            if (newValue.GetType() == value.GetType())
            {
                value = newValue;
            }
            else
            {
                throw new ArgumentException();
            }
        }           

    }

    public class AnyOf<T1, T2, T3>
    {

        private readonly object value;

        public AnyOf(object value)
        {
            if (value is T1 || value is T2 || value is T3)
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

    public class AnyOf<T1, T2, T3, T4>
    {

        private readonly object value;

        public AnyOf(object value)
        {
            if (value is T1 || value is T2 || value is T3 || value is T4)
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

    public class AnyOf<T1, T2, T3, T4, T5>
    {

        private readonly object value;

        public AnyOf(object value)
        {
            if (value is T1 || value is T2 || value is T3 || value is T4 || value is T5)
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