---
external help file: Dotenv-help.xml
Module Name: Dotenv
online version:
schema: 2.0.0
---

# Remove-DotenvPattern

## SYNOPSIS
Removes a pattern from the dotenv whitelist.

## SYNTAX

```
Remove-DotenvPattern [-Pattern] <String[]> [<CommonParameters>]
```

## DESCRIPTION
Removes an entry from the Dotenv whitelist.
This only has an effect with the safe mode enabled.

## EXAMPLES

### Example 1
```powershell
PS C:\> Remove-DotenvPattern "$HOME/**"
```

No description.

## PARAMETERS

### -Pattern
A pattern to remove from the whitelisted patterns.

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
