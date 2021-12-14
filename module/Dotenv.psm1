new-variable Dotenv -Option Constant -Value ([DotenvConfig]::new())

[ScriptBlock]$filter = {
	param($dotenv)
	if(!$pwd.path.startswith($dotenv.root)) {
		[void]$dotenv.unsource()
		$true
	}
}

[ScriptBlock]$unsource = {
	param($dotenv)
	[void]$dotenv.unsource()
	$true
}

class DotenvEntry {
	[string]$name
	[string]$value
	[string]$replaced

	[string]ToString() {
		return "$($this.name) = $($this.value)"
	}

	DotenvEntry([Dotenv.EnvEntry]$entry) {
		$this.replaced = get-content "env:$($entry.name)" -errorAction ignore
		$this.name = $entry.name
		$this.value = $entry.value
		set-item "env:$($entry.name)" $this.value
	}

	[void]unset() {
		if($this.replaced) {
			set-content "env:/$($this.name)" $this.replaced
		} else {
			remove-item "env:\$($this.name)" -errorAction SilentlyContinue
		}
	}
}

class Dotenv {
	[string]$Root
	[string]$Path
	[string]$Name
	[DotenvEntry[]]$vars

	Dotenv([string]$path, [DotenvEntry[]]$vars) {
		$this.vars = $vars
		$this.path = $path
		$this.name = split-path -leaf $path
		$this.root = split-path -parent $path
	}

	[void]unsource() {
		script::info "unsourcing $($this.path)"
		foreach($x in $this.vars) {
			$x.unset()
		}
	}
}

[System.Collections.Generic.List[Dotenv]]$envs = [System.Collections.Generic.List[Dotenv]]::new(32)
[string]$Lastdir = $PWD.path

function :source-dotenv {
	param(
		[parameter(mandatory, position = 0)]
		[string]$path
	)
	# Do not parse the file if it's already sourced.
	if($script:envs.exists({
				param($dotenv)
				if($dotenv.root -eq $path) { $true }
			})) {
		script::info "skipped sourcing $path because it's already sourced"
		return
	}

	$vars = read-dotenv $path -skipErrors:$script:skipErrors -ignoreExportPrefix:$script:IgnoreExportPrefix `
	| % { [DotenvEntry]::new($_) }

	if($vars) {
		script::debug "sourcing $path ($($vars.length) items)"
		$script:envs.add([Dotenv]::new($path, $vars))
	} else {
		script::debug "'$path' does not contain any variables, the file is skipped"
	}
}

function :should-update {
	$pwd.path -ne $script:lastdir
}

function Update-Dotenv {
	[CmdletBinding()]
	param(
		[parameter(HelpMessage = "Forces the command to re-evaluate .env files found in the current directory and its parents.`nSetting this flag won't have an effect if the module level state is disabled (using Disable-Dotenv).")]
		[switch]$force
	)
	if(!$script:enabled) {
		return
	}
	if($force) {
		script::info "unsourcing $($script:envs.count) env files"
		$script:envs.removeAll($script:unsource)
	} elseif(-not (script::should-update)) {
		return
	}
	$script:lastdir = $pwd.path

	[void]$script:envs.removeAll($filter)
	$dir = $pwd.path

	while($true) {
		foreach($name in $script:names) {
			$x = join-path $dir $name
			if(test-path -pathType leaf $x) {
				script::source-dotenv $x
			}
		}
		if([System.IO.Path]::EndsInDirectorySeparator($dir)) {
			break
		}
		$dir = split-path -parent $dir
	}
}

function Enable-Dotenv {
	if(!$script:enabled) {
		$script:enabled = $true
		script:update-dotenv -force
	}
}

function Disable-Dotenv {
	if($script:enabled) {
		$script:envs.removeAll($script:unsource)
		$script:enabled = $false
	}
}

function Register-DotenvName {
	[CmdletBinding()]
	param(
		[Parameter(Mandatory, Position = 0)]
		[string]$Name
	)

	if(script::contains $script:names $name) {
		write-warning "$name is already registered"
	} else {
		$script:names += $name
		script:update-dotenv -force
	}
}

function Unregister-DotenvName {
	[CmdletBinding()]
	param(
		[Parameter(Mandatory, Position = 0)]
		[string]$Name
	)

	if($script:NamesToSource.removeAll({
				param($s)
				if(script::eq $s $name) { $true }
			})) {
		write-information "removed $name from the list of names"
		[void] $script:envs.removeAll({
				param($dotenv)
				if(script::eq $name $dotenv.name) {
					[void]$dotenv.unsource()
					$true
				}
			})
	} else {
		write-warning "$name is not in the list of names"
	}
}

function Show-Dotenv {
	[CmdletBinding()]
	[OutputType([PSCustomObject])]
	param(
		[Parameter(HelpMessage = "Show if the module is enabled or not.")]
		[switch]$Enabled,
		[Parameter(HelpMessage = "Show the list of files that are currently sourced.")]
		[switch]$Sourced,
		[Parameter(HelpMessage = "Show the variables currently set by this module.")]
		[switch]$Vars,
		[Parameter(HelpMessage = "Show the currently configured logging preference for Dotenv.")]
		[switch]$LoggingPreference,
		[Parameter(HelpMessage = "Show the list of names that are being considered as env files.")]
		[switch]$Names,
		[Parameter(HelpMessage = "Show if errors are skipped while parsing.")]
		[switch]$SkipErrors,
		[Parameter(HelpMessage = "Show if the export keyword is ignored before a variable name.")]
		[switch]$IgnoreExportPrefix
	)

	$default = $MyInvocation.BoundParameters.count -eq 0
	$members = if($default) {
		"Enabled", "Sourced", "Names"
	} else {
		$MyInvocation.BoundParameters.keys | % { "$_" }
	}

	$typeData = @{
		TypeName = "Dotenv.Status"
		DefaultDisplayPropertySet = $members
	}

	update-typedata -force @typedata
	[PSCustomObject]@{
		PSTypeName = "Dotenv.Status"
		Enabled = $script:Enabled
		LoggingPreference = $script:log
		Sourced = $script:envs.path
		Vars = $script:envs.vars
		Names = $script:names
		SkipErrors = $script:SkipErrors
		IgnoreExportPrefix = $script:IgnoreExportPrefix
	}
}

$exports = @{
	Variable = "Dotenv"
	Cmdlet = "Read-Dotenv"
	Function = @(
		"Update-Dotenv"
		"Enable-Dotenv"
		"Disable-Dotenv"
		"Show-Dotenv"
		"Register-DotenvName"
		"Unregister-DotenvName"
	)
}

Export-ModuleMember @exports