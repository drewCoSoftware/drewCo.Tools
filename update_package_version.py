# Useful functions to help update version information in AssemblyInfo.cs and .nuspec files.
import re
import fileinput
import os
import sys
import collections.abc
import xml.etree.ElementTree as ET

from enum import Enum
from typing import List


class VersionPart(Enum):
    MAJOR = 0
    MINOR = 1
    PATCH = 2
    REVISION = 3

# ------------------------------------------------------------------------------------


def get_netcore_version(filename, new_version) -> str:
    tree = ET.parse(filename)
    root = tree.getroot()
    # NOTE: It seems that sometimes there will be namespace information.  We will have to be sensitive to that...
    # //ns = {'ns': 'http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd'}
    version = root.find('PropertyGroup/Version')
    return version.text

# ------------------------------------------------------------------------------------


def update_netcore_version(filename, new_version) -> str:
    try:
        tree = ET.parse(filename)
        root = tree.getroot()
        # NOTE: It seems that sometimes there will be namespace information.  We will have to be sensitive to that...
        # //ns = {'ns': 'http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd'}
        version = root.find('PropertyGroup/Version')
        asmVersion = root.find('PropertyGroup/AssemblyVersion')
        fileVersion = root.find('PropertyGroup/FileVersion')

        version.text = new_version
        asmVersion.text = newVersion
        fileVersion.text = newVersion

        # Write the content back out to disk.
        tree.write(filename)

    except Exception as e:
        print(f"Error updating version number: {e}")

    pass

# ------------------------------------------------------------------------------------


def get_assembly_info_version(filename) -> str:
    # Load the file
    with open(filename, 'r', encoding='utf-8-sig') as f:
        content = f.read()

    # Use a regular expression to find the version information
    version_regex = r'\[assembly: AssemblyVersion\("([\d.]+)"\)\]'
    match = re.search(version_regex, content)
    if not match:
        return None

    res = match.group(1)
    return res

# ------------------------------------------------------------------------------------


def update_assembly_info_version(file_path, new_version):
    # Load the file
    with open(file_path, 'r') as f:
        content = f.read()

    # Use a regular expression to find the version information
    version_regex = r'\[assembly: AssemblyVersion\("([\d.]+)"\)\]'
    version_match = re.search(version_regex, content)
    if not version_match:
        raise ValueError('Version information not found in file')

    file_version_regex = r'\[assembly: AssemblyFileVersion\("([\d.]+)"\)\]'
    file_version_match = re.search(file_version_regex, content)
    if not file_version_match:
        raise ValueError('File Version information not found in file')

    # Update the version information with the new version
    old_version_content = f'[assembly: AssemblyVersion("{version_match.group(1)}")]'
    new_version_content = f'[assembly: AssemblyVersion("{new_version}")]'
    content = content.replace(old_version_content, new_version_content)

    old_file_version_content = f'[assembly: AssemblyFileVersion("{file_version_match.group(1)}")]'
    new_file_version_content = f'[assembly: AssemblyFileVersion("{new_version}")]'
    content = content.replace(old_file_version_content,
                              new_file_version_content)

    # old_file_version = file_version_match.group(1)

    # Write the updated content back to the file
    with open(file_path, 'w') as f:
        f.write(content)

# ------------------------------------------------------------------------------------


def get_nuspec_version(filename):
    """Parse version number from a .nuspec file."""
    try:
        tree = ET.parse(filename)
        root = tree.getroot()
        # NOTE: It seems that sometimes there will be namespace information.  We will have to be sensitive to that...
        # //ns = {'ns': 'http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd'}
        version = root.find('metadata/version').text
        return version
    except Exception as e:
        print(f"Error parsing version number: {e}")

# ------------------------------------------------------------------------------------


def update_nuspec_version(filename, new_version):
    """Parse version number from a .nuspec file."""
    try:
        tree = ET.parse(filename)
        root = tree.getroot()
        # NOTE: It seems that sometimes there will be namespace information.  We will have to be sensitive to that...
        # //ns = {'ns': 'http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd'}
        version = root.find('metadata/version')
        version.text = new_version
        tree.write(filename)

    except Exception as e:
        print(f"Error updating version number: {e}")


