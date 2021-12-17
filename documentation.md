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

## Read-Dotenv
Parses an env file. The parsed variables are not sourced, the caller is expected to do it. You don't have to call this command, the module uses it under the hood.

## Disable-Dotenv
Disables the module without removing it from the session. Equivalent to `$Dotenv.Enabled = $true`.

## Enable-Dotenv
Enables the module back. Equivalent to `$Dotenv.Enabled = $true`.

## Update-Dotenv
Triggers the module to check for env files in the current and parent directories. This is the entrypoint to this module. This command is meant to be called automatically by your `Prompt` function.

## Register-DotenvName
Adds a new name to the list of env file names this module will check for. Equivalent to `$Dotenv.AddName()`.

## Unregister-DotenvName
Removes a name from the list of names this module will consider as an env file. Equivalent to `$Dotenv.RemoveName()`.
