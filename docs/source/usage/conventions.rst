.. _naming_convention:

*******************
Naming Convention
*******************

Naming convention is not enforced by the language, but the following rules are encouraged.

* Module names, user-defined type names, service names and function names should use ``UpperCamelCase`` notation.
* Interface names should start with ``I`` and use ``UpperCamelCase`` notation.
* Record field names,  enum field names and function argument names should use ``lower_underscore`` notation.
* Generic argument names should be a single capitalized char (usually ``T``), or an ``UpperCamelCase`` name starting with ``T`` (for example ``TKey``).
* User defined attribute names should use ``lower_underscore`` notation and have a domain prefix separated with dot. Example: ``catalogue.enabled``.
