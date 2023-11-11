Early .NET 8 support for [Reinforced.Typings](https://github.com/reinforced/Reinforced.Typings).

# How to use
1. Clone this repo and cake build:
   ```bash
   git clone https://github.com/iotalambda/Reinforced.Typings
   cd Reinforced.Typings/cake
   dotnet tool restore
   cake build
   ```
   This creates a .nupkg file to `Reinforced.Typings` directory.
2. Copy the newly created `.nupkg` file to your solution directory under some folder e.g. `LocalNugetPackages`
3. Add a `NuGet.config` file to your solution directory:
   ```xml
   <?xml version="1.0" encoding="utf-8"?>
   <configuration>
      <packageSources>
         <add key="LocalNugetPackages" value=".\LocalNugetPackages" />
      </packageSources>
   </configuration>
   ```
