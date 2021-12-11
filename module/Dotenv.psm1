using namespace System.Management.Automation

function expand-env($s) {
	$ExecutionContext.InvokeCommand.ExpandString($s)
}

function log($msg) {
	write-debug "[Dotenv][debug] $msg"
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
		# If it's double quoted or bare, expand env vars.
		if(-not ($entry.quote -eq [Dotenv.QuoteKind]::Single -or $entry.quote -eq [Dotenv.QuoteKind]::MultiSingle)) {
			$this.value = expand-env ($entry.value -replace '\$([^"])', '$env:$1')
		} else {
			$this.value = $entry.value
		}
		set-item "env:$($entry.name)" $this.value
		$this.name = $entry.name
	}
}

class Dotenv {
	[string]$root
	[DotenvEntry[]]$vars

	[void]unsource() {
		foreach($x in $this.vars) {
			$name = $x.name
			if($x.replaced) {
				set-content"env:$name" $x.replaced
			} else {
				remove-item "env:$name" -errorAction ignore
			}
		}
	}
}

[System.Collections.Generic.List[Dotenv]]$envs = [System.Collections.Generic.List[Dotenv]]::new(32)
[string]$Lastdir = $PWD.path
[bool]$enabled = $true

[ScriptBlock]$filter = {
	param($dotenv)
	if(!$pwd.path.startswith($dotenv.root)) {
		[void]$dotenv.unsource()
		$true
	}
}

function Source-Dotenv {
	param(
		[parameter(mandatory, position = 0)]
		[string]$path
	)
	if($script:envs.exists({
				param($dotenv)
				if($dotenv.root -eq $path) { $true }
			})) {
		script:log "skipped sourcing $path because it's already sourced"
		return
	}

	$vars = read-dotenv $path -ignoreErrors | % { [DotenvEntry]::new($_) }
	if($vars) {
		script:log "sourcing $path ($($vars.length) items)"
		$script:envs.add([Dotenv]@{
				root = $path
				vars = $vars
			})
	}
}

function should-update {
	$pwd.path -ne $script:lastdir
}

function Update-Dotenv {
	[CmdletBinding()]
	param(
		[parameter()]
		[switch]$force
	)
	if(!$script:enabled) {
		return
	}
	if($force) {
		$script:envs.clear()
	} elseif(-not (should-update)) {
		return
	}
	$script:lastdir = $pwd.path

	[void]$script:envs.removeAll($filter)
	$dir = $pwd.path

	do {
		$x = join-path $dir ".env"
		if(test-path -pathtype leaf $x) {
			source-dotenv $x
		}
		$dir = split-path -parent $dir
	} while(![System.IO.Path]::EndsInDirectorySeparator($dir))
}

function Enable-Dotenv {
	if(!$script:enabled) {
		$script:enabled = $true
		update-dotenv -force
	}
}

function Disable-Dotenv {
	if($script:enabled) {
		$script:envs.clear()
		$script:enabled = $false
	}
}

function Show-Dotenv {
	@{
		Enabled = $script:enabled
		Sourced = $script:envs.root
		Vars = $script:envs.vars
	}
}
