**********
 Overview
**********

Unreal Engine 4 Target
======================

Unreal Engine 4 target name is ``ue4``. It must be used for the attribute target for attributes
controlling UE4 code generation:

.. code-block:: igor

    [ue4 enabled]

It is also used as the :ref:`compiler` command line argument for the ``-target`` option:

.. code-block:: batch

    igorc.exe -t ue4 *.igor

Output Files
============

As usual ``-o`` command line flag can be used to define output folder. 

.. code-block:: batch

    igorc.exe - t ue4 -o Source\PROJECT\Protocol *.igor

Both ``*.h`` and ``*.cpp`` files are generated to the output folder. 
However, when using ``Private`` and ``Public`` folder layout, you may want header files to go to ``Public`` folder
and source files go to ``Private`` folder. You can achieve that by using ``h_path`` and ``cpp_path`` attributes.
They can be specified per module or in the command line, to apply globally.

.. code-block:: batch

    igorc.exe - t ue4 -o Source\PROJECT -set h_path='Protocol/Public' -set cpp_path='Protocol/Private' *.igor

Alternatively you can control individual file names and locations by using ``h_file`` and ``cpp_file`` attributes:

.. code-block:: igor

    [ue4 h_file="Protocol/Public/MyProtocol.h" cpp_file="Protocol/Private/MyProtocol.cpp"]
    module MyProtocol
    {
    }


