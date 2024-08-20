function Item-Exists($path) {

    $x = Get-Item -Path $path -ErrorAction SilentlyContinue
    return $x.Length -gt 0
}


# Get a clean copy of the build dir.
if (Item-Exists("nuget-build")) {
    Remove-Item "nuget-build" -Force -Recurse

    New-Item "nuget-build" -ItemType "Directory"
}
New-Item "nuget-build\lib\net472" -ItemType "Directory"
New-Item "nuget-build\lib\net6.0" -ItemType "Directory"


# Build our assemblies:
msbuild drewCo.Tools.sln -p:Configuration=Release
dotnet build drewCo.Tools.Core.sln -p:Configuration=Release


#Copy the stuff we care about to the lib folder....
# NET 4.7.2
Copy-Item -Path ".\drewCo.Tools\bin\Any CPU\Release\drewCo.Tools.dll" "nuget-build\lib\net472\drewCo.Tools.dll"
Copy-Item -Path ".\drewCo.Tools\bin\Any CPU\Release\drewCo.Tools.pdb" "nuget-build\lib\net472\drewCo.Tools.pdb"
Copy-Item -Path ".\drewCo.Tools\bin\Any CPU\Release\drewCo.Tools.xml" "nuget-build\lib\net472\drewCo.Tools.xml"

## NOTE: We are no longer going to support netcore 3.1
## NETCORE 3.1
# Copy-Item -Path ".\drewCo.Tools.Core\bin\Release\netcoreapp3.1\drewCo.Tools.dll" "nuget-build\lib\netcoreapp3.1\drewCo.Tools.dll"
# Copy-Item -Path ".\drewCo.Tools.Core\bin\Release\netcoreapp3.1\drewCo.Tools.pdb" "nuget-build\lib\netcoreapp3.1\drewCo.Tools.pdb"
# Copy-Item -Path ".\drewCo.Tools.Core\bin\Release\netcoreapp3.1\drewCo.Tools.xml" "nuget-build\lib\netcoreapp3.1\drewCo.Tools.xml"

# NETCORE 5.0
# Copy-Item -Path ".\drewCo.Tools.Core\bin\Release\net5.0\drewCo.Tools.dll" "nuget-build\lib\net5.0\drewCo.Tools.dll"
# Copy-Item -Path ".\drewCo.Tools.Core\bin\Release\net5.0\drewCo.Tools.pdb" "nuget-build\lib\net5.0\drewCo.Tools.pdb"
# Copy-Item -Path ".\drewCo.Tools.Core\bin\Release\net5.0\drewCo.Tools.xml" "nuget-build\lib\net5.0\drewCo.Tools.xml"
# Copy-Item -Path ".\drewCo.Tools.Core\bin\Release\net5.0\drewCo.Tools.deps.json" "nuget-build\lib\net5.0\drewCo.Tools.deps.json"

# NETCORE 6.0
Copy-Item -Path ".\drewCo.Tools.Core\bin\Release\net6.0\drewCo.Tools.dll" "nuget-build\lib\net6.0\drewCo.Tools.dll"
Copy-Item -Path ".\drewCo.Tools.Core\bin\Release\net6.0\drewCo.Tools.pdb" "nuget-build\lib\net6.0\drewCo.Tools.pdb"
Copy-Item -Path ".\drewCo.Tools.Core\bin\Release\net6.0\drewCo.Tools.xml" "nuget-build\lib\net6.0\drewCo.Tools.xml"
Copy-Item -Path ".\drewCo.Tools.Core\bin\Release\net6.0\drewCo.Tools.deps.json" "nuget-build\lib\net6.0\drewCo.Tools.deps.json"


# Copy everything to the build dir....
# Copy-Item -Path ".\lib" ".\nuget-build\lib" -Recurse
Copy-Item ".\drewCo.Tools.nuspec" ".\nuget-build\drewCo.Tools.nuspec"

# Pack it all up...
nuget pack ".\nuget-build\drewCo.Tools.nuspec"