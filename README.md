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

Install
---
for .NET, .NET Core

* PM> Install-Package [MasterMemory](https://www.nuget.org/packages/MasterMemory)

for Unity, Unity packages exists on [MasterMemory/Releases](https://github.com/neuecc/MasterMemory/releases) as well. More details, please see the [Unity-Supports](https://github.com/neuecc/MasterMemory#unity-supports) section.

Features
---

* O(log n) index key search
* allows multikey index
* zero index memory space for primary key
* dynamic secondary key index
* closest search
* lightweight range-view
* ILookup/IDictionary view

Quick Start
---
TODO....

Performance
---

![image](https://cloud.githubusercontent.com/assets/46207/21770444/ac6ca034-d6c6-11e6-9b64-b947f291e307.png)

![image](https://cloud.githubusercontent.com/assets/46207/21770452/b561f932-d6c6-11e6-833e-97f977b6e00c.png)



TODO....

Architecture
---

TODO...

Unity Supports
---
MasterMemory requires [ZeroFormatter](https://github.com/neuecc/ZeroFormatter/) as dependencies.

MasterMemory.Unity works on all platforms(PC, Android, iOS, etc...). But it can 'not' use dynamic keytuple index generation due to IL2CPP issue. But pre code generate helps it. Code Generator is located in `packages\MasterMemory.*.*.*\tools\MasterMemory.CodeGenerator.exe`, which is using [Roslyn](https://github.com/dotnet/roslyn) so analyze source code, pass the target `csproj`. 

```
arguments help:
  -i, --input=VALUE             [required]Input path of analyze csproj
  -o, --output=VALUE            [required]Output path
  -u, --unuseunityattr          [optional, default=false]Unuse UnityEngine's RuntimeInitializeOnLoadMethodAttribute on MasterMemoryInitializer
  -c, --conditionalsymbol=VALUE [optional, default=empty]conditional compiler symbol
  -n, --namespace=VALUE         [optional, default=MasterMemory]Set namespace root name
```

TODO:....

Author Info
---
Yoshifumi Kawai(a.k.a. neuecc) is a software developer in Japan.  
He is the Director/CTO at Grani, Inc.  
Grani is a top social game developer in Japan.  
He is awarding Microsoft MVP for Visual C# since 2011.  
He is known as the creator of [UniRx](http://github.com/neuecc/UniRx/)(Reactive Extensions for Unity)  

Blog: https://medium.com/@neuecc (English)  
Blog: http://neue.cc/ (Japanese)  
Twitter: https://twitter.com/neuecc (Japanese)   

License
---
This library is under the MIT License.