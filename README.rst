Duckpond
==============
A .NET library to provide duck-typing capabilities. The extension method object.As<T> allows any object to be cast to an interface as long as it implements methods/properties with the same signature. The intended use is in mapping classes to interfaces they do not implement in Dependency Injection scenarios. 

Uses
================
- Create an interface around a class that does not implement interfaces for easier mocking.
- Create fine grained contracts on the dependency end rather than accepting everything a class exposes.


Missing Features
================
- Cannot remap methods with same type signature but different names
- Cannot remap parameter types to duck types along the way


Installation
============
Currently using the driver in the GAC is not supported.  Simply copy the driver assembly somewhere and reference it in your project.  It should be deployed in your application's bin directory.  It is not necessary to use the test assembly.

Patches
=======
Patches are welcome and will likely be accepted.  By submitting a patch you assign the copyright to me, Arne Claassen.  This is necessary to simplify the number of copyright holders should it become necessary that the copyright need to be reassigned or the code relicensed.  The code will always be available under an OSI approved license.

Usage
=====
Given a class ``Duck`` like this:

::

  public class Duck {
    public void Quack(double decibels) {
      ...
    }
    ... various other methods ...
  }

and you want to inject an instance into a class, but only care about the *Quack* capabilities:

::

  public interface IQuacker {
    void Quack(double decibels);
  }

you can simply cast ``Duck`` to ``IQuacker`` without making ``Duck`` implement the code:

::

  var quacker = new Duck().AsImplementationOf<IQuacker>();

Contributors
============
- Arne Claassen (sdether)


