#!/bin/bash

# Check for the version parameter
if [ "$#" -ne 1 ]; then
  echo "Usage: $0 <version>"
  exit 1
fi

VERSION=$1
URL="https://github.com/immodi/graduation-backend/releases/download/v${VERSION}/backend-linux.tar.gz"
FILE="backend-linux.tar.gz"

echo "Downloading release version ${VERSION}..."
wget -O "${FILE}" "${URL}"
if [ $? -ne 0 ]; then
  echo "Download failed. Please check the version number and your network connection."
  exit 1
fi

echo "Removing existing './app' directory (if it exists) and creating a fresh one..."
rm -rf ./app
mkdir -p ./app

echo "Extracting ${FILE} into ./app..."
tar -xzf "${FILE}" -C ./app
if [ $? -ne 0 ]; then
  echo "Extraction failed."
  exit 1
fi

# Optionally remove the downloaded tarball
rm -f "${FILE}"

echo "Deployment completed successfully."
