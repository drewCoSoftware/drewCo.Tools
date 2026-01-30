import subprocess
import shutil
from os import path

DIST_DIR = "./dist-linux"

if path.exists(DIST_DIR):
    shutil.rmtree(DIST_DIR)

exe = f'dotnet publish -c Release -o "{DIST_DIR}" --runtime linux-x64 ./ConsoleLoggerTestbed/ConsoleLoggerTestbed.csproj'
callRes = subprocess.call(exe)
if callRes != 0:
    raise Exception("Could not build the project!")

