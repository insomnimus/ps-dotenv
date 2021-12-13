## Disable-Dotenv
Disables the Dotenv module without removing it from the session.


This will unsource any existing env variables parsed from env files. You can enable the module back with the Enable-Dotenv command.
### Examples
```powershell
PS C:\> Disable-Dotenv
```

## Enable-Dotenv
Enables the Dotenv module. This is useful because you can temporarily disable and enable the effects of this module without changing your profile.
### Examples
```powershell
PS C:\> Enable-Dotenv
```

## Show-Dotenv
Shows the current configuration of the Dotenv module.
### Examples
```powershell
PS C:\> Show-Dotenv

Enabled Sourced Names
------- ------- -----
   True         {.env}
```

## Update-Dotenv
Makes the module check for env files to source if it is enabled.


This cmdlet is meant to be called automatically from inside the prompt function.
### Examples
```powershell
PS C:\> Update-Dotenv
```

## Read-Dotenv
Parses a file or text containing env variables in accordance to the dotenv syntax.


Parsed values are not applied to the environment, the user is expected to apply them.
### Examples
```powershell
PS C:\> $env:GOPATH = "D:\go"
PS C:\> $envfile = @'
GOBIN = "$GOPATH/bin"
CC = 'D:\clang\bin\clang.exe'
'@
PS C:\> Read-Dotenv -Text $envfile

Name  Value
----  -----
GOBIN D:\go/bin
CC    D:\clang\bin\clang.exe
```

## Get-DotenvLogLevel
Gets the currently set log level for Dotenv.
### Examples
```powershell
PS C:\> Get-DotenvLogLevel
Debug
```

## Set-DotenvLogLevel
Sets the log level for the Dotenv module.
### Examples
```powershell
PS C:\> Set-DotenvLogLevel 'Info'
```

## Register-DotenvName
Registers a file name that will be considered as an env file.


By default, the only name considered is ".env".
### Examples
```powershell
PS C:\> Register-DotenvName ".myCustomName"
```

## Unregister-DotenvName
Unregisters a name from the list of env file names.
### Examples
```powershell
PS C:\> Unregister-DotenvName ".env"
```
