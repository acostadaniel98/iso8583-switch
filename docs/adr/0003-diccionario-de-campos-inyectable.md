# 0003 — Diccionario de definiciones de campos inyectable en builder/parser

## Estado

Aceptada

## Contexto

`BUILD_PLAN.md` (Fase 1) fija un subset realista de campos ISO 8583 a soportar: MTI y
los campos 2, 3, 4, 7, 11, 12, 13, 14, 22, 25, 32, 37, 38, 39, 41, 42 y 49. Todos esos
campos son `Fixed` o `LLVAR`, y todos tienen número ≤ 64.

La misma fase exige también soporte completo de:

1. El tipo `LLLVAR` (prefijo de longitud de 3 dígitos), y
2. El bitmap secundario (campos 65-128).

Ninguno de los dos aparece en el subset de campos de negocio. Si `Iso8583MessageBuilder`
y `Iso8583MessageParser` dependieran directamente de `IsoFieldDefinitions.Standard`
(como en la primera versión de esta fase), no habría forma de ejercitar esos dos
caminos de código a través de la API pública sin inventar un campo de negocio
ficticio fuera del subset aprobado — lo cual violaría la regla 6 del plan ("nada de
scope creep").

## Decisión

`Iso8583MessageBuilder` e `Iso8583MessageParser.Parse` reciben el diccionario de
definiciones de campos como parámetro opcional (`IReadOnlyDictionary<int,
IsoFieldDefinition>?`), con `IsoFieldDefinitions.Standard` como valor por defecto.
`Iso8583Message` guarda internamente el diccionario con el que fue construido, para
que `ToBytes()` serialice de forma consistente sin volver a consultar el diccionario
estático.

En producción (`Switch.Api`, `Switch.Infrastructure`, etc.) el parámetro nunca se pasa
explícitamente, así que el comportamiento observable es idéntico a usar directamente
`IsoFieldDefinitions.Standard`. En los tests de `Switch.Iso8583.Tests`, un diccionario
sintético con una definición `Lllvar` y una definición de campo > 64 permite probar
ambos caminos de código sin tocar el diccionario real de negocio.

## Consecuencias

- Cero cambio de comportamiento para el uso normal (sin parámetro, se usa `Standard`).
- `Iso8583Message` ya no depende implícitamente de un estado global
  (`IsoFieldDefinitions.Standard`) para serializarse — es autocontenido, lo cual es
  además una mejora de diseño independiente del problema de testing que la motivó.
- Si una fase futura agrega campos reales `LLLVAR` o > 64 al subset de negocio (fases
  7-9 mencionan reversos y settlement, que sí los usan), el diccionario `Standard` se
  extiende y los tests de esta fase pueden migrarse a usarlo directamente si se desea,
  sin romper la API.
