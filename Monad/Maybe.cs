using System;
using System.Collections.Generic;

namespace Monad.Maybe
{
    // --------------------------------------------------------------------------------------------
    // Maybe monad.
    // This is a convnetional Maybe monad, its convnetional by using a pull interface, and by working immediately (not deferred or lazy). In other words the Bind evalutaes the composed application functions as it goes.  
    // The main interface is IMaybe.
    // IMaybe has two implementation classes, MaybeSomething for containing a value, and MaybeNothing for containing nothing.
    // The functions written in the application layer to be composed return either a new MaybeSomething or a new MaybeNothing.
    // IMaybe is a pulling interface a little like IEnumerable except it only pulls one value.
    // The Bind method operates on IMaybe and returns an IMaybe.
    // A ToMaybe extension method is provided to start a chain of Binds from a straight value.
    // e.g. 1.ToMaybe.Bind(first function).Bind(second Function)

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


    public static class MaybeMonadExtensionMethods
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





namespace Monad.PushMaybe
{

    // --------------------------------------------------------------------------------------------
    // Push and deferred version of Maybe monad.
    // Now that we are familar with the more conventional pull/immediate monad, we are going to do a push/deferred version of it
    // We do this because we want it as a stepping stone to compare with ALA.
    // ALA defaults to pushing (like reactive extensions) so this is like the reactive extension version of the Maybe monad.
    // ALA always uses deferred operation (wires up the program, and then runs it), so that's what we do with this monad.
    // In other words the Bind function builds a structure instead of executing the composed application functions as it goes.
    // (To implement an immediate version, the subscribe method would cause the source to send immediately)
    // The main interface is IPushMaybe. It is a pushing interface. So its implemented by the receiver of the data. IPushMaybe goes in the direction of Dataflow.
    // Bind needs an interface implemented by the source of the data. For that we use ISubscribeMaybe. It goes in the opposiate direction of the dataflow, which is what the Bind function always needs.
    // ISubscribeMaybe has a single method called Subscribe that receives an IPushMaybe.
    // (Note: ISubscribeMaybe is only needed for the Bind operator. As we will see later, the ALA WireIn operator needs interfaces that go in the same direction as the dataflow.)
    // (Note: ISubscribeMaybe is to IPushMaybe what IObservable is to IObeserver. It's needed by the Bind funtion. IObservable is also implemented by the data source classes and IObserver by the data destination classes. IObservable has a Subscribe method which takes an IObserver.
    // In other words IObservers are wired in the direction of dataflow, but Bind always needs interfaces that go in the opposite direction of the dataflow.
    // We support multiple subscribers, just because we can.



    // This is teh main interface used by the monad
    // This interface is implemented by any data destination class
    public interface IPushMaybe<T>
    {
        void NoValue();
        void Value(T value);
    }


    // This interface is implemented by any data source class
    // It is only needed to support the Bind function
    public interface ISubscribeMaybe<T>
    {
        void Subscribe(IPushMaybe<T> subscriber);
    }





    // extension methods for the PushMonad

    public static class PushMaybeMonadExtensionMethods
    {
        public static ISubscribeMaybe<T> ToMaybe<T>(this T value)
        {
            return new ValueToMaybe<T>(value);
        }


        public static ISubscribeMaybe<U> Bind<T, U>(this ISubscribeMaybe<T> source, Action<T, IPushMaybe<U>> action)
        {
            var maybe = new Maybe<T, U>(action);
            source.Subscribe(maybe);   // subscribes the IPushMaybe<T> interface of the object
            return maybe;              // returns the ISubscribeMaybe<U> interface of the object, which allows the next Bind function to use it. 
        }


        public static void ToValue<T>(this ISubscribeMaybe<T> source, Action<T> valueAction, Action noValueAction)
        {
            source.Subscribe(new MaybeToValue<T>(valueAction, noValueAction));
        }

    }




