MasterMemory
===
Embedded Readonly In-Memory Document Database for .NET, .NET Core and Unity

Work in progress.

Concept
---
MasterMemory's objective has two areas.

* **memory efficient**, Do not create index in PrimaryKey search, only use underlying data memory.
* **startup speed**, MasterMemory adopts [ZeroFormatter](https://github.com/neuecc/ZeroFormatter/) as an internal data structure so enable infinitely fast deserialize.

Features
---

* O(log n) index key search
* allows composite primary/secondary key
* zero index memory space for primary key
* secondary key index(use memory space)
* range search
* 1-nearest neighbor search
