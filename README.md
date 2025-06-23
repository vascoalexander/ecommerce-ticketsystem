## Getting Started

### Clone Project Files  
**SSH:** `git clone git@github.com:vascoalexander/lushware-ticketsystem.git`  
**HTTPS:** `git clone https://github.com/vascoalexander/lushware-ticketsystem.git`

### Setup local database
create or edit:`src/WebApp/appsettings.Development.json`

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=appdb;Username=postgres;Password=postgres"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```
**OPTIONAL** create or edit `.env`
```toml
POSTGRES_USER=postgres  
POSTGRES_PASSWORD=postgres  
POSTGRES_DB=appdb
```
**Start postrgreSQL DB instance**:  
in root-dir execute
```bash
docker compose up -d
```
### Usefull docker commands
| Command                      | Function                                           |
|------------------------------|----------------------------------------------------|
| `docer ps`                   | Check running container instances                  |
| `docker images`              | List local docker images                           |
| `docker rmi <image>`         | Remove image. Option -f to force remove            |
| `docker stop <container>`    | Stop running container                             |
| `docker start <container>`   | Start stopped container                            |
| `docker restart <container>` | Restart running container                          |
| `docker compose up`          | Start container as specified in docker-compose.yml |
| `docker compose up -d`       | Start container in the background                  |

## Git Workflow
[Git Quickreference](https://vascoalexander.github.io/my-documentation/docs/Tools/GIT)
### Branching
Create a new branch to work on and checkout on this branch
```bash
git checkout -b feature/nameOfFeature
```
Push the feature branch to origin (github)
```bash
git push -u origin feature/nameOfFeature
```
If you havent pushed your feature branch to origin you may delete it after merging
```bash
git branch -d feature/nameOfFeature
```
### Working local
Prepare files for commits (Staging)
```bash
# add specific files
git add filename1 filename2
# add all files
git add . 
```
Create a commit
```bash
git commit -m "Your commit message text"
```
### Pushing & Merging
Push local feature branch to origin (if it exists in origin)
```bash
git checkout feature/nameOfFeature
git push
```
Merge feature branch with develop branch
```bash
# switch to develop branch
git checkout develop
# OPTIONAL: update local develop branch
git pull origin develop
# merge feature into develop
git merge feature/nameOfFeature
# OPTIONAL: test if everything works
# push to origin
git push origin develop
```
## Projektstruktur

```
├── src/
│   └── WebApp/
│       ├── Controllers
│       └── Data
|            ├──AppDbContext.cs
|            └──DbInitializer.cs
│       ├── Models
│       ├── Views
│       ├── WebApp.csproj
│       ├── Program.cs
│       ├── appsettings.json
│       ├── appsettings.Development.json (local)
│       └── ...
├── tests/
│   └── WebApp.Tests/
├── .github/
│   └── /workflows
├── Dockerfile
├── docker-compose.yml
├── .env (local)
├── .gitignore
└── README.md
```

