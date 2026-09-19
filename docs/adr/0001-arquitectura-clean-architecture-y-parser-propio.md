# 0001 — Clean Architecture y parser ISO 8583 propio

## Estado

Aceptada

## Contexto

`iso8583-switch` simula un switch de autorización de pagos que habla el protocolo
ISO 8583. Hay dos decisiones estructurales que conviene fijar antes de escribir la
primera línea de lógica de negocio: cómo se organizan las capas del backend, y si el
parseo/armado de mensajes ISO 8583 se apoya en una librería existente o se construye
desde cero.

## Decisión

### Clean Architecture con capas separadas por proyecto

La solución se divide en cinco proyectos con una regla de dependencia estricta hacia
adentro:

- `Switch.Domain`: entidades, value objects y reglas de negocio. No referencia ningún
  paquete externo, ni siquiera EF Core.
- `Switch.Iso8583`: librería de dominio puro para parsear y armar mensajes ISO 8583.
  Tampoco depende de infraestructura.
- `Switch.Application`: casos de uso, expresados como puertos (interfaces) que la
  infraestructura implementa. No conoce EF Core, TCP ni SignalR.
- `Switch.Infrastructure`: implementaciones concretas de los puertos — persistencia
  con EF Core/PostgreSQL, listener TCP, hubs de SignalR.
- `Switch.Api`: host que expone REST/SignalR y arranca el listener TCP como
  `BackgroundService`.

Cada proyecto de test (`tests/*.Tests`) espeja a su proyecto de producción
correspondiente.

**Por qué:** este es un proyecto de portafolio pensado para demostrar diseño de nivel
senior. Clean Architecture hace explícita la regla de dependencia (el dominio no sabe
que existe Postgres o un socket TCP), lo cual facilita testear la lógica de
autorización sin infraestructura real y deja ver, con solo mirar las referencias de
proyecto, qué depende de qué.

### Parser ISO 8583 propio en vez de una librería NuGet

Existen librerías .NET para ISO 8583 (por ejemplo `OpenIso8583net` o paquetes
similares). Se descartan en favor de un parser/builder implementado desde cero en
`Switch.Iso8583`.

**Por qué:**

1. El parseo de bitmaps (primario/secundario), campos de longitud fija y variable
   (`LLVAR`/`LLLVAR`) y el manejo de mensajes malformados es exactamente el tipo de
   problema que este proyecto busca demostrar que se sabe resolver. Delegarlo a una
   librería externa vacía la pieza más valiosa del portafolio.
2. Las librerías existentes suelen estar poco mantenidas o cubrir un subset distinto
   de campos al que interesa mostrar aquí.
3. Un parser propio permite diseñar excepciones de dominio claras (por ejemplo,
   distinguir "el bitmap indica un campo que el buffer no contiene" de una
   `IndexOutOfRangeException` genérica), lo cual es parte del objetivo pedagógico del
   repositorio.

## Consecuencias

- Más trabajo inicial en la Fase 1 (parser/builder con tests exhaustivos) comparado
  con integrar una librería existente.
- El dominio (`Switch.Domain`, `Switch.Iso8583`) queda completamente libre de
  dependencias externas, lo que simplifica sus tests unitarios (no requieren mocks de
  infraestructura ni contenedores).
- Cualquier cambio en la capa de persistencia, transporte o mensajería en tiempo real
  no debería requerir tocar `Switch.Domain`, `Switch.Iso8583` ni `Switch.Application`.
