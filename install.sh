#!/usr/bin/env bash
set -euo pipefail
export PATH="$HOME/.dotnet:$PATH"
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$ROOT"
if ! command -v dotnet >/dev/null 2>&1; then
  echo "dotnet SDK not found. Install .NET 8+ from https://dotnet.microsoft.com/download"
  echo "Or run: curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 8.0"
  exit 1
fi
dotnet --version
dotnet restore SecurePort.sln
dotnet build SecurePort.sln
echo "Done. Run tests: dotnet test SecurePort.sln"
