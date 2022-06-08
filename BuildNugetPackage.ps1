function Item-Exists($path) {

    $x = Get-Item -Path $path -ErrorAction SilentlyContinue
    return $x.Length -gt 0
}


# Get a clean copy of the build dir.
if (Item-Exists("nuget-build")) {
    Remove-Item "nuget-build" -Force -Recurse

    New-Item "nuget-build" -ItemType "Directory"
}

# Build our assemblies:
msbuild drewCo.Tools.sln -p:Configuration=Release
dotnet build drewCo.Tools.Core.sln -p:Configuration=Release

# Copy everything to the build dir....
Copy-Item -Path ".\lib" ".\nuget-build\lib" -Recurse
Copy-Item ".\drewCo.Tools.nuspec" ".\nuget-build\drewCo.Tools.nuspec"

# Pack it all up...
nuget pack ".\nuget-build\drewCo.Tools.nuspec"