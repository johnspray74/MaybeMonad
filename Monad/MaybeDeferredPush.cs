using System;
using System.Collections.Generic;

namespace Monad.MaybeDeferredPush
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
    // 
    public interface IMaybeObservabe<T>
    {
        void Subscribe(IMaybeObserver<T> subscriber);
    }


    // This is a secondary interface used by the monad
    // This interface is implemented by any data destination class
    // It is analogous to IObserver fro the IObservable monad.
    public interface IMaybeObserver<T>
    {
        void NoValue();
        void Value(T value);
    }





    // extension methods for the PushMonad

    public static class ExtensionMethods
    {
        public static IMaybeObservabe<T> ToMaybe<T>(this T value)
        {
            return new ToMaybe<T>(value);
        }

        public static IMaybeObservabe<U> Bind<T, U>(this IMaybeObservabe<T> source, Action<T, IMaybeObserver<U>> action)
        {
            var maybe = new Maybe<T, U>(action);
            source.Subscribe(maybe);   // subscribes the IPushMaybe<T> interface of the object
            return maybe;              // returns the ISubscribeMaybe<U> interface of the object, which allows the next Bind function to use it. 
        }

        public static void ToOutput<T>(this IMaybeObservabe<T> source, Action<T> valueAction, Action noValueAction)
        {
            source.Subscribe(new ToOutput<T>(valueAction, noValueAction));
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

    // Bind will pass to this class application action via the constructor.
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

    class Maybe<T, U> : IMaybeObserver<T>, IMaybeObservabe<U>
    {
        // implement the constructor, which receives the Action function
        private Action<T, IMaybeObserver<U>> action;
        public Maybe(Action<T, IMaybeObserver<U>> action) { this.action = action; }

        // implement the IMaybeObservable interface
        // This interface allows the next object in the chain to wire to us.
        private List<IMaybeObserver<U>> subscribers = new List<IMaybeObserver<U>>();
        void IMaybeObservabe<U>.Subscribe(IMaybeObserver<U> subscriber)
        {
            subscribers.Add(subscriber);
        }

        // Implement the IMaybeObserver interface
        // This interface receives data from the previous object in the chain
        void IMaybeObserver<T>.NoValue()
        {
            // Don't even call the Action, just pass the NoValue straight through to the subscribers
            foreach (var subscriber in subscribers)
            {
                subscriber.NoValue();
            }
        }

        void IMaybeObserver<T>.Value(T value)
        {
            // Weve been give a value
            // Call the action, giving the value and an interface for it to output its result
            // For the interface we need to provide an object implementing the IMaybe Interface
            action(value, new ActionReceiver<T, U>(this));
        }


        // This class is used to create an object implementing the IMaybeObserver interface to receive the output from the action.
        private class ActionReceiver<T, U> : IMaybeObserver<U>
        {
            // First we need to keep a reference to the outer object because we need to access the subscribers
            private Maybe<T, U> outer;
            public ActionReceiver(Maybe<T, U> outer) { this.outer = outer; }

            // Implement the Ipushmaybe interface
            void IMaybeObserver<U>.NoValue()
            {
                foreach (var subscriber in outer.subscribers)
                {
                    subscriber.NoValue();
                }
            }

            void IMaybeObserver<U>.Value(U value)
            {
                foreach (var subscriber in outer.subscribers)
                {
                    subscriber.Value(value);
                }
            }
        }

    }




    class ToMaybe<T> : IMaybeObservabe<T>
    {
        private T value;
        public ToMaybe(T value) { this.value = value; }

        private List<IMaybeObserver<T>> subscribers = new List<IMaybeObserver<T>>();
        void IMaybeObservabe<T>.Subscribe(IMaybeObserver<T> subscriber)
        {
            subscribers.Add(subscriber);
        }

        public void Run()
        {
            foreach (var subscriber in subscribers)
            {
                subscriber.Value(value);
            }
        }
    }





    class ToOutput<T> : IMaybeObserver<T>
    {
        private Action<T> valueAction;
        private Action noValueAction;

        public ToOutput(Action<T> valueAction, Action noValueAction) { this.valueAction = valueAction; this.noValueAction = noValueAction; }

        void IMaybeObserver<T>.NoValue()
        {
            noValueAction();
        }

        void IMaybeObserver<T>.Value(T value)
        {
            valueAction(value);
        }
    }
}
