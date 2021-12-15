---
external help file: Dotenv-help.xml
Module Name: Dotenv
online version:
schema: 2.0.0
---

# Register-DotenvName

## SYNOPSIS
Registers a file name that will be considered as an env file.

## SYNTAX

```
Register-DotenvName [-Name] <String> [<CommonParameters>]
```

## DESCRIPTION
Registers a file name that will be considered as an env file.

By default, the only name considered is ".env".

## EXAMPLES

### Example 1
```powershell
PS C:\> Register-DotenvName ".myCustomName"
```

## PARAMETERS

### -Name
The name to register.

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
