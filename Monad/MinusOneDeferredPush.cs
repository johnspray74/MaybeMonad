using System;

namespace Monad.MinusOneDeferredPush
{
    public interface IMinusOneObservable
    {
        void Wire(IMinusOneObserver onserver);
    }

    public interface IMinusOneObserver
    {
        void Push(int value);
    }




    public static class ExtensionMethod
    {
        public static IMinusOneObservable ToMinusOneMonad(this int value)
        {
            return new MinusOneStart(value);
        }

        public static IMinusOneObservable Bind(this IMinusOneObservable source, Func<int, int> function)
        {
            return new MinusOne(source, function);
        }

        public static void ToOutput(this IMinusOneObservable source, Action<int> action)
        {
            new MinusOneOutput(source, action);
        }
    }




    class MinusOneOutput : IMinusOneObserver
    {
        private Action<int> action;
        public MinusOneOutput(IMinusOneObservable source, Action<int> action)
        {
            this.action = action;
            source.Wire(this);
        }

        void IMinusOneObserver.Push(int value)
        {
            action(value);
        }
    }





    class MinusOne : IMinusOneObservable, IMinusOneObserver
    {
        private IMinusOneObserver observer;

        private Func<int, int> function;

        public MinusOne(IMinusOneObservable source, Func<int, int> function)
        {
            source.Wire(this);
            this.function = function;
        }

        void IMinusOneObserver.Push(int value)
        {
            if (value == -1)
            {
                observer.Push(-1);
            }
            else
            {
                observer.Push(function(value));
            }
        }

        void IMinusOneObservable.Wire(IMinusOneObserver observer)
        {
            this.observer = observer;
        }
    }




    class MinusOneStart : IMinusOneObservable
    {
        private int value;
        private IMinusOneObserver observer;


        public MinusOneStart(int value) { this.value = value; }

        void IMinusOneObservable.Wire(IMinusOneObserver observer)
        {
            this.observer = observer;
        }

        public void Run()
        {
            observer.Push(value);
        }
    }
}