************************
    Name Translation
************************

Different programming languages may have different naming conventions prefered or even enforced. Those conventions
may contradict one another. The approach named **name translation** is used to ensure that naming conventions of all
targets are respected.

.. _notation:

Notation
========

Notation describes the way names are translated. The following table describes notations supported by Igor:

===========================  ========================= ===================== ===========================
Notation                     Example: ``IMyInterface`` Example: ``ObjectID`` Example: ``Test_one2three``
===========================  ========================= ===================== ===========================
**none**                     ``IMyInterface``          ``ObjectID``          ``Test_one2three``
**lowercase**                ``imyinterface``          ``objectid``          ``test_one2three``
**uppercase**                ``IMYINTERFACE``          ``OBJECTID``          ``TEST_ONE2THREE``
**lower_camel**              ``iMyInterface``          ``objectId``          ``testOne2Three``
**upper_camel**              ``IMyInterface``          ``ObjectId``          ``TestOne2Three``
**lower_underscore**         ``i_my_interface``        ``object_id``         ``test_one2_three``
**upper_underscore**         ``I_MY_INTERFACE``        ``OBJECT_ID``         ``TEST_ONE2_THREE``
**lower_hyphen**             ``i-my-interface``        ``object-id``         ``test-one2-three``
**upper_hyphen**             ``I-MY-INTERFACE``        ``OBJECT-ID``         ``TEST-ONE2-THREE``
**first_letter_last_word**   ``i``                     ``i``                 ``t``
===========================  ========================= ===================== ===========================

It is recommended that :ref:`naming_convention` is used when writing Igor source files, and name translation is used to translate 
identifiers to the target language when code is generated.
