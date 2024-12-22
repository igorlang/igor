.. _script_compilation:

*********************
Script Compilation
*********************

Providing scripts
==================

Use ``-x`` command line option to provide extension scripts. You can use wildcard symbols, for example:

.. code-block:: batch

   igorc.exe -t TARGET -x Scripts\*.cs *.igor

By default, relative paths are resolved from current folder. Use ``-X`` command line option to provide additional search paths.

.. seealso:: :ref:`cli_options`

Using Roslyn
==============

By default Igor only supports C# 4.0 for script compilation. You can use modern C# version if you provide the path to **Roslyn** C# compiler.

You can get **Roslyn** using **NuGet** package manager.

**NuGet** download link: https://dist.nuget.org/win-x86-commandline/latest/nuget.exe

Download latest **Roslyn** version using **NuGet**:

.. code-block:: batch

   nuget.exe install Microsoft.Net.Compilers.Toolset

Provide **Roslyn** path to Igor Compiler:

.. code-block:: batch

   igorc.exe -roslyn Microsoft.Net.Compilers.3.3.0\tasks\net472 -t TARGET -x Scripts\*.cs *.igor
