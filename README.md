MasterMemory
===
Embedded Readonly In-Memory Document Database for .NET, .NET Core and Unity

Work in progress.

Concept
---
MasterMemory's objective has two areas.

* **memory efficient**, Do not create index in PrimaryKey search, only use underlying data memory.
* **startup speed**, MasterMemory adopts [ZeroFormatter](https://github.com/neuecc/ZeroFormatter/) as an internal data structure so enable infinitely fast deserialize.

These features are suitable for master data management(read-heavy and less-write) on application embedded especially role-playing game. MasterMemory has better performance than any other in-memory database(100x faster than filebase SQLite and 10x faster than inmemory SQLite).

Features
---

* O(log n) index key search
* allows multikey index
* zero index memory space for primary key
* dynamic secondary key index
* closest search
* lightweight range-view
* ILookup/IDictionary view





Performance
---