.. _services:

******************
     Services
******************

Services are used to describe client-server protocols, for example network protocols. Client and server can 
exchange messages of two types: remote procedure *casts* and remote procedure *calls*. 

Casts are fire and forget: there's no result or confirmation returned. Calls may return results and/or throw
exceptions.

Syntax
======

.. code-block:: igor

    service ServiceName
    {
        c->s ClientToServerCast(ArgumentType argumentName, ...);
        c->s ClientToServerCall(ArgumentType argumentName, ...) returns (ResultType resultName, ...) throws Exception1, Exception2, ...;
        
        s->c ServerToClientCast(ArgumentType argumentName, ...);
        s->c ServerToClientCall(ArgumentType argumentName, ...) returns (ResultType resultName, ...) throws Exception1, Exception2, ...;
    }

where

* ``ServiceName`` is the service name identifier
* ``ClientToServerCast`` etc. is the name of the function/message
* ``ArgumentType`` and ``argumentName`` are type and name of function arguments
* ``ResultType`` and ``resultName`` are type and name of return arguments
* ``ExceptionX`` are exception type names that can be thrown

Both **return** and **throws** are optional. If none of them is present, the function is a cast, otherwise it is a call.

There can be an arbitrary number of request and return arguments (comma-separated).

**c->s** and **s->c** tokens denote client-to-server and server-to-client messages.
