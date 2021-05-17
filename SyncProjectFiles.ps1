# This script will update drewCo.Tools.Core so that it contains the same files (via link) as the .net 4.7.2 build
# of the library.  Naturally we don't want to go syncing all of this junk by hand if we don't have to.
function Path-Exists($path)
{
    $res = Get-Item -Path $path -ErrorAction SilentlyContinue
    return $res -ne $NULL
}

function Get-Relpath($from, $to)
{
    $oldLocation = Get-Location

    Set-Location $from
    $relative = Resolve-Path $to -Relative

    #restore the location...
    Set-Location $oldLocation

    return $relative
}

function AddAttribute($xml, $node, $name, $value) 
{
    $attr = $xml.CreateAttribute($name)
    $attr.InnerXml = $value
    $node.Attributes.Append($attr)
}

function RemoveNodesFromDoc($xml, $xpath)
{

    $nodes = $xml.SelectNodes($xpath)
    RemoveNodes($nodes)
}

function RemoveNodes($nodes)
{
    $nodes | ForEach-Object -Process {
        $_.ParentNode.RemoveChild($_)
    }
}

function RemoveEmptyNodes($xml, $xpath)
{

    $nodes = $xml.SelectNodes($xpath)
    $nodes | ForEach-Object -Process {
        if ($_.Children.Count -eq 0) {
            $_.ParentNode.RemoveChild($_)
        }
        
    }

}

# Syncs the files listed in the source project (dotnet) to the destination project (netcore)
function Sync-Dotnet-Netcore {

    Param (
        [Parameter(Mandatory=$true)]
        [String]$SourceProject,
        [Parameter(Mandatory=$true)]
        [String]$DestinationProject
    )

    $srcDir = (Get-Item $SourceProject).DirectoryName
    $destDir = (Get-Item $DestinationProject).DirectoryName
    $relDir = Get-Relpath $destDir $srcDir


    # Open the .net project file.
    [xml]$srcXml = Get-Content -Path  $SourceProject

    $nsMgr = New-Object -TypeName "System.Xml.XmlNamespaceManager" -ArgumentList $srcXml.NameTable
    $msBuildNs = "http://schemas.microsoft.com/developer/msbuild/2003"
    $nsMgr.AddNamespace("msb", $msBuildNs)


    $root =  $srcXml.DocumentElement
    $srcFiles = $root.SelectNodes("msb:ItemGroup/msb:Compile/@Include", $nsMgr)


    # Now that we have our input files, we can update the target project XML...
    [xml]$dest = Get-Content -Path $DestinationProject

    RemoveNodesFromDoc $dest "/Project/ItemGroup/Compile"
    RemoveNodesFromDoc $dest "/Project/ItemGroup/Folder"


    # Find + remove empty itemgroup nodes.....
    RemoveEmptyNodes $dest "/Project/ItemGroup"

    # Now we can recreate the files + folders in the destination project.
    $targetFiles = $dest.CreateElement("ItemGroup")
    $folderList = New-Object Collections.Generic.List[string]

    $srcFiles | ForEach-Object {

        $folderName = Split-Path $_.Value

        # The items in the 'properties' folder will be ignored.
        # Including this data in a .netcore project has bad consequences.
        if ($folderName -eq "Properties") { 
            return
        }

        # Collect unique folder names.
        if ($folderName -ne "") {
            if (!$folderList.Contains($folderName)) {
                $folderList.Add($folderName)
            }
        }

        # check it for a path (folder) -> append to our list of folders if new.
        # include it in the target files...
        $compileNode = $dest.CreateElement("Compile")


        # We need to compute the relative paths correctly.....
        $relPath = "$relDir\" + $_.Value

        AddAttribute $dest $compileNode "Include" $relPath
        AddAttribute $dest $compileNode "Link" $_.Value

        $targetFiles.AppendChild($compileNode)
    }

    $dest.Project.AppendChild($targetFiles)


    $targetFolders = $dest.CreateElement("ItemGroup")
    $folderList | ForEach-Object -Process {
       $folderNode = $dest.CreateElement("Folder")
       AddAttribute $dest $folderNode "Include" $_ 

       $targetFolders.AppendChild($folderNode)

       # Create + git-add any missing folders.
       $folderDir = "$destDir\" + $_
       if ((Path-Exists($folderDir)) -eq $false)
       {
            New-Item -Path $folderDir -ItemType "Directory"
            git add $folderDir
       }

    }
    $dest.Project.AppendChild($targetFolders)




    $dest.Save($DestinationProject)


}

Sync-Dotnet-Netcore "./drewCo.Tools/drewCo.Tools.csproj" "./drewCo.Tools.Core/drewCo.Tools.Core.csproj"

#NOTE: We can enable this AFTER we convert the source project to NUnit
#Sync-Dotnet-Netcore "./drewCo.Tools.Testers/drewCo.Tools.Testers.csproj" "./drewCo.Tools.Core.Testers/drewCo.Tools.Core.Testers.csproj"
