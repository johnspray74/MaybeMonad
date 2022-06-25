using System;

namespace Monad.MinusOneDeferredPush
{
    public interface IMinusOneObserver
    {
        void Push(int value);
    }

    public interface IMinusOne
    {
        void Wire(IMinusOneObserver onserver);
    }




    public static class ExtensionMethod
    {
        public static IMinusOne ToMinusOneMonad(this int value)
        {
            return new MinusOneStart(value);
        }

        public static IMinusOne Bind(this IMinusOne source, Func<int, int> function)
        {
            return new MinusOne(source, function);
        }

        public static void ToOutput(this IMinusOne source, Action<int> action)
        {
            new MinusOneOutput(source, action);
        }
    }




    class MinusOneOutput : IMinusOneObserver
    {
        private Action<int> action;
        public MinusOneOutput(IMinusOne source, Action<int> action)
        {
            this.action = action;
            source.Wire(this);
        }

        void IMinusOneObserver.Push(int value)
        {
            action(value);
        }
    }





    class MinusOne : IMinusOne, IMinusOneObserver
    {
        private IMinusOneObserver observer;

        private Func<int, int> function;

        public MinusOne(IMinusOne source, Func<int, int> function)
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

        void IMinusOne.Wire(IMinusOneObserver observer)
        {
            this.observer = observer;
        }
    }




    class MinusOneStart : IMinusOne
    {
        private int value;
        private IMinusOneObserver observer;


        public MinusOneStart(int value) { this.value = value; }

        void IMinusOne.Wire(IMinusOneObserver observer)
        {
            this.observer = observer;
        }

        public void Run()
        {
            observer.Push(value);
        }
    }
}