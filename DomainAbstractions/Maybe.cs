using ProgrammingParadigms;
using Foundation;

using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace DomainAbstractions
{
    public static class ExtensionMethods
    {
        public static IBindable<T> ToMaybe<T>(this T value)
        {
            return new ToMaybe<T>(value);
        }

        public static IBindable<U> Bind<T, U>(this IBindable<T> source, Action<T, IMaybe<U>> action)
        {
            return (IBindable<U>)source.WireIn(new Maybe<T, U>(action));
        }

        public static void ToOutput<T>(this IBindable<T> source, Action<T> valueAction, Action noValueAction)
        {
            source.WireTo(new ToOutput<T>(valueAction, noValueAction));
        }
    }







    // Note that this class is almost identical to the one used by non ALA Monad implementation. The only difference is that the ISubscribeMonad interface doesn't contain a Subscribe method.
    // The Bind function calls WireIn(new Monad(action) instead, to show how the Bind function is built on top of the normal ALA WireIn method to make the application syntax look exactly like the monad version.
    // Normally we would not use a Bind function at all.

    public class Maybe<T, U> : IMaybe<T>, IBindable<U>  // input port
    {
        // first do the constructor, which is called by the Bind function and receives an Action to be composed
        Action<T, IMaybe<U>> action;
        public Maybe(Action<T, IMaybe<U>> action) { this.action = action; }



        // #pragma warning disable CS0649 // Field 'subscribers' is never assigned to, and will always have its default value null
        List<IMaybe<U>> subscribers = new List<IMaybe<U>>();  // output port
        // #pragma warning restore CS0649




        // Implement the IMaybe interface
        void IMaybe<T>.NoValue()
        {
            // Don't even call the Action, just pass the NoValue straight through to the subscribers
            foreach (var subscriber in subscribers)
            {
                subscriber.NoValue();
            }
        }


        void IMaybe<T>.Value(T value)
        {
            // We've been give a value from the previous monad
            // Call the action, giving the value and an interface for it to output its result
            // For the interface we need to provide an object implementing the IMaybe Interface
            action(value, new ActionReceiver<T, U>(this));
        }


        // This class is used to create an object implementing the IMaybe interface to receive the output from the action.
        private class ActionReceiver<T, U> : IMaybe<U>
        {
            // First we need to keep a reference to the outer object because we need to access the subscribers
            Maybe<T, U> outer;
            public ActionReceiver(Maybe<T, U> outer) { this.outer = outer; }



            // Implement the Ipushmaybe interface
            void IMaybe<U>.NoValue()
            {
                foreach (var subscriber in outer.subscribers)
                {
                    subscriber.NoValue();
                }
            }

            void IMaybe<U>.Value(U value)
            {
                foreach (var subscriber in outer.subscribers)
                {
                    subscriber.Value(value);
                }
            }
        }
    }



}