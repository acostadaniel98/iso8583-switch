# 0002 — Target framework: .NET 10 en vez de .NET 8

## Estado

Aceptada

## Contexto

El plan de construcción original especifica .NET 8 (LTS) como target framework para
todo el backend. Al hacer el bootstrap de la solución (Fase 0), la máquina de
desarrollo solo tiene instalado el SDK de .NET 10 — no hay SDK ni runtime de .NET 8
disponibles localmente.

## Decisión

Se construye toda la solución sobre `net10.0` en lugar de `net8.0`.

**Por qué:** instalar un SDK adicional solo para cumplir la versión mencionada en el
plan no aporta valor real al objetivo del proyecto (demostrar diseño y arquitectura),
y añade una dependencia de entorno innecesaria. .NET 10 es la versión ya disponible y
totalmente compatible con todo lo planeado (ASP.NET Core, EF Core, SignalR,
BackgroundService).

## Consecuencias

- Todas las referencias a ".NET 8" en `BUILD_PLAN.md` deben leerse como ".NET 10" de
  aquí en adelante.
- `dotnet new sln` en .NET 10 genera un archivo de solución `.slnx` (formato XML) en
  vez del clásico `.sln`. El repositorio usa `Switch.slnx`; todos los comandos
  (`dotnet build`, `dotnet test`, el workflow de CI) apuntan a ese archivo.
- Si en el futuro se requiere compatibilidad LTS estricta, esta decisión debe
  revisarse explícitamente en una nueva ADR antes de hacer el downgrade.