    // This is the class that will be used by Bind to implement the monad.
    // A class is needed becasue the monad is deferred, so this class suports the structure that will be built.
    // Bind will pass it the application action in the constructor
    // Although this class has nothing to do with ALA (it is solely to support the monad version), note how similar it is to an ALA domain abstraction class.
    // It has an input "port", which is the implemented interface IPushMaybe and an output port, the List field.

    // In this monad, the application "functions" which are composed are not function but Actions. 
    // This comes about because of the pushing nature of the monad.
    // Strictly speaking, a monad composes functions, but I don't think this implementation detail stops it being classified as a monad.
    // Note that pulling type monads compose functions which take a value and return an interface.
    // By contrast, pushing type monads compose Actions which take a value and an interface. The Action transforms the value and some way, and then calls whatever is apropriate on the interface.
    // In this case, the Actions will call either the Value or the NoValue on the interface.
    // Ok so this PushMaybe class needs to create an object that implements the interface that's passed to the Action.
    // The class used for this object is internal to this PushMaybe class and is called ActionReceiver.
    // Once you understand all this, this class should be quite straightforward to follow.

    class Maybe<T,U> : IPushMaybe<T>, ISubscribeMaybe<U>
    {
        // implement the constructor, which receives the Action function
        Action<T, IPushMaybe<U>> action;
        public Maybe(Action<T, IPushMaybe<U>> action) { this.action = action; }



    
        // impolement the ISubscribeMaaybe interface
        List<IPushMaybe<U>> subscribers = new List<IPushMaybe<U>>();
        void ISubscribeMaybe<U>.Subscribe(IPushMaybe<U> subscriber)
        {
            subscribers.Add(subscriber);
        }



        // Implement the IPushMaybe interface
        void IPushMaybe<T>.NoValue()
        {
            // Don't even call the Action, just pass the NoValue straight through to the subscribers
            foreach (var subscriber in subscribers)
            {
                subscriber.NoValue();
            }
        }


        void IPushMaybe<T>.Value(T value)
        {
            // Weve been give a value
            // Call the action, giving the value and an interface for it to output its result
            // For the interface we need to provide an object implementing the IPushMaybe Interface
            action(value, new ActionReceiver<T,U>(this));
        }


        // This class is used to create an object implementing the IPushMaybe interface to receive the output from the action.
        private class ActionReceiver<T,U> : IPushMaybe<U>
        {
            // First we need to keep a reference to the outer object because we need to access the subscribers
            Maybe<T, U> outer;
            public ActionReceiver(Maybe<T, U> outer) { this.outer = outer; }



            // Implement the Ipushmaybe interface
            void IPushMaybe<U>.NoValue()
            {
                foreach (var subscriber in outer.subscribers)
                {
                    subscriber.NoValue();
                }
            }

            void IPushMaybe<U>.Value(U value)
            {
                foreach (var subscriber in outer.subscribers)
                {
                    subscriber.Value(value);
                }
            }
        }

    }



    class ValueToMaybe<T> : ISubscribeMaybe<T>
    {
        T value;
        List<IPushMaybe<T>> subscribers = new List<IPushMaybe<T>>();

        public ValueToMaybe(T value) { this.value = value; }



        public void Run()
        {
            foreach (var subscriber in subscribers)
            {
                subscriber.Value(value);
            }
        }



        void ISubscribeMaybe<T>.Subscribe(IPushMaybe<T> subscriber)
        {
            subscribers.Add(subscriber);
        }
    }



    class MaybeToValue<T> : IPushMaybe<T>
    {
        Action<T> valueAction;
        Action noValueAction;

        public MaybeToValue(Action<T> valueAction, Action noValueAction) { this.valueAction = valueAction; this.noValueAction = noValueAction;  }


        void IPushMaybe<T>.NoValue()
        {
             noValueAction();
        }

        void IPushMaybe<T>.Value(T value)
        {
            valueAction(value);
        }
    }














}
