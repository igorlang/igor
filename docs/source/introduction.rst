************
Introduction
************

What is Igor?
=============

Igor is the common name for the language, serialization format and tooling for defining the data types and network services.

Igor provides:

* :ref:`language` - a language used to define data types and services
* :ref:`binary` - a binary serialization format and network protocol
* :ref:`compiler` - a code generator tool that generates target language code and schema files
* Target language runtime libraries

Igor supports multiple target languages, including C#, Erlang, TypeScript, C++ (UE4), Lua, though not all features are supported on all platforms. 

Igor supports several serialization formats, including Json, binary (**Igor Binary Protocol**) and XML.

Igor was initially developed by Artplant for the initial purpose of describing game protocols between Unity3D/C# game clients and game servers 
written in Erlang or C#.

Design Goals
============

Extensibility
-------------

Igor is designed to be easily extensible. More target languages and serialization formats may be added.

Customization
-------------

Igor is highly customizable via attribute usage. Attributes allow to:

* Reuse data types existing in target language (for instance Vector3 definition may use **UnityEngine.Vector3**' or **System.Numerics.Vector3** type).
* Provide custom serialization for any type
* Control many aspects of generated type, depending on target language (for instance, in C# you can control naming,
  class or struct generation, class modifiers, attributes, Equals/GetHashCode/equality code generation, and much more)

and a lot more.

Scriptability
-------------

Custom attributes may be defined and code generation may be controlled with C# scripts. Scripts may alter target language model before 
any text code files are generated, which allows to control any aspects of target code generation without worrying of low level aspects
like code formatting.

Rich Type System
----------------

Igor language aims to provide the rich type system. Alias types, record inheritance via variant types, multiple inheritance 
via interfaces, generic types are supported and whenever possible are reflected by code generated target language.

Human-readable code
-------------------

Igor aims to generate the code that a human would write, with respect to target language's commonly accepted style guidelines 
and naming conventions.

Limitations
===========

Igor is not designed to allow applications to support communication between several versions of the protocol.
For example, when  client with the old protocol connects to the server with newer protocol, this should be detected and client 
should be updated.

Not supporting several versions of protocol allows binary Igor protocol to be more compact and efficient, but if this feature is 
required, alternatives like Google Protobuf should be considered.

Acknowledgements
================

Igor is named after its original inventor Igor Timurov. 

----------------

There're several products that provide functionality similar to Igor, each with their own features, benefits and limitations. 
Some of them are:

* Google Protobuf (https://developers.google.com/protocol-buffers/)
* Apache Thrift (https://thrift.apache.org/)
* Microsoft Bond (https://github.com/Microsoft/bond)
