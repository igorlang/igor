**********
 Overview
**********

Go Target
==========

Go target name is ``go``. It must be used for the attribute target for attributes
controlling Go code generation:

.. code-block:: igor

    [go enabled]

It is also used as the :ref:`compiler` command line argument for the ``-target`` option:

.. code-block:: batch

    igorc.exe -t go *.igor
