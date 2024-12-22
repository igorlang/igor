********************
  Network Protocol
********************

Igor network protocol is used to transfer data (for example, Igor service packets) over TCP stream.

Packets
=======

Data stream is the sequence of binary packets. 

*Packet encoding:*

========== ============= ==========================================
Field      Byte Size     Description
========== ============= ==========================================
Size       1..4          Variable-length encoded payload size
Payload    0..0x0FFFFFFF Application-specific payload
========== ============= ==========================================

Zero-size payload means disconnect request. The connection should be shutdown.

Service Maps
============

Depending on application needs, packet payload may just contain a single service message, or a service map packet.

*Service map encoding:*

=============== =============== ==========================================
Field           Byte Size       Description
=============== =============== ==========================================
Service         1               Service id
Service message 0..0x0FFFFFFE   Service-specfic message 
=============== =============== ==========================================

Service id can be defined as Igor Enum. Most often 1 byte is used as an integer type (but another integer type may be used as well).

.. seealso::
    * :ref:`enums`
    * :ref:`binary_enum_encoding`

Service maps can be nested. For example, when several players play a split-screen game, a nested service map may be used to multiplex messages for different players.

For Igor service message encoding, see :ref:`binary_service_encoding`.



