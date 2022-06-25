using System;

namespace Monad.PullMinusOne
{
    public static class ExtensionMethod
    {
        public static int Bind(this int source, Func<int, int> function)
        {
            return source==-1 ? -1 : function(source);
        }
    }
}