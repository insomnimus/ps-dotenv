---
external help file: Dotenv-help.xml
Module Name: Dotenv
online version:
schema: 2.0.0
---

# Update-Dotenv

## SYNOPSIS
Triggers the module to check for env files in the current and parent directories.

## SYNTAX

```
Update-Dotenv [-Force] [<CommonParameters>]
```

## DESCRIPTION
Triggers the module to check for env files in the current and parent directories.
This is the entrypoint to this module.
This command is meant to be called automatically by your `Prompt` function.

## EXAMPLES

### Example 1
```powershell
PS C:\> Update-Dotenv
```

## PARAMETERS

### -Force
Forces the module to reload every env file if any.

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
