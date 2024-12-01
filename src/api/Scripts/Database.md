## Common Scripts

Adding a migration
```
dotnet ef migrations add name-of-migration
```

Apply migration to the database
```
dotnet ef database update
```

Listing all migrations
```
dotnet ef migrations list
```

Removing the last migration **[Remember to apply the database update after]**
```
dotnet ef migrations remove
```