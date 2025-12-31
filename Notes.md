Awesome—let’s build a small C# (C# 10+) console app on modern .NET (works on .NET 6/7/8) that uses SQLite to manage a 3NF database with:

Players (entity)
Inventory (entity)
PlayerInventory (join table with a Qty field and proper foreign keys)

We’ll go step-by-step, including the NuGet installs, the schema, and C# code to create the DB/tables if not present, enforce referential integrity with ON DELETE CASCADE for players, and provide queries to add inventory items (plus some example CRUD).

As a Computer Science teacher, you’ll also see how we use Lists, Dictionaries, and Classes cleanly in the code.

#0) Prereqs

Install .NET SDK (6/7/8).
Create a new console project:

```powershell

dotnet new console -n GameDbDemo
cd GameDbDemo

```

