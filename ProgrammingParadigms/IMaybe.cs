namespace ProgrammingParadigms
{
    // used only by the ALA version
    // Actually its exactly the same as the monad push version.
    // Used as the main Programming Paradigm interface for the ports of the ALA version
    // This interface is implemented by the data destinations
    // The WireIn function wires up ports of this type. It wires them in the same direction as the dataflow
    public interface IMaybe<T>
    {
        void NoValue();
        void Value(T value);
    }
}
