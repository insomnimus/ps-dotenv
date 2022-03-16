---
external help file: Dotenv-help.xml
Module Name: Dotenv
online version:
schema: 2.0.0
---

# Add-DotenvPattern

## SYNOPSIS
Adds a glob pattern to the whitelist.

## SYNTAX

```
Add-DotenvPattern [-Pattern] <String[]> [<CommonParameters>]
```

## DESCRIPTION
Adds a glob pattern to the whitelist.
This only has an effect with the safe mode enabled.

## EXAMPLES

### Example 1
```powershell
PS C:\> Add-DotenvPattern "$HOME/**"
```

This example whitelists any potential env file under the users home directory.

## PARAMETERS

### -Pattern
A glob pattern to whitelist.

```yaml
Type: String[]
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
