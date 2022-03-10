# PS-Dotenv
PS-Dotenv is an unintrusive yet fully featured `dotenv` module for `Powershell`.

## Features
- Complete syntax support including multiline strings and string interpolation.
-	Works on any platform where Powershell Core runs.
-	Logging with configurable log levels.
-	Option to turn the module on and off on the fly.
-	Option to add/remove custom filenames for auto sourcing of env files.
-	A subset of bash-like parameter expansion: `$val`, `${val}`, `${val?error}`, `${val+replacement}` and `${val-default}`.
- Opt-in safe mode where files have to be explicitly allowd.
-	Smart loading and unloading env variables: you won't lose the previously set values upon unloading, the "replaced" values are remembered and kindly reset to their original values.
-	It just works the way you'd expect it to.

## Use Case
This module aims to provide the same functionality as [direnv](https://direnv.net/).

Add `Update-Dotenv` in your `Prompt` function and as you navigate directories, Dotenv will source the appropriate env files in the current directory and its parents.

## Performance
The parsing of the env files is done in `C#` with a hand-written parser.

Dotenv, when there are env files under $PWD or its parents, adds approximately 2 milliseconds to the prompt, which itself takes about 5 milliseconds.
If no env files are detected, there's pretty much no overhead.

## Building the Project
Make sure you have all the requirements installed:

-	`git`: To clone the repository.
-	`dotnet cli` with `dotnet 6.0` or above: To build the project.
-	`Powershell` version 6.0 or above: To run the build script.

To build the project, run the commands below.

```powershell
git clone https://github.com/insomnimus/ps-dotenv
cd ps-dotenv
git checkout main # This is sometimes required.
./build.ps1 -release
# Now import the module.
Import-Module ./Dotenv
```

## Usage
For the env files to be automatically sourced, you'll need to configure your prompt to let `Dotenv` know you possibly changed directories.

You don't need to check if the current directory changed or if there are files that must be loaded since `Dotenv` takes care of that for you by keeping its own state.
If you don't have a powershell profile setup yet, please read [this article from Microsoft](https://docs.microsoft.com/en-us/powershell/module/microsoft.powershell.core/about/about_profiles?view=powershell-7.2) first.

> Important: Dotenv needs to be manually imported because it requires no user interaction (modules are imported when they are first used by the user).

First, add this in your profile (don't forget to replace `C:\example\dotenv` with the actual path to the folder you built in above stesps):

`Import-Module C:\example\dotenv`

Depending on if you have a custom prompt follow one of the following steps:

### If you already have a customized prompt
You just have to add the following snippet inside your prompt function:

```powershell
# If we're in a filesystem path, tell dotenv to check itself.
# We also check if the command exists to not cause errors.
if((Test-Path function:/Update-Dotenv) -and $PWD.Provider.Name -eq "FileSystem") { Dotenv\Update-Dotenv }
```

### If you haven't customized your prompt
In your powershell profile, define a new prompt (the snippet below is the built-in prompt with dotenv enabled):

```powershell
function prompt {
	# Print the built-in prompt:
	$(if (Test-Path variable:/PSDebugContext) { '[DBG]: ' }
		else { '' }) + 'PS ' + $(Get-Location) +
	$(if ($NestedPromptLevel -ge 1) { '>>' }) + '> '

	# If we're in a filesystem path, tell dotenv to check itself.
	# We also check if the command exists to not cause any errors.
	if((Test-Path function:/Update-Dotenv) -and $PWD.Provider.Name -eq "FileSystem") { Dotenv\Update-Dotenv }
}
```

## Documentation

-	[All in one page](documentation.md)
-	[Individual command docs](docs/)
-	[Syntax](syntax.md)

## Commands Overview
- `Read-Dotenv`: Parses an env file. 
- `Disable-Dotenv`: Disables the module without removing it from the session. 
- `Enable-Dotenv`: Enables the module back. 
- `Update-Dotenv`: Triggers the module to check for env files in the current and parent directories. 
- `Approve-DotenvFile`: Authorizes a file for the module. 
- `Deny-DotenvFile`: Unauthorizes an env file. 
- `Register-DotenvName`: Adds a new name to the list of env file names this module will check for. 
- `Unregister-DotenvName`: Removes a name from the list of names this module will consider as an env file. 

