using Monad;
using Foundation;
using System;
using System.Threading.Tasks;

namespace DomainAbstractions
{
/*

    public class Maybe<T, U> : IMaybeable, IMaybe<T> // input port
    {
        readonly Func<T, Task<U>> function;
#pragma warning disable CS0649 // Field 'Continuation<T, U>.next' is never assigned to, and will always have its default value null
        private IMaybe<U> next; // output port
#pragma warning restore CS0649

        public Maybe(Func<T, Task<U>> function)
        {
            this.function = function;
        }

        bool IMaybe<T>.hasValue => throw new NotImplementedException();

        T IMaybe<T>.Value => throw new NotImplementedException();
    }


    public static class BindExtensionMethod
    {

        public static IMaybe<U> Bind<T, U>(this IMaybe<T> source, Func<T, Task<U>> function)
        {
            return (IMaybe<U>)source.WireIn(new Maybe<T, U>(function));
        }
    }
*/

}