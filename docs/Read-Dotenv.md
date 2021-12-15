---
external help file: Dotenv.dll-Help.xml
Module Name: Dotenv
online version:
schema: 2.0.0
---

# Read-Dotenv

## SYNOPSIS
Parses an env file.

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
Parses an env file.
The parsed variables are not sourced, the caller is expected to do it.
You don't have to call this command, the module uses it under the hood.

## EXAMPLES

### Example 1
```powershell
PS C:\> Read-Dotenv ./.env
```

## PARAMETERS

### -IgnoreExportPrefix
Ignore the \`export \` prefix in env variables (POSIX shells have this keyword for exporting env variables).

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

### -Path
Path to a env file.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.String

## OUTPUTS

### Dotenv.EnvEntry

## NOTES

## RELATED LINKS
