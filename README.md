# lumen.api
## Guild API using .netCore 2.2 and EF Core Inmemory Provider
___
### The API
> Developed with .Net Core 2.2, EF Core 2, Microsoft.EntityFrameworkCore.Inmemory package, Repository Pattern and Dependency Injection.

### Setup

Install [.NetCore SDK ](https://dotnet.microsoft.com/download "microsoft downloads")

in command prompt with .net CLI and git installed:
```
git clone https://github.com/icarotorres/lumen.api.git
dotnet restore
dotnet add package Microsoft.EntityFrameworkCore.InMemory --version 2.2.1	

_to compile files without running:_
dotnet build

dotnet run
or configure your VS CODE Debugger with .vscode folder on project root:
```
#### tasks.json
``` js
{
    "version": "2.0.0",
    "tasks": [
        {
            "label": "build",
            "command": "dotnet",
            "type": "process",
            "args": [
                "build",
                "${workspaceFolder}/lumen.api.csproj"
            ],
            "problemMatcher": "$msCompile"
        }
    ]
}
```
___

### Content

#### Actions, Methods and Endpoints
| Action | Method | Endpoint URI format |
| -------| -------| -------------------|
| Create | GET | https://[server]/lumen.api/guild/create?guildname=[string]&mastername=[string] |
| Guilds | GET | https://[server]/lumen.api/guild/guilds |
| Guilds | GET | https://[server]/lumen.api/guild/guilds/[count] |
| Info | GET | https://[server]/lumen.api/guild/info/[guildname] |
| Enter | GET | https://[server]/lumen.api/guild/enter/[guildname]/[username] |
| Leave | GET | https://[server]/lumen.api/guild/leave/[username]/[guildname] |
| Transfer | GET | https://[server]/lumen.api/guild/transfer[guildname]/[string] |

> **Create**
> Receives 2 params `(string guildname, string mastername)` to create a new Guild.
> if the user do not already exists, creates a brand new one setting it as a member and guildmaster of the resulting new Guild.
> Fails if guild already exists.
> _Return a boolean corresponding the the success status_.
___
> **Guilds**
> Receives 1 param `(int count)` to return the Nth first Guilds or no params to return the first 20ths.
> _Return a list of guilds found_.
___
> **Info**
> Receives 1 param `(string guildname)` display an JSON Object with the guildname, guildmaster and a list of members.
> Fails if the guild not exists.
> if fail, returns an object containing `{ "error":  "guild not found."}`.
> _Return expected_.
``` js
{
  "guild": {
    "name": "<the guildname>",
    "guildmaster": "<the guildmaster's name>",
    "members" : [
      //... a list of member names
    ]
  }
}
```
___
> **Enter**
> Receives 2 params `(string guildname, string username)` to insert a new member to a guild.
> Fails if: 
+ guild or user not found;
+ inserting a user currently in other guild;
+ user already in the guild.
> _Return a boolean corresponding the success status_.
___

> **Leave**
> Receives 2 params `(string username, string guildname,)` to remove a new member from a guild.
> Fails if: 
+ guild or user not found;
+ trying to remove a user currently out of the guild;
+ user is guildmaster (nedding to Trasfer the guild ownership to other member).
> _Return a boolean corresponding the success status_.
___
> **Transfer (bonus)**
> Receives 2 params `(string guildname, string username,)` to pass the guild ownership position to another member from the guild.
> Need to be performed before a guildmaster leaves a guild.
> Fails if: 
+ guild or user not found;
+ trying to remove a user currently out of the guild;
+ user is guildmaster (nedding to Trasfer the guild ownership to other member).
> _Return a boolean corresponding the success status_.
___
### Model representation & Context
#### User.cs
``` c#
public class User
{
  public User ()
  {
    Gold = 0.0f;
    Level = 1;
    CreationDate = DateTime.Now;
  }
  [Key]
  public string Name { get; set; }
  public int Level { get; set; }
  public Double Gold { get; set; }
  public virtual Guild Guild { get; set; }
  public DateTime CreationDate { get; set; }
  public string GuildName { get; set; }
  public bool IsGuildMaster { get => Name.Equals(Guild?.MasterName); }
  public string FormatDate { get => CreationDate.ToString("d"); }
}
```

#### Guild.cs
``` c#
 public class Guild
{
  public Guild ()
  {
    Level = 1;
    CreationDate = DateTime.Now;
  }
  [Key]
  public string Name { get; set; }
  public int Level { get; set; }
  public virtual User Master { get; set; }
  public virtual ICollection<User> Members { get; set; }
  public DateTime CreationDate { get; set; }
  public string MasterName { get; set; }
}
```

#### LumenContext
``` c#
public class LumenContext : DbContext
{
  public DbSet<Guild> Guilds { get; set; }
  public DbSet<User> Users { get; set; }
  public LumenContext(DbContextOptions<LumenContext> options) : base(options) { }
  public LumenContext() {}
  protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseInMemoryDatabase("lumenInMemoryDB");
  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.Entity<User>()
      .HasOne(u => u.Guild)
      .WithOne(g => g.Master)
      .HasForeignKey<User>(u => u.GuildName);

    modelBuilder.Entity<Guild>()
      .HasOne(g => g.Master)
      .WithOne(u => u.Guild)
      .HasForeignKey<Guild>(g => g.MasterName);
  }
}
```
