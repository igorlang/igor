********************
   New in Igor 2.1
********************

Igor Compiler
=============

* **[Change]** Switch from Nitra parser to custom parser
* **[New]** Introduce problem codes
* **[New]** Allow to use compiled script assemblies. That allows to debug generator scripts.

TypeScript Generator
=====================

* **[New]** ``error_message`` attribute to match igor exception field with TypeScript Error field

Erlang Generator
================

* **[Fix]** Fix ``json.nulls`` for variants

*********************
   New in Igor 2.1.1
*********************

Igor Compiler
=============

* **[Fix]** Fix parsing identifiers starting with ``_``

Erlang Generator
=================

* **[Fix]** Fix service type specs
* **[Fix]** Fix ``interface_records`` attribute behavior

C# Generator
=============

* **[Fix]** Allow ``[csharp namespace=""]`` for no namespace
* **[Improvement]** ``tpl`` attribute to control if TPL tasks are used in generated code

UE4 Generator
==============

* **[Improvement]** Attributes to configure how HTTP client request variables are set up
* **[Improvement]** ``http.base_url`` attribute
* **[Improvement]** Document UE4 HTTP clients

TypeScript Generator
====================

* **[Fix]** Fix serialization of error messages
* **[Improvement]** Document TypeScript exceptions

Lua Generator
==============

* **[Fix]** Record fields are set to default values during deserialization if JSON object values are not present

*********************
   New in Igor 2.1.2
*********************

Elixir Generator
=================

* **[New]** First version of Elixir generator

C# Generator
=============

* **[New]** Support for JSON service message serialization

UE4 Generator
==============

* **[Improvement]** ``h_path`` and ``cpp_path`` attributes (useful for Private/Public folder layout)
* **[Improvement]** ``api_macro`` attribute for UE4 module API macro
* **[Improvement]** support for UMETA attribute for UENUM fields

*********************
   New in Igor 2.1.3
*********************

Igor Compiler
=============

* **[Improvement]** Document debugging extension scripts with Visual Studio
* **[Improvement]** Document problem codes
* **[Improvement]** Extension scripts can now define new targets by implementing ``ITarget`` interface (see Dump sample)

Schema
======

* **[Improvement]** Add primitive type information
* **[Improvement]** Allow to override root type via command line
* **[Fix]** Collect and merge scoped meta attributes

Elixir Generator
================

* **[Improvement]** Support for unions
* **[Improvement]** Support for text data format in web services

TypeScript Generator
====================

* **[Fix]** Fix interface inheritance

JavaScript Generator
====================

* **[New]** First version of JavaScript generator

UE4 Generator
==============

* **[Improvement]** Generate interfaces (controlled by ``interfaces`` attribute)
  
C# Generator
=============

* **[Fix]** Fix default values for define types

*********************
   New in Igor 2.1.4
*********************

Igor Compiler
=============

* **[New]** Introduce Compiler commands (:ref:`compiler_commands`)
* **[New]** Postman sample target for generating Postman collections
* **[New]** Support input from standard input and output through standard output
