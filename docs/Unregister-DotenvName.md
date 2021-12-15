---
external help file: Dotenv-help.xml
Module Name: Dotenv
online version:
schema: 2.0.0
---

# Unregister-DotenvName

## SYNOPSIS
Unregisters a name from the list of env file names.

## SYNTAX

```
Unregister-DotenvName [-Name] <String> [<CommonParameters>]
```

## DESCRIPTION
Unregisters a name from the list of env file names.

## EXAMPLES

### Example 1
```powershell
PS C:\> Unregister-DotenvName ".env"
```

## PARAMETERS

### -Name
Name to unregister.

```yaml
Type: String
Parameter Sets: (All)
Aliases:
Accepted values: .env

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
