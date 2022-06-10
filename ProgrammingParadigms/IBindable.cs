namespace ProgrammingParadigms
{
    // Used only by the ALA version, and only needed to support the Bind function to make the ALA version look like a monad

    // The IBindable interface is only used as the type for the Bind function. (The interface itself is empty)
    // Its called IBindable because a domain abstraction implementing it can be Wire using Bind (insead of using WireIn)
    // Put it on all domain abstraction classes that are sources, that is can be wired using Bind.
    // For example Both the Maybe and ValueToMybe classes use this interface.
    // The IMaybe interface is the one actually used to implement the ALA programming paradigm for use as a port on the domain abstractions. IMaybe is used for wiring with WireTo or WireIn.
    // If instead of .Bind(function) you always use the longer full ALA syntax ".WireIn(new Maybe(action))" then you could delete the Bind function and this IBindable interface.
    // We can consider using IBindable as the main programming paradigm interface, and delete IMaybe.
    // There are two disadvantes:
    // 1) The domain abstraction ports would be reversed. That is WireIn would wire in the opposite direction of the dataflow - this would be confusing.
    // 2) The data destination end of the chain would be where you start the program - it will pull the IMaybes through from the source. Or you could use C# events but that is even more inconvenient.
    // So IMaybe is the correct interface to use as the programming paradigm.
    // As I say, IBindable is only needed as the interface on which the Bind extension method is defined to replicate the exact syntax of monads in the aplication layer code.
    // We normally wouldn't use Bind in ALA. We are just showing that ALA is versatile enough to compose functions with the same syntax as monads.
    // Normally in ALA we would just use WireIn instead of Bind, and have more specialized domain abstractions, just as monad programming doesn't often use Bind directly, but uses other more specific composing functions such as  than the class Maybe() which takes a function as its configuration.
    // The ALA domain abstraction objectw would not require you to have so much implemenation detail in the lambda expression code that is in the application layer.
    // In other words the abstraction level of Domain abstractions can be a little more specific, taking some of the burden off the Application code, so it doesn't always need any actual code like lambda expressions.
    // See the abtractionlayerarchitecture.com online book chapter 3 section on monads for a discussion and comparison of the monad versus domain abstraction philosophies.

    public interface IBindable<T>
    {
    }
}
