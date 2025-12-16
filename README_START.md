# How to Start the Project

## Bash Script (Git Bash / WSL / Linux / Mac)

### Using Git Bash on Windows:
```bash
./start-project.sh
```

### Using WSL:
```bash
bash start-project.sh
```

### Options:
```bash
./start-project.sh --skip-dependency-check  # Skip dependency checks
./start-project.sh --skip-localdb            # Skip LocalDB check
```

## PowerShell Script (Windows PowerShell)

```powershell
.\start-project.ps1
```

### Options:
```powershell
.\start-project.ps1 -SkipDependencyCheck  # Skip dependency checks
.\start-project.ps1 -SkipLocalDB          # Skip LocalDB check
```

## Manual Start

### 1. Start Backend (Terminal 1)
```bash
cd backend
dotnet run
```

### 2. Start Frontend (Terminal 2)
```bash
cd frontend
npm run dev
```

## URLs

- **Frontend**: http://localhost:5173
- **Backend API**: http://localhost:5168
- **Swagger UI**: http://localhost:5168/swagger

## Troubleshooting

### Bash Script Issues:
- Make sure Git Bash is installed
- If script won't run: `chmod +x start-project.sh`
- On Windows, you may need to use `bash start-project.sh` instead of `./start-project.sh`

### PowerShell Script Issues:
- Run PowerShell as Administrator if you get execution policy errors
- Run: `Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser`

### Port Already in Use:
- Backend port 5168: Change in `backend/Properties/launchSettings.json`
- Frontend port 5173: Vite will automatically use next available port

### Database Issues:
- Ensure SQL Server LocalDB is installed
- Start manually: `sqllocaldb start mssqllocaldb`
- Check status: `sqllocaldb info mssqllocaldb`
