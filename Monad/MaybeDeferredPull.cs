using System;
using System.Collections.Generic;

namespace Monad.MaybeDeferredPull
{

    // --------------------------------------------------------------------------------------------
    // With deferred monads, the Bind function doesn't call the composed functions and evaluate the entire expression as it goes.
    // Instead the Bind function builds a structure of conencted objects which are like a program.
    // Then later you can run the program.
    // Deferred monads can work using either Push or pull. This is a Push version.
    // First you should be familiar with the conventional immediate version of Maybe in the Maybe.cs and perhaps the Deferred/Pull version in MaybeDeferredPull.cs.
    // We want to do a push version becasue I want to investigate the nuances of how a push version of the monad would work.
    // ALso ALA largely uses pushing programming paradigms, so this version will be closest to how ALA would do the same job.
    // This is a reactive extensions version of the Maybe monad.
    // The major difference between this monad and reactive extensions is that in reactive extension, subscribing causes the data to be pushed through.
    // In this monad, Subscribing just does the wiring, without starting the data flowing.
    // To start the data flowing, you call a Run method at the very source end of the chain.
    // ALA does wiring using a WireIn method that takes two ojects as parameters.
    // By contrast, monads are wired using a Bind method. The bind method creates the second object and then wires it in. 
    // The main monad type that Bind takes and returns is IMaybeObservable.
    // IMaybeObservable is implemented by sources.
    // Note that the interface that Bind uses to do the wiring, IMaybeObservable, goes in the opposite direction of the dataflow, which is what Bind functions always do.
    // Since we want to use push, we can't use IMaybeObservable as the interface for the actual dataflow becasue it would need to use pull.

    // So there needs to be a second interface involved with push actions in it, which is called IMaybeObserver.
    // IMaybeObserver is analogous to IObserver for the IObservable monad.
    // IMaybeObserver is only implemented by a destination.
    // IMaybeObserver goes in the direction of Dataflow.
    // When Bind runs, it wires the IMayebObserver interface.
    // IMaybeObservable has a single method called Subscribe that receives an IMaybeObserver.
    // Note: IMaybeObservable is only needed to support the Bind way of wiring things up. It is used only once to effect the wiring in the opposite direction.
    // As we will see later, the ALA WireIn operator needs interfaces that go in the same direction as the dataflow.
    // We support fanout (multiple subscribers), just because we can with deferred push monads.

    // Note that although IMaybeObservable is analogous to IObservable, and has the same method in it, Subscribe, unlike IObservable, the subscribe method does not cause the dataflow to start.
    // I find that is a weird aspect of iObservale that you start pushing data by first inititating it from the destination end.
    // To me a pushing paradigm initiates from the source. So that's the way this monad works. subcribe doesn't start the data flow, it just wires things up.
    // You start the actual dataflow from teh source.

    // The functions that can be composed with this monad take the form Action<T, IMaybeObserver<T>>. So they are not functions at all, they are Actions.
    // This is consistent with the fact that it is a push monad. We want the composed 'functions' need to push their result out, not return their result. 
    // Note that in the IObservableMonad, the Selectmany method (which is the Bind function) has two versions:
    // One takes a function of the form Func<T,IEnumerable<U>>. The Bind then has to use the IEnumerable to pull the values from the function. That seems a little weird given that we are in the middle of push style chain. 
    // The other takes a function of the form Func<T,IObservable<U>>. The Bind function will subscribe to the returned IObservable and then the function can push its values out to the observer.
    // Both of those methods leave the function a quite complicated thing. It has to create an object that implements either the IEnumerable or IObservable interface.
    // Why not allow the function to directly push its results?
    // To do that with IObserveable, you would want the composed function to actually be an Action. The action is called with two parameters, the normal input value of type T, and an IObserver. 
    // Now the function is simple again, it can just directly output its results to the IObserver.
    // That's the appoach we use in this implementation of the MaybeDeferredPush monad.
    // The 'functions' that you compose together take the form Action<T, IMaybeObserver<U>>



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
            return new ToMaybe<T>(value);
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
    // This class implements two interfaces: IMaybeObservable and IMaybeObserver.
    // IMaybeObservable is implemented by sources.
    // IMaybeOberver is implemented by destinations. So the two interfaces go in opposiate directions.
    // This is a chainable class so it implements both.
    // Bind is an extention method on IMaybeObservable. All it does is create this class and then wire the source to this class using the IMaybeObserver interface.
    // The IMaybeObservable interface is not used after that - it exists solely because Bind needs an interface that is implemented by the source.
    // It is the IMaybeObserver interface that holds the chain together and does the work at runtime.
    // Being a push interface, the IMaybeObserver is wired in the same direction as the dataflow.

    // Bind will pass to this class the source and the application action via the constructor.
    // In this monad, the application layer "functions" that are composed together are not functions but Actions. 
    // This comes about because of the pushing nature of the monad.
    // Strictly speaking, a monad composes functions, but I don't think this implementation detail stops it being thought of as a monad.
    // Note that pulling type monads compose functions which take a value and return an interface.
    // By contrast, pushing type monads compose Actions which take a value and an interface. The Action transforms the value in some way, and then calls whatever is apropriate on the interface to push the result out.
    // In this case, the Actions will call either the Value or the NoValue on the IMaybeOverver interface.
    // So the Bind function takes an IMaybeObservable and returns an IMaybeObservable, but unlike convention monads, the functions don't return an ImaybeObservable, instead they take an IMaybeObserver.
    // This Maybe class needs something that implements IMaybeObserver to pass to the the Action.
    // So we have a contained class for this purpose called ActionReceiver.
    // Once you understand all the above, this class is quite straightforward to follow.

    // Although this class has nothing to do with ALA (it is solely to support the monad version), note how similar it is to the ALA domain abstraction class.
    // It has an input "port", which is the implemented interface IMaybeObserver and an output port, the IMaybeObservable field.

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




    class ToMaybe<T> : IMaybe<T>
    {
        private T value;
        public ToMaybe(T value) { this.value = value; }

        bool IMaybe<T>.hasValue => true;

        T IMaybe<T>.Value => value;
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
