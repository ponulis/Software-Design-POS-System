# Bash Setup for dotnet-ef

## What Was Done

The dotnet tools path has been added to your `.bashrc` file. The path format is correct for Git Bash on Windows.

## To Apply Changes

**Option 1: Reload your bash profile**
```bash
source ~/.bashrc
```

**Option 2: Restart your terminal/Git Bash**

**Option 3: Run in current session**
```bash
export PATH="$PATH:/c/Users/$USER/.dotnet/tools"
```

## Verify It Works

After reloading, test:
```bash
dotnet ef --version
```

If it still doesn't work, you can use the full path:
```bash
/c/Users/$USER/.dotnet/tools/dotnet-ef.exe --version
```

## For WSL Users

If you're using WSL instead of Git Bash, the path format is different:
```bash
export PATH="$PATH:$HOME/.dotnet/tools"
```

## Troubleshooting

If `dotnet ef` still doesn't work:
1. Check if the file exists: `ls -la ~/.dotnet/tools/dotnet-ef.exe`
2. Make sure the path is correct for your environment
3. Try using the helper script: `./dotnet-ef.ps1` (PowerShell) or use the full path
