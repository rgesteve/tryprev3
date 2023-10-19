#!/usr/bin/env bash

# apt install python3-venv

DOTNET_EXE=$HOME/dnet/dotnet
PYTHON_EXE=python3

VENV_NAME=".venv"

$PYTHON_EXE -m venv $VENV_NAME
source $VENV_NAME/bin/activate
$PYTHON_EXE -m pip install -r requirements.txt
$PYTHON_EXE run_bench.py
deactivate

#LD_LIBRARY_PATH=/root/.nuget/packages/microsoft.ml.onedal/0.21.0-dev.22619.1/runtimes/linux-x64/native dotnet run
#LD_LIBRARY_PATH=/root/.nuget/packages/microsoft.ml.onedal/0.21.0-dev.22619.1/runtimes/linux-x64/native ${DOTNET_EXE} run


# $LIBS_LOCATION="..\..\..\artifacts\bin\Native\x64.Debug"
# #ls $LIBS_LOCATION
# 

# & $PYTHON_NAME -m venv $VENV_NAME
# & ($VENV_NAME + "/Scripts/Activate.ps1")
# #$env:PATH=("..\..\artifacts\bin\Native\x64.Debug;" + $env:PATH)
# $env:PATH=("$LIBS_LOCATION;" + $env:PATH)
# & $PYTHON_NAME run_bench.py
# deactivate
