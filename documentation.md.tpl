# Dotenv

## Installation and Setup
See the [readme](readme.md).

## The `$Dotenv` variable.
This variable is exported and holds the configuration for the entire module.
You cannot remove or replace it but you can modify its properties, which take effect immediately.

For example, instead of running `Disable-Dotenv`, you can run `$Dotenv.Enabled = $false`.

## Configuration
The `$Dotenv` variable is how you configure the modules behaviour.
These are its fields:

-	`Enabled`: Turns the module on and off.
-	`LoggingPreference`: Configures how the logs are written, you can tab through its fields.
-	`Parallel`: If set to true, everytime `Update-Dotenv` is called, it will be done in a new thread therefore will not block.
-	`Names`: Controls which names are considered env files. You should either call its methods `AddName()` and `RemoveName()` or use the cmdlets `Register-DotenvName` and `Unregister-DotenvName`.
-	`IgnoreExportPrefix`: If set to true, the `export` keyword before a variable name will be ignored (this is common in *NIX systems).
-	`SkipErrors`: If set to true, errors encountered during parsing env files will cause the parser to skip to the next line instead of returning.
