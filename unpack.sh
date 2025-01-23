#!/bin/bash

# Define the source directory
SOURCE_DIR="publish"

# Check if the 'publish' directory exists
if [[ ! -d "$SOURCE_DIR" ]]; then
  echo "Error: Directory '$SOURCE_DIR' does not exist."
  exit 1
fi

# Move all files and directories from 'publish' to the current directory
mv "$SOURCE_DIR"/* . 2>/dev/null
mv "$SOURCE_DIR"/.* . 2>/dev/null

# Check if the 'publish' directory is now empty
if [[ -z "$(ls -A "$SOURCE_DIR")" ]]; then
  # Remove the empty 'publish' directory
  rmdir "$SOURCE_DIR"
  echo "All files and directories have been moved from '$SOURCE_DIR' to the current directory, and '$SOURCE_DIR' has been removed."
else
  echo "Some files or directories could not be moved. Please check the '$SOURCE_DIR' directory."
fi