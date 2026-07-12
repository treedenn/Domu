# Domu.Api

`Domu.Api` is the backend API for Domu.

The project is being rebuilt around a feature-based architecture:

- business capabilities live under `Features/`
- shared technical concerns live in the outer `Application/`, `Infrastructure/`, and `Interface/` folders
- a feature may contain smaller internal groups until they are large enough to become features of their own

See [AGENTS.md](C:\Users\Denni\Documents\Development\Software\Projects\domu\backend\dotnet\src\Domu.Api\AGENTS.md) for
the working architecture rules.

## Run locally

```powershell
dotnet run
```
