# Welcome to your CDK C# project!

This is a blank project for C# development with CDK.

The `cdk.json` file tells the CDK Toolkit how to execute your app.

It uses the [.NET Core CLI](https://docs.microsoft.com/dotnet/articles/core/) to compile and execute your project.

## Useful commands

* `dotnet build src` compile this app
* `cdk deploy`       deploy this stack to your default AWS account/region
* `cdk diff`         compare deployed stack with current state
* `cdk synth`        emits the synthesized CloudFormation template


```SQL


```

```powershell
[CmdletBinding()]
param (
    [Parameter()] [String] $RdsInstanceName = "Music-Store-SQL-Server",
    [Parameter()] [String] $RdsSecretPrefix = "music-store-database-password"
)

Write-Host "Windows user `"$($env:USERNAME)`" logged in at $(Get-Date)."

$rdsInstance = Get-RDSDBInstance $RdsInstanceName
$rdsEndpoint = $rdsInstance.Endpoint.Address

$rdsCredSecret = Get-SECSecretList | where { $_.Name.StartsWith($RdsSecretPrefix) }
$rdsSecret = (Get-SECSecretValue $rdsCredSecret.Name).SecretString

$rdsInstance.DBSecurityGroups[0]

# $rdsEndpoint
# $rdsInstance.MasterUsername
# $rdsSecret

"Data Source=$rdsEndpoint; Initial Catalog=MusicStore; User Id=$($rdsInstance.MasterUsername); Password=$rdsSecret" | Out-File C:\MusicStoreDbString.txt
"Data Source=$rdsEndpoint; Initial Catalog=Identity; User Id=$($rdsInstance.MasterUsername); Password=$rdsSecret" | Out-File C:\IdentityDbString.txt

& $env:WINDIR\Microsoft.Net\Framework\v4.0.30319\aspnet_regsql.exe -S $rdsEndpoint -d Identity -A all -U $($rdsInstance.MasterUsername) -P $rdsSecret

$User_script = @'
declare @now datetime
set @now= GETDATE()
exec aspnet_Membership_CreateUser 'MvcMusicStore','admin','Pass@word1',
    '','admin@amazon.com','','',1,@now,@now,0,0,null


EXEC aspnet_Roles_CreateRole 'MvcMusicStore', 'Administrator'
EXEC aspnet_UsersInRoles_AddUsersToRoles 'MvcMusicStore', 'admin', 'Administrator', 8 
'@

& Invoke-SqlCmd -ServerInstance $rdsEndpoint -Database 'Identity' -Username $($rdsInstance.MasterUsername) -Password $rdsSecret -Query $User_script

$AlbumView = @'
CREATE VIEW AlbumsWithArtistAndGenre AS (
  SELECT a.*, G.Name as Genre, ar.Name as Artist FROM
    Albums a
  Inner JOIN 
    Genres G 
  ON
    a.GenreId = g.GenreId
  Inner JOIN
    Artists ar
  ON 
    a.ArtistId = ar.ArtistId
  
);
'@

& Invoke-SqlCmd -ServerInstance $rdsEndpoint -Database 'MusicStore' -Username $($rdsInstance.MasterUsername) -Password $rdsSecret -Query $AlbumView

```