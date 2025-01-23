#!/bin/bash

# Get the name of the script itself
SCRIPT_NAME=$(basename "$0")

# Loop through all files and directories in the current directory
for item in *; do
  # Check if the item is not 'index.html' and not the script itself
  if [[ "$item" != "index.html" && "$item" != "$SCRIPT_NAME" && "$item" != "unpack.sh" ]]; then
    # Remove the item (file or directory)
    rm -rf "$item"
  fi
done

echo "All files and directories have been deleted, except for 'index.html' and the script itself."