# ------------------------------------------------------------------------------------
# TODO: Update this so that we can exclude certain directories...
def find_files(directory: str, filenames):
    if not isinstance(filenames, collections.abc.Sequence):
        raise ValueError("'filenames' argument must be a sequence!")

    matches = []
    for filename in filenames:
        for root, dirs, files in os.walk(directory):
            for file in files:
                isMatch = re.match(filename, file)
                if isMatch:
                    file_path = os.path.abspath(os.path.join(root, file))
                    matches.append(file_path)
    return matches

# ------------------------------------------------------------------------------------


def update_version(version_number: str, part: VersionPart) -> str:
    version_parts: List[str] = version_number.split('.')
    while len(version_parts) < 4:
        version_parts.append('0')

    if part == VersionPart.MAJOR:
        version_parts[0] = str(int(version_parts[0]) + 1)
        for i in range(1, len(version_parts)):
            version_parts[i] = '0'

    elif part == VersionPart.MINOR:
        version_parts[1] = str(int(version_parts[1]) + 1)
        for i in range(2, len(version_parts)):
            version_parts[i] = '0'

    elif part == VersionPart.PATCH:
        version_parts[2] = str(int(version_parts[2]) + 1)
        for i in range(3, len(version_parts)):
            version_parts[i] = '0'

    elif part == VersionPart.REVISION:
        version_parts[3] = str(int(version_parts[3]) + 1)

    else:
        raise ValueError('Invalid version part')
    return '.'.join(version_parts)


if __name__ == "__main__":
    args = [
        "get-nuspec-version",
        "get-asm-version",
        "get",
        "update"
    ]

    if len(sys.argv) < 2:
        print("Please provide at least one argument!")
        print("Valid arguments are: \n" + "\n".join(args))
        exit()

    cmd = sys.argv[1]
    print("Command is: " + cmd)

    if cmd == "get" or cmd == "get-asm-version":
        # We will always use this as our "source of truth" for version information.
        file = find_files("./drewco.Tools", ["AssemblyInfo.cs"])[0]
        version = get_assembly_info_version(file)
        print(version)

        # newVersion =  update_version(version, VersionPart.MINOR)
        # print("The new version is: " + newVersion)

    elif cmd == "get-nuspec-version":
        files = find_files("./", [".*\\.nuspec"])
        nuspecFile = files[0]
        version = get_nuspec_version(nuspecFile)
        print(version)

        # print("nuspec files are: \n" + "\n".join(files))

    elif cmd == "update":
        asm1 = find_files("./drewco.Tools", ["AssemblyInfo.cs"])[0]
        oldVersion = get_assembly_info_version(asm1)

        manualVersion = None
        useVersionPart = VersionPart.REVISION
        arg2 = None
        if len(sys.argv) > 2:
            arg2 = sys.argv[2]
            if arg2 == "--version":
                # TODO: Validate argument count + format!
                manualVersion = sys.argv[3]

            else:
              try:
                  useVersionPart = VersionPart[arg2]
              except KeyError:
                  print(f'Invalid version part : "{arg2}"!')
                  print(f'Valid part names are:\n' +
                        "\n".join(VersionPart._member_names_))
                  exit()

        newVersion = update_version(oldVersion, useVersionPart)
        if manualVersion is not None:
            newVersion = manualVersion

        print("New package version will be: " + newVersion)

        print("")
        print("Updating .nuspec version...")
        files = find_files("./", [".*\\.nuspec"])
        nuspecFile = files[0]
        update_nuspec_version(nuspecFile, newVersion)

        print("updating assembly info file (classic)...")
        asm1 = find_files("./drewco.Tools", ["AssemblyInfo.cs"])[0]
        update_assembly_info_version(asm1, newVersion)

        print("updating netcore versions...")
        asm2 = "./drewCo.Tools.Core/drewCo.Tools.Core.csproj"
        update_netcore_version(asm2, newVersion)
