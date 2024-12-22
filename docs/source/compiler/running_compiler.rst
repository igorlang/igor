*********************
Running Igor Compiler
*********************

Compiler executable
===================

Igor Compiler executable is called *igorc.exe*. It is a command line application. Call it with the list of source files to generate the output:

.. code-block:: batch

   igorc.exe -t csharp *.igor

See :ref:`cli_options` for the list of supported CLI options.

.. _compiler_output:

Compiler Output
===============

Igor Compiler console output is **MSBuild** compatible. Full file path and location are provided when possible.

Igor Compiler reports the following types of problems:

* Igor language syntax errors

* Igor language semantic errors (unresolved references, type errors, etc.)

* Warnings for unknown or obsolete attributes

* Target-specific problems

* Script compilation errors and Warnings

* Script runtime execution exceptions (Igor Compiler analyses stacktrace to find the most relevant location in the script file)

Running Igor Compiler on MacOS and Linux
========================================

You need **Mono** to run Igor Compiler on **macOS** or **Linux**:

.. code-block:: batch

   mono igorc.exe -t csharp *.igor

.. seealso:: `Install Mono on macOS <https://www.mono-project.com/docs/getting-started/install/mac/>`_

.. warning::

   Pay attention to the correct path separators for your platform (slash or backslash).

.. warning::

   Shells like **bash** automatically replace filename wildcards with the list of files.
   It can mess up command line options. 
   
   For example when providing ``-x *.cs`` option shell will replace ``*.cs`` with the list of all matching files. 
   Only the first file will be associated with a ``-x`` option and will be treated as an extension script.
   The rest will be treated as source Igor files (and compilation will obviously fail).

   Therefore on Linux you must escape asterisks with backslash (``\*.cs``) or put wildcard filenames in quotes (``'*.cs'``).

