#!/bin/bash
# Script to add dotnet-ef to PATH in bash

# Detect if running in WSL or Git Bash
if [[ -d "/mnt/c/Users" ]]; then
    # Git Bash on Windows
    DOTNET_TOOLS_PATH="/mnt/c/Users/$USER/.dotnet/tools"
elif [[ -d "/home/$USER/.dotnet" ]]; then
    # WSL or Linux
    DOTNET_TOOLS_PATH="$HOME/.dotnet/tools"
else
    # Windows native bash (if exists)
    DOTNET_TOOLS_PATH="$HOME/.dotnet/tools"
fi

# Add to PATH if not already present
if [[ ":$PATH:" != *":$DOTNET_TOOLS_PATH:"* ]]; then
    echo "Adding dotnet tools to PATH..."
    
    # Add to .bashrc
    if [ -f "$HOME/.bashrc" ]; then
        if ! grep -q "export PATH.*\.dotnet/tools" "$HOME/.bashrc"; then
            echo "" >> "$HOME/.bashrc"
            echo "# Add dotnet tools to PATH" >> "$HOME/.bashrc"
            echo "export PATH=\"\$PATH:$DOTNET_TOOLS_PATH\"" >> "$HOME/.bashrc"
            echo "Added to .bashrc"
        fi
    fi
    
    # Add to .bash_profile (for macOS/login shells)
    if [ -f "$HOME/.bash_profile" ]; then
        if ! grep -q "export PATH.*\.dotnet/tools" "$HOME/.bash_profile"; then
            echo "" >> "$HOME/.bash_profile"
            echo "# Add dotnet tools to PATH" >> "$HOME/.bash_profile"
            echo "export PATH=\"\$PATH:$DOTNET_TOOLS_PATH\"" >> "$HOME/.bash_profile"
            echo "Added to .bash_profile"
        fi
    fi
    
    # Also add to current session
    export PATH="$PATH:$DOTNET_TOOLS_PATH"
    echo "PATH updated for current session"
    echo "Run 'source ~/.bashrc' or restart your terminal to apply permanently"
else
    echo "dotnet tools already in PATH"
fi

# Verify
if command -v dotnet-ef &> /dev/null || dotnet ef --version &> /dev/null; then
    echo "✓ dotnet ef is now available"
    dotnet ef --version
else
    echo "⚠ dotnet ef not found. Make sure dotnet-ef is installed:"
    echo "  dotnet tool install --global dotnet-ef"
fi
