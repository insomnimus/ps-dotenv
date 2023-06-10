New-Variable -Option ReadOnly Dotenv ([Dotenv.Daemon]::new())

$ExecutionContext.SessionState.Module.OnRemove += {
	if($global:Dotenv.Enabled) {
		Write-Host "dotenv: unloading..."
		$global:Dotenv.Disable()
		remove-item -force -ea silentlyContinue variable:/Dotenv
	}
}

[string]$lastdir = ""

function Clear-DotenvJobs {
	Get-Job -State Completed `
	| Where-Object { $_.name -eq "Dotenv" } `
	| Remove-Job
}

function Update-Dotenv {
	[CmdletBinding()]
	param (
		[Parameter(HelpMessage = "Forces the module to reload every env file if any.")]
		[switch]$Force
	)
	if($pwd.provider.name -ne "FileSystem" -or (!$force -and $pwd.providerpath -eq $script:lastdir)) {
		return
	}
	$script:lastdir = $pwd.providerpath
	if(!$script:Dotenv.Enabled) {
		return
	}
	if($script:Dotenv.Async) {
		Clear-DotenvJobs
		$null = Start-ThreadJob -Name "Dotenv" -ArgumentList $script:Dotenv, $force {
			param(
				[Dotenv.Daemon]$Daemon,
				[bool]$force
			)
			if($force) {
				$daemon.Clear()
			}
			$Daemon.Update($pwd.providerpath)
		}
	} else {
		if($force) {
			$script:dotenv.clear()
		}
		$script:dotenv.update($pwd.providerpath)
	}
}

class UnregisterDotenvNameValidator: System.Management.Automation.IValidateSetValuesGenerator {
	[string[]] GetValidValues() {
		return $script:dotenv.names
	}
}

function Register-DotenvName {
	[CmdletBinding()]
	param(
		[Parameter(Mandatory, Position = 0, HelpMessage = "The name to add. The name must be a valid filename and not a path.")]
		[ValidateScript({
				foreach($c in ([System.IO.Path]::GetInvalidFilenameChars())) {
					if($_.contains($c)) { return $false }
				}
				$true
			})]
		[string]$Name
	)
	if($script:Dotenv.AddName($name)) {
		write-information "added $name"
	} else {
		write-warning "$name already exists"
	}
}

function Unregister-DotenvName {
	[CmdletBinding()]
	param(
		[Parameter(Mandatory, Position = 0, HelpMessage = "The name to remove.")]
		[ValidateSet([UnregisterDotenvNameValidator])]
		[string]$Name
	)
	if($script:Dotenv.RemoveName($name)) {
		write-information "removed $name"
	} else {
		write-warning "$name does not exist"
	}
}

function Enable-Dotenv {
	$script:Dotenv.Enabled = $true
}

function Disable-Dotenv {
	$script:Dotenv.Enabled = $false
}

function Approve-DotenvFile {
	[CmdletBinding()]
	param(
		[Parameter(
			Mandatory,
			Position = 0,
			HelpMessage = "Path to an env file or a directory to whitelist."
		)]
		[string[]]$Path
	)
	$yes = $false
	foreach($f in $path) {
		$f = [System.IO.Path]::GetFullPath($f, $pwd.providerpath)
		if($script:Dotenv.AuthorizePattern($f, $false)) {
			write-information "allowed $f"
			$yes = $true
		} else {
			write-warning "$f is already allowed"
		}
	}
	if($yes) {
		script:update-dotenv -force
	}
}

function Approve-DotenvDir {
	[CmdletBinding()]
	param(
		[Parameter(
			Mandatory,
			Position = 0,
			HelpMessage = "A directory to whitelist; every file under it will be recursively allowed."
		)]
		[ValidateScript({ (test-path -type container $_) -or (throw "path must target to an existing directory") })]
		[string[]]$Path
	)
	$yes = $false
	foreach($p in $path) {
		$yes = $script:Dotenv.AuthorizeDirectory([System.IO.Path]::GetFullPath($p, $pwd.providerpath)) -or $yes
	}
	if($yes) {
		script:update-dotenv -force
	}
}

function Deny-DotenvFile {
	[CmdletBinding()]
	param(
		[Parameter(
			Mandatory,
			Position = 0,
			HelpMessage = "Path to an env file to deny."
		)]
		[ArgumentCompleter({ $script:Dotenv.AuthorizedPatterns | where-object { [WildcardPattern]::ContainsWildcardCharacters("$_") } | sort-object })]
		[string[]]$Path
	)
	$yes = $false
	foreach($f in $path) {
		$f = [System.IO.Path]::GetFullPath($f, $pwd.providerpath)
		if($script:Dotenv.UnauthorizePattern($f, $false)) {
			write-information "denied $f"
			$yes = $true
		} else {
			write-warning "$f is not allowed"
		}
	}
	if($yes) {
		script:update-dotenv -force
	}
}

function Add-DotenvPattern {
	[CmdletBinding()]
	param(
		[Parameter(
			Mandatory,
			Position = 0,
			HelpMessage = "A glob pattern to whitelist."
		)]
		[string[]]$Pattern
	)
	$yes = $false
	foreach($p in $pattern) {
		$yes = $script:Dotenv.AuthorizePattern($p) -or $yes
	}
	if($yes) {
		script:update-dotenv -force
	}
}

function Remove-DotenvPattern {
	[CmdletBinding()]
	param(
		[Parameter(
			Mandatory,
			Position = 0,
			HelpMessage = "A pattern to remove from the whitelisted patterns."
		)]
		[ArgumentCompleter({ $script:Dotenv.AuthorizedPatterns | sort-object })]
		[string[]]$Pattern
	)
	$yes = $false
	foreach($p in $pattern) {
		if($script:Dotenv.UnauthorizePattern($p)) {
			write-information "un-whitelisted $p"
			$yes = $true
		} else {
			write-error "$p is not whitelisted"
		}
	}
	if($yes) {
		script:update-dotenv -force
	}
}

$exports = @{
	Function = @(
		"Update-Dotenv"
		"Register-DotenvName"
		"Unregister-DotenvName"
		"Enable-Dotenv"
		"Disable-Dotenv"
		"Approve-DotenvFile"
		"Approve-DotenvDir"
		"Deny-DotenvFile"
		"Add-DotenvPattern"
		"Remove-DotenvPattern"
	)
	Variable = "Dotenv"
	Cmdlet = "Read-Dotenv"
}

Export-ModuleMember @exports
