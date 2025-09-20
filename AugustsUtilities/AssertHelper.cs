using System;

namespace AugustsUtility.Testing
{
    public static class Assert
    {
        public static void IsTrue(bool condition, string message = "")
        {
            if (!condition)
                throw new AssertionException($"Assertion Failed: Expected true, but was false. {message}");
        }

        public static void IsFalse(bool condition, string message = "")
        {
            if (condition)
                throw new AssertionException($"Assertion Failed: Expected false, but was true. {message}");
        }

        public static void IsNull(object obj, string message = "")

        {
            if (obj != null)
                throw new AssertionException($"Assertion Failed: Expected null, but was {obj}. {message}");
        }

        public static void IsNotNull(object obj, string message = "")
        {
            if (obj == null)
                throw new AssertionException($"Assertion Failed: Expected not null, but was null. {message}");
        }

        public static void AreEqual<T>(T expected, T actual, string message = "")
        {
            if (!Equals(expected, actual))
                throw new AssertionException($"Assertion Failed: Expected '{expected}', but got '{actual}'. {message}");
        }
    }

    public class AssertionException : Exception
    {
        public AssertionException(string message) : base(message) { }
    }
}
