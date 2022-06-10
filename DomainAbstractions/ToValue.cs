using ProgrammingParadigms;
using System;

namespace DomainAbstractions
{
    class ToValue<T> : IMaybe<T>  // input port
    {
        private Action<T> valueAction;
        private Action noValueAction;

        public ToValue(Action<T> valueAction, Action noValueAction) { this.valueAction = valueAction; this.noValueAction = noValueAction; }

        void IMaybe<T>.NoValue()
        {
            noValueAction();
        }

        void IMaybe<T>.Value(T value)
        {
            valueAction(value);
        }
    }

}