#!/bin/bash

SRC=/etc/cs-source/Program.cs
if [ ! -f "$SRC" ]; then
  echo "No C# source file detected."
  echo 1
fi

WORK_DIR=/tmp/Proj$RANDOM
mkdir -p $WORK_DIR

cd $WORK_DIR
dotnet new web
cp $SRC $WORK_DIR/
dotnet run
