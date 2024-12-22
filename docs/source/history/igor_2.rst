********************
   New in Igor 2.0
********************

**[New]** Documentation
=======================

* Initial release of this documentation

Igor Language
=============

* Experimental *union* type, see :ref:`unions`
* Webservices support, see :ref:`webservices` for details

Igor Compiler
=============

* **[Improvement]** Show warnings for unknown attributes
* **[Improvement]** Command line options for listing supported targets and attributes
* **[Drop]** Remove support for compiling for several targets at once
* **[Drop]** Remove *global namespace* option

C# Generator
============

* **[New]** Webservices (client only)
* **[New]** C# 5.0+ features
* **[Change]** Significant refactor of services

Erlang Generator
================

* **[New]** Webservices 
* **[New]** Union types 
* **[New]** ``x-www-form-urlencoded`` encoding
* **[Change]** Significant refactor and enhacement of XML serialization
* **[Improvement]** Allow to generate records into Erlang records, maps or tuples

**[New]** TypeScript Generator
==============================

* Records, variants, enums, interfaces and alias types
* JSON serialization
* Webservice clients (JSON only)

**[New]** Lua Generator
=======================

* Records, variants, enums and alias types
* JSON serialization
* Services (JSON only)

Schema
======

* **[Improvement]** Several new attributes, including *compact* mode and *path* options

Tools
=====

* **[New]** Initial release of Visual Studio Code plugin (supports only subset of Igor language)
* **[Drop]** Support of Visual Studio is temporary removed

**********************
   New in Igor 2.0.1
**********************

Documentation
=============

* Multiple documentation improvements

C# Generator
============

* Support for C# 4.0 (no TPL)

UE4 Generator
=============

* **[New]** Support for multiple UE4 UPROPERTY and USTRUCT specifiers
* Multiple fixes

**[New]** Go Generator
======================

* Support records, variants & JSON marshalling

**********************
   New in Igor 2.0.2
**********************

Igor Language
=============

* **[Change]** Change union syntax
* **[New]** Annotations (documentation comments) syntax

Igor Compiler
=============

* **[New]** ``mkdir`` option allows to create output path

XSD Schema Generation
=====================

* **[New]** Document XSD target
* **[New]** Generate annotations
* **[Improvement]** Multiple fixes and improvements in XSD generation

Erlang Generator
================

* **[New]** Generate comments from Igor annotations

Go Generator
============

* **[New]** Document Go target

TypeScript Generator
======================

* **[Improvement]** Webservices: support multiple status codes
* **[Improvement]** Fixes and improvements in webservice generation

Schema
======

* **[New]** *group* attribute for category groups
* **[Change]** Deprecate *help* attribute, use annotations instead
* **[Improvement]** Document *path* attributes

Tools
=====

* **[Improvement]** Visual Studio Code plugin: support annotations and webservices

**********************
   New in Igor 2.0.3
**********************

Igor Language
=============

* **[New]** Built-in ``json`` type
* **[New]** Generic interfaces
* **[New]** Aliases for built-in integer and double types: ``int8``, ``int16``, ``int32``, ``int64``, ``uint8``, ``uint16``, ``uint32``, ``uint64``, ``float32``, ``float64``

Igor Compiler
=============

* **[Improvement]** Report location and error details on script compile-time and run-time error messages
* **[Fix]** Fix duplicated error messages

C# Generator
============

* **[New]** Limited support for XML serialization
* **[Impovement]** Support string serialization for enums to allow their usage in HTTP queries

TypeScript Generator
====================

* **[New]** Support services
* **[New]** Support annotations
* **[New]** Experimental support for patch record JSON serialization

Erlang Generator
================

* **[New]** Experimental support for patch record JSON serialization

Tools
=====

* **[Improvement]** Visual Studio Code plugin: support services

**********************
   New in Igor 2.0.4
**********************

Igor Compiler
=============

* **[Improvement]** ``-roslyn`` option allows to specify Roslyn compiler to support C# 5.0+ for script compilation

Documentation
=============

* **[New]** Igor API Reference
* **[Improvement]** Scripting documentation

**********************
   New in Igor 2.0.5
**********************

Igor Compiler
=============

* **[Fix]** Script compilation on **macOS**

Unreal Engine
=============

* **[Improvement]** Multiple new attributes controlling UE4 C++ code generation (``namespace``, ``prefix``, ``h_file``, ``cpp_file``, etc.)
* **[Improvement]** Support annotations

**********************
   New in Igor 2.0.6
**********************

Igor Compiler
=============

* **[Improvement]** ``-w`` option allows to overwrite readonly files
* **[Improvement]** Allow script files path masks (``-x`` option) to start with ``..``

C# Generator
============

* **[New]** Experimental support for patch record JSON serialization (optional fields are not supported)

Erlang Generator
================

* **[Fix]** Fix string and http query serialization of enums

Unreal Engine
=============

* **[Improvement]** Avoid exceptions when parsing JSON types
* **[Improvement]** Improve namespaces support
* **[Improvement]** Scripting model supports UE4 metadata specifiers

Schema
======

* **[New]** ``meta`` attribute for custom settings

**********************
   New in Igor 2.0.7
**********************

Igor Language
=============

* **[Change]** Change object attribute syntax from ``{}`` to ``()``. Support for the old syntax will be removed in further releases.

C# Generator
============

* **[New]** JSON services
* **[New]** Customizable target framework version
* **[Improvement]** C# 8.0 support
* **[Improvement]** Improve FxCop compliance of generated code
* **[Change]** Binary service namespace is now ``Igor.Services.Binary``

Unreal Engine
=============

* **[Fix]** Fix error responses of JSON services

Schema
======

* **[Change]** Deprecate ``compact`` attribute, use ``meta=(compact)`` instead
* **[Improvement]** Add version field to schema

Documentation
=============

* **[Improvement]** Enhance diagram schema documentation

**********************
   New in Igor 2.0.8
**********************

Documentation
=============

* **[Improvement]** Document patch records 
* **[Improvement]** Document missing JSON attributes

Erlang Generator
================

* **[Improvement]** ``src_path`` and ``include_path`` attributes

Unreal Engine
=============

* **[New]** HTTP client

Go Generator
=============

* **[Improvement]** Enum types JSON and string serialization

TypeScript Generator
=====================

* **[Fix]** Fix errors when strict mode is used
* **[New]** Support JSON service RPCs
* **[New]** Update default target_version to 3.0
