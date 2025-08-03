# cspmerge
compare and merge `PackageReference`versions between csproj files.

```
Usage: CSPMerge [command] [options]

Options:
  -?|-h|--help  Show help information.

Commands:
  compare       compare two csproj files
  list          list the PackageRefs for a given csproj
  update        update matching package versions to the latest version

Run 'CSPMerge [command] -?|-h|--help' for more information about a command.
```

## Note
If a NuGet version is 'blocked' - that is to say, set to a fixed version:

```csharp
    <PackageReference Include="AutoMapper" Version="[14.0.0]" />
```
The merge logic will find this to be a 'less than' scenario, thereby always ensuring that it gets copied/overwritten.
