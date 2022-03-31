# Dotenv

## Installation and Setup
See the [readme](readme.md).

## The `$Dotenv` variable.
This variable is exported and holds the configuration for the entire module.
You cannot remove or replace it (without force) but you can modify its properties, which take effect immediately.

For example, instead of running `Disable-Dotenv`, you can run `$Dotenv.Enabled = $false`.

## Configuration
The `$Dotenv` variable is how you configure the modules behaviour.
These are its fields:

-	`Enabled`: Turns the module on and off.
-	`LoggingPreference`: Configures how the logs are written, you can tab through its fields.
-	`Parallel`: If set to true, everytime `Update-Dotenv` is called, it will be done in a new thread therefore will not block.
-	`Names`: Controls which names are considered env files. You should either call its methods `AddName()` and `RemoveName()` or use the cmdlets `Register-DotenvName` and `Unregister-DotenvName`.
-	`SkipErrors`: If set to true, errors encountered during parsing env files will cause the parser to skip to the next line instead of returning.
-	`SafeMode`: If enabled, only the files explicitly allowed will be sourced.
-	`Quiet`: If set to `$true`, disables info messages while the safe mode is enabled and there are unauthorized files in the current directory or its parents.
-	`AuthorizedPatterns`: A read-only list containing the patterns you whitelisted.
## Read-Dotenv
Parses an env file. The parsed variables are not sourced, the caller is expected to do it. You don't have to call this command, the module uses it under the hood.

## Disable-Dotenv
Disables the module without removing it from the session. Equivalent to `$Dotenv.Enabled = $true`.

## Enable-Dotenv
Enables the module back. Equivalent to `$Dotenv.Enabled = $true`.

## Update-Dotenv
Triggers the module to check for env files in the current and parent directories. This is the entrypoint to this module. This command is meant to be called automatically by your `Prompt` function.

## Approve-DotenvDir
Whitelists a directory for dotenv. Every existing and future env files under the directory (recursively) will be allowed. This only has an effect with the safe mode enabled.

## Approve-DotenvFile
Whitelists a particular env file for dotenv. This only has an effect with the safe mode enabled. With the safe mode, files not explicitly allowed by you will not be sourced.

## Deny-DotenvFile
Removes a file from the list of authorized files. This only has an effect with the safe mode enabled. With the safe mode, files not explicitly allowed by you will not be sourced.

## Register-DotenvName
Adds a new name to the list of env file names this module will check for. Equivalent to `$Dotenv.AddName()`.

## Unregister-DotenvName
Removes a name from the list of names this module will consider as an env file. Equivalent to `$Dotenv.RemoveName()`.

## Add-DotenvPattern
Adds a glob pattern to the whitelist. This only has an effect with the safe mode enabled.

## Remove-DotenvPattern
Removes an entry from the Dotenv whitelist. This only has an effect with the safe mode enabled.
