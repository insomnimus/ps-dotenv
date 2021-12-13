---
external help file: Dotenv-help.xml
Module Name: Dotenv
online version:
schema: 2.0.0
---

# Update-Dotenv

## SYNOPSIS
Makes the module check for env files to source if it is enabled.

## SYNTAX

```
Update-Dotenv [-force] [<CommonParameters>]
```

## DESCRIPTION
Makes the module check for env files to source if it is enabled.

This cmdlet is meant to be called automatically from inside the prompt function.

## EXAMPLES

### Example 1
```powershell
PS C:\> Update-Dotenv
```

## PARAMETERS

### -force
Forces the command to re-evaluate .env files found in the current directory and its parents.
Setting this flag won't have an effect if the module level state is disabled (using Disable-Dotenv).

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

### None

## OUTPUTS

### System.Object
## NOTES

## RELATED LINKS
