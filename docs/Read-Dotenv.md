---
external help file: Dotenv.dll-Help.xml
Module Name: Dotenv
online version:
schema: 2.0.0
---

# Read-Dotenv

## SYNOPSIS
Parses a file containing env variables according to the dotenv file format.

## SYNTAX

### file (Default)
```
Read-Dotenv [-Path] <String> [-SkipErrors] [-IgnoreExportPrefix] [<CommonParameters>]
```

### text
```
Read-Dotenv [-Text] <String> [-SkipErrors] [-IgnoreExportPrefix] [<CommonParameters>]
```

## DESCRIPTION
Parses a file or text containing env variables in accordance to the dotenv syntax.

Parsed values are not applied to the environment, the user is expected to apply them.

## EXAMPLES

### Example 1
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

## PARAMETERS

### -Path
Path to a .env file.

```yaml
Type: String
Parameter Sets: file
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -SkipErrors
Skip syntax errors if any are encountered.
The default behaviour is to stop parsing.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Text
The text input to parse.

```yaml
Type: String
Parameter Sets: text
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName, ByValue)
Accept wildcard characters: False
```

### -IgnoreExportPrefix
Ignore the `export ` prefix in env variables (POSIX shells have this keyword for exporting env variables).

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.String

## OUTPUTS

### Dotenv.EnvEntry

## NOTES

## RELATED LINKS
