# TLDExtract

Kleine Bibliothek zum Parsen von Domain-Strings und Extrahieren von Subdomain, Domain und Suffix basierend auf der Public Suffix List.

Kurz: `TLDExtractor.Extract(...)` akzeptiert eine `string`-Darstellung einer Domain oder eine `Uri` und liefert ein `ExtractResult` mit `SubDomain`, `Domain`, `Suffix` und `SuffixType`.

## Quick Start

1. Projekt bauen:
   - CLI: `dotnet build`
   - Visual Studio: Öffne die Lösung und baue über __Build > Build Solution__.

2. Tests ausführen:
   - CLI: `dotnet test`
   - Visual Studio: benutze __Test Explorer__.

## Beispiele

C# (String):
```c#
using TLDExtract;
var result = TLDExtractor.Extract("seraphim.escsoft.de"); Console.WriteLine(result.ToString());
// Beispiel-Ausgabe: // ExtractResult(subdomain='www', domain='seraphim', suffix='escsoft.de', suffix type='ICANN')
