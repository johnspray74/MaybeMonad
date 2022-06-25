using System;


namespace Monad.MinusOneDeferredPull
{
    public static class ExtensionMethod
    {
        public static Func<int> ToFunc(this int source)
        {
            return () => source;
        }

        public static Func<int> Bind(this Func<int> source, Func<int, int> function)
        {
            return () =>
            {
                int value = source();
                return value == -1 ? -1 : function(value);
            };
        }
    }
}