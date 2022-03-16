---
external help file: Dotenv-help.xml
Module Name: Dotenv
online version:
schema: 2.0.0
---

# Approve-DotenvDir

## SYNOPSIS
Whitelists a directory for dotenv.

## SYNTAX

```
Approve-DotenvDir [-Path] <String[]> [<CommonParameters>]
```

## DESCRIPTION
Whitelists a directory for dotenv.
Every existing and future env files under the directory (recursively) will be allowed.
This only has an effect with the safe mode enabled.

## EXAMPLES

### Example 1
```powershell
PS C:\> Approve-DotenvDir $HOME
```

This example whitelists the users home directory.

## PARAMETERS

### -Path
A directory to whitelist; every file under it will be recursively allowed.

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
