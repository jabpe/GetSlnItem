# GetSlnItem
Gets the items and child items in one or more specified locations within a SLN (VS solution) virtual file structure.

Requires at least PowerShell v. 3.

# Example
```powershell
Get-SlnItem -SlnPath Solution\Solution.sln -VirtualPath Server\Scheduler -File -Recurse
```
Gets all the file items (including .csproj projects) from the Server\Scheduler folder and its child folders within the Solution\Solution.sln solution file.
