#!/bin/bash

# Build and publish the .NET application
dotnet publish -c Release -o out

# Export environment variables
export ASPNETCORE_ENVIRONMENT=Production
export DOTNET_URLS="https://localhost:7092;http://localhost:5073"

# Run the application
./out/backend
