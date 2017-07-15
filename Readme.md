strafesharp [![Status Umbra][status-umbra]][andivionian-status-classifier]
===========

strafesharp is a .NET library to control [Corsair Strafe
RGB][corsair-strafe-rgb] keyboard highlight.

Build
-----

### Prerequisites

To build the library, you need to install crossplatform [.NET Core SDK][dotnet].

### Building

Prepare for the build (set the dependencies up):

```console
$ dotnet restore
$ dotnet build
```

### Test

```console
$ dotnet test ./StrafeSharp.Tests/StrafeSharp.Tests.fsproj
```

### IDE Integration

There're [VSCode tasks][vscode-tasks] configured for the project. If you're
using VSCode, run `build` task to build the project, and `test` task to execute
the unit tests.

[andivionian-status-classifier]: https://github.com/ForNeVeR/andivionian-status-classifier#status-umbra-
[corsair-strafe-rgb]: http://www.corsair.com/en-eu/strafe-rgb-mechanical-gaming-keyboard-cherry-mx-silent
[vscode-tasks]: https://code.visualstudio.com/docs/editor/task

[status-umbra]: https://img.shields.io/badge/status-umbra-red.svg
