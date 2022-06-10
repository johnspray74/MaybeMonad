using ProgrammingParadigms;
using System.Collections.Generic;

namespace DomainAbstractions
{
    class ToMaybe<T> : IBindable<T>
    {
        // first do the constructor, which is called by the Bind function and receives an Action to be composed
        private T value;
        public ToMaybe(T value) { this.value = value; }

        private List<IMaybe<T>> outputs = new List<IMaybe<T>>(); // output port


        public void Run()
        {
            foreach (var output in outputs)
            {
                output.Value(value);
            }
        }


    }
}