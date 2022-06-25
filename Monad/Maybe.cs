using System;
using System.Collections.Generic;

namespace Monad.Maybe
{
    // --------------------------------------------------------------------------------------------
    // Maybe monad.
    // This is a convnetional Maybe monad, its convnetional by being immediate (not deferred or lazy).
    // In other words the Bind evalutaes the composed application functions as it goes.  
    // The monad type, which Bind takes and returns, and which the composed functions return is IMaybe interface.
    // IMaybe has two implementation classes, MaybeSomething for containing a value, and MaybeNothing for containing nothing.
    // The functions written in the application layer to be composed return either a new MaybeSomething or a new MaybeNothing.
    // A ToMaybe extension method is provided to start a chain of Binds from a straight value.
    // e.g. 42.ToMaybe.Bind(first function).Bind(second Function)
    // At the ouput end of the chain, you have an IMaybe interface from which you can first see if there is a value and then get the value if there is one.

    public interface IMaybe<T>
    {
        bool hasValue { get; }
        T Value { get; }
    }




    public class MaybeNothing<T> : IMaybe<T>
    {
        bool IMaybe<T>.hasValue { get => false; }
        T IMaybe<T>.Value { get { throw new Exception("No value"); } }
    }




    public class MaybeSomething<T> : IMaybe<T>
    {
        T value;

        public MaybeSomething(T value) { this.value = value; }

        bool IMaybe<T>.hasValue { get => true; }
        T IMaybe<T>.Value { get => value; }
    }




    public static class ExtensionMethods
    {
        public static IMaybe<T> ToMaybe<T>(this T value)
        {
            return new MaybeSomething<T>(value);
        }


        public static IMaybe<U> Bind<T, U>(this IMaybe<T> source, Func<T, IMaybe<U>> function)
        {
            return source.hasValue ? function(source.Value) : new MaybeNothing<U>();
        }
    }
}