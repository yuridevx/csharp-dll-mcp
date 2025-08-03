using System;

namespace MyTestLibrary
{
    public interface IMyInterface
    {
        string GetName();
    }

    public enum MyEnum
    {
        ValueA,
        ValueB = 10,
        ValueC
    }

    public struct MyStruct
    {
        public int Number { get; set; }
        public string Text { get; set; }

        public MyStruct(int number, string text)
        {
            Number = number;
            Text = text;
        }

        public string GetInfo()
        {
            return $"Number: {Number}, Text: {Text}";
        }
    }

    public class MyGenericClass<T>
    {
        public T Value { get; set; }

        public MyGenericClass(T value)
        {
            Value = value;
        }

        public string GetValueType()
        {
            return typeof(T).FullName;
        }
    }
}