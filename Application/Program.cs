
#define PushMonad
// #define ALA              // Selects ALA version designed to run identical application layer

// This code is wrtten as example code for chapter 3 section on moands in the online book at abstractionlayeredarchitecture.com
// See that website for full discussion of the comparison between ALA and monads

#if ALA
using DomainAbstractions;
using ProgrammingParadigms;
using Foundation;
#else
#if PushMonad
using Monad.PushMaybe;
#else
using Monad.Maybe;
#endif
#endif
using System;

namespace Application
{
    class Program
    {
        // Main just uses the Nito.AsyncEx package to proovde a dispatcher to enable console programs to use async/await on one thread.
        // It just calls another function called Application
        // (if you don't do this then Main will either complete immediately (ending the program before the asyncronous tasks finish)
        // or if you put in a ConsoleReadKey or Thread.Sleep at the end of Main, that will just block the main thread, causing the asynchronous tasks to run on other threads.
        // The program still works when it uses other threads, but I particular wanted to demonstrate this monad working on a single thread.


        static int Main()
        {
            try
            {
                Console.WriteLine("The application has started");
                Application();
                Console.WriteLine("The application has finished");
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return -1;
            }
        }



        // The application function composes two functions using the Continuation monad.
        // The Continuation monad consists of Task<T> plus the Totask extension method plus the Bind extension method (both of which are defined in the programming paradigms layer folder)
        // First it creates a source Task with the value 1. Then the first function adds 2, then the second function adds a number from the console. 
        // The thing is, because we are using the Coninuation monad, these two functions are allowed to take as much time as they want (be asynchronous).
        // To demo that, the first function does a delay, and the second function waits for Console input.
        // What the monad does is, despite these functions taking time and returning a Task object instead of an immediate result, allows you to compose them as if they were functions that just return a result. You just have to use the Bind function to compose them.
        // And the function must return Task<U> instead of U.
        // The monad code in the programming paradigms layer takes care of making everything work by providing two extension methiods, Bind() and ToTask().
        // There is no blocking of the main thread in this program until it hits the final ReadKey.
        // Everything runs on a single thread.

        // There are two versions - the first version uses async/await. If you are not familiar with async/await then use the second version which uses ContinueWith instead.
        // Each version has a verbose version that Console.WriteLines stuff to see what is going on.

#if !ALA

#if !PushMonad


        // This is a simple sample application that uses the typical implemention of maybe Monad.

        static void Application()
        {
            var result = 1.ToMaybe()
            .Bind(x => new MaybeSomething<int>(x + 2))
            .Bind<int, double>((x) =>
             {
                 if (x == 0) return new MaybeNothing<double>();
                 else return new MaybeSomething<double>((double)1 / x);
             });
            Console.WriteLine($"Final result is {(result.hasValue ? result.Value : "nothing") }.");
        }

#else // PushMaybe

        // This is the same simple application as above, but uses a push, deferred implementation of the Maybe monad.
        // This version of the Maybe monad is closer to how ALA works, becasue ALA uses deferred execution, and we like to default to pushing data (like reactive extensions) and only pull when it makes sense to pull.


        static void Application()
        {
            var program = 1.ToMaybe();  // Because the monad uses pushing, we keep a reference to the beginning of the chain rather than the end.

            program.Bind<int, int>((x, i) => { i.Value(x + 2); })    // x is the value passed to us. i is the interface we use to push back the result
            .Bind<int, double>((x, i) =>
             {
                 if (x == 0) i.NoValue();
                 else i.Value((double)1 / x);
             })
            .ToValue(
                (x)=>Console.WriteLine($"Final result is {x}."),            // This runs if the source produces a value
                ()=>Console.WriteLine($"Final result is {"nothing"}.")      // This runs if the source produces a noValue
                );

            ((ValueToMaybe<int>)program).Run();   // program is a ISubscribeMaybe, so we need to cast back to ValueToMaybe so we can call its Run method 
        }



#endif


#else // ALA


#endif


    }

}



