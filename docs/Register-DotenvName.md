---
external help file: Dotenv-help.xml
Module Name: Dotenv
online version:
schema: 2.0.0
---

# Register-DotenvName

## SYNOPSIS
Adds a new name to the list of env file names this module will check for.

## SYNTAX

```
Register-DotenvName [-Name] <String> [<CommonParameters>]
```

## DESCRIPTION
Adds a new name to the list of env file names this module will check for.
Equivalent to `$Dotenv.AddName()`.

## EXAMPLES

### Example 1
```powershell
PS C:\> Register-Dotenvname .myCustomEnv
```

## PARAMETERS

### -Name
The name to add.
The name must be a valid filename and not a path.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None

## OUTPUTS

### System.Object
## NOTES

## RELATED LINKS
