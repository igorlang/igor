.. _modules:

***************
    Modules
***************

Each *igor* file can contain one or several modules. Module is the only top-level declaration allowed in *igor* file.

Module Declaration
==================

*Syntax:*

.. code-block:: igor

    module ModuleName
    {
        // nested declarations
        ...
    }

where

* ``ModuleName`` is the valid identifier of the declared module name.

Modules embed nested declarations (types and services, but not other modules). 

Types and services declared in the same module can reference each other by name. Order of declaration is not important.

Module Names
============

Module names are not required to correlate with file names they are defined in. 

.. note::

    In fact, though currently **Igor Compiler** only deals with files, one can imagine situation where modules are transfered over
    network as some service payload or are generated in runtime. That's why it's not important where the module comes from, and module
    is identified by its name, not its file name.

Using Statement
===============

To reference types located in another module we need to import this module first via **using** statement. Using statements must be 
located at the very top of the file before module(s).

*Syntax:*

.. code-block:: igor

    using ImportedModuleName;

*Example:*

.. code-block:: igor

    // content of vectors.igor
    module Vectors
    {
        record Vector2
        {
            float x;
            float y;
        }

        record Vector3
        {
            float x;
            float y;
            float z;
        }
    }

.. code-block:: igor

    // content of shapes.igor
    using Vectors;

    module Shapes
    {
        record Circle
        {
            // without using Vectors statement at the top of the file this would cause an error
            Vector2 center;
            float radius;
        }
    }

In order for imported module to be found, it should be located at *source path* or *import path*. See :ref:`cli_options` for details.


