New-Variable -Option Constant Dotenv ([Dotenv.Daemon]::new())

[string]$lastdir = $pwd.providerpath

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
	if(!$force -and $pwd.providerpath -eq $script:lastdir ) {
		return
	}
	$script:lastdir = $pwd.providerpath
	if($script:Dotenv.Parallel) {
		Clear-DotenvJobs
		Start-ThreadJob -Name "Dotenv" -ArgumentList $script:Dotenv, $force {
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

$exports = @{
	Function = @(
		"Update-Dotenv"
		"Register-DotenvName"
		"Unregister-DotenvName"
		"Enable-Dotenv"
		"Disable-Dotenv"
	)
	Variable = "Dotenv"
	Cmdlet = "Read-Dotenv"
}

Export-ModuleMember @exports
