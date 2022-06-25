using System;
using System.Collections.Generic;

namespace Monad.MaybeDeferredPull
{

    // --------------------------------------------------------------------------------------------
    // With deferred monads, the Bind function doesn't call the composed functions and evaluate the entire expression as it goes.
    // Instead the Bind function builds a structure of conencted objects which are like a program.
    // Then later you can run the program.
    // Deferred monads can work using either Push or pull. This is a Pull version.
    // First you should be familiar with the conventional immediate version of Maybe in the Maybe.cs.
    // To start the data flowing, you call hasValue and value at the end of the chain.
    // ALA does wiring using a WireIn method that takes two ojects as parameters.
    // By contrast, monads are wired using a Bind method. The bind method creates the second object and then wires it in. 
    // The main monad type that Bind takes and returns is IMaybe.
    // IMaybe is implemented by sources.
    // Note that the interface that Bind uses to do the wiring, IMaybe, goes in the opposite direction of the dataflow, which is what Bind functions always do.
    // IMaybe is also the interface used for the actual dataflow becasue using pull.




    // This interface is main monad type
    // The Bind function takes this interface and returns this interface
    // It is also the interface used at runtime to pull through the data
    // This is the same interface used by the immediate version of the monad
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





    // extension methods for the PullMonad
    public static class ExtensionMethods
    {
        public static IMaybe<T> ToMaybe<T>(this T value)
        {
            return new MaybeSomething<T>(value);
        }

        public static IMaybe<U> Bind<T, U>(this IMaybe<T> source, Func<T, IMaybe<U>> function)
        {
            return new Maybe<T, U>(source, function);
        }

        public static ToOutput<T> ToOutput<T>(this IMaybe<T> source, Action<string> output)
        {
            return new ToOutput<T>(source, output);
        }
    }




    // This is the class used by Bind to implement the monad.
    // A class is needed because the monad is deferred, so objects of this class need to be chained together to build a program that can be run later.
    // This class implements the IMaybe interface.

    // IMaybe is implemented by sources.
    // Bind is an extention method on IMaybe. All it does is create this class and pass to this class the source and the application function via the constructor.
    // The source field is the reference to the previous, so wiring involves setting the source field.

    class Maybe<T, U> : IMaybe<U>
    {
        // implement the constructor, which receives the Action function
        private Func<T, IMaybe<U>> function;
        private IMaybe<T> source;
        private IMaybe<U> result;

        public Maybe(IMaybe<T> source, Func<T, IMaybe<U>> function) { this.source = source; this.function = function; }

        bool IMaybe<U>.hasValue 
        { get 
            {
                if (result == null)
                {
                    if (source.hasValue)
                    {
                        result = function(source.Value);
                    }
                    else
                    {
                        return false;
                    }
                }
                return result.hasValue;
            }
        }

        U IMaybe<U>.Value
        {
            get
            {
                if (result == null)
                {
                     result = function(source.Value);  // might throw exception
                }
                return result.Value; // might throw exception
            }
        }
    }






    public class ToOutput<T>
    {
        private IMaybe<T> source;
        private Action<string> output;

        public ToOutput(IMaybe<T> source, Action<string> output) { this.source = source; this.output = output; }


        public void Run()
        {
            if (source.hasValue) output(source.Value.ToString());
            else output("no value");
         }
    }
}
