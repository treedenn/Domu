# Show all project commands, including commands from child justfiles
default:
    @just --list --list-submodules

# Backend commands
mod backend 'backend/dotnet/justfile'

# Local infrastructure commands
mod infrastructure 'infrastructure/justfile'
