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
}

class Dotenv {
	[string]$root
	[DotenvEntry[]]$vars

	[void]unsource() {
		foreach($x in $this.vars) {
			$name = $x.name
			if($x.replaced) {
				set-content"env:/$name" $x.replaced
			} else {
				remove-item "env:/$name" -errorAction ignore
			}
		}
	}
}

[System.Collections.Generic.List[Dotenv]]$envs = [System.Collections.Generic.List[Dotenv]]::new(32)
[string]$Lastdir = $PWD.path
[bool]$enabled = $true
[System.Collections.Generic.List[string]]$NamesToSource = [System.Collections.Generic.List[string]]@(".env")

function Source-Dotenv {
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

	$vars = read-dotenv $path -skipErrors | % { [DotenvEntry]::new($_) }
	if($vars) {
		script::debug "sourcing $path ($($vars.length) items)"
		$script:envs.add([Dotenv]@{
				root = $path
				vars = $vars
			})
	} else {
		script::debug "'$path' does not contain any variables, the file is skipped"
	}
}

function should-update {
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
		script::info "unsourcing sourced env files"
		$script:envs.removeAll($script:unsource)
	} elseif(-not (should-update)) {
		return
	}
	$script:lastdir = $pwd.path

	[void]$script:envs.removeAll($filter)
	$dir = $pwd.path

	do {
		foreach($name in $script:NamesToSource) {
			$x = join-path $dir $name
			if(test-path -pathType leaf $x) {
				script:source-dotenv $x
			}
		}
		$dir = split-path -parent $dir
	} while(![System.IO.Path]::EndsInDirectorySeparator($dir))
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

function Show-Dotenv {
	[CmdletBinding()]
	[OutputType([PSCustomObject])]
	param(
		[switch]$Enabled,
		[switch]$Sourced,
		[switch]$Vars,
		[switch]$LogLevel,
		[switch]$Names
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
		Enabled = $script:enabled
		Sourced = $script:envs.root
		Vars = $script:envs.vars
		LogLevel = $script:LogLevel
		Names = $script:NamesToSource
	}
}

function Register-DotenvName {
	[CmdletBinding()]
	param(
		[Parameter(Mandatory, Position = 0)]
		[string]$FileName
	)

	if($script:NamesToSource -contains $filename) {
		write-information "filename already in watch"
	} else {
		$script:NamesToSource.add($filename)
		write-information "added $filename as a .env file name to source"
		script:update-dotenv -force
	}
}

function Unregister-DotenvName {
	[CmdletBinding()]
	param(
		[Parameter(Mandatory, Position = 0)]
		[string]$FileName
	)

	if($script:NamesToSource.remove($filename)) {
		write-information "removed $filename from the list of names to source"
		$script:envs.removeAll({
				param($dotenv)
				if($dotenv.root.endswith($filename, $true, $null)) {
					[void]$dotenv.unsource()
					$true
				}
			})
	} else {
		write-warning "$filename is not in the list of names to source"
	}
}
