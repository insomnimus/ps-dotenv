using namespace System.Management.Automation

function expand-env($s) {
	$ExecutionContext.InvokeCommand.ExpandString($s)
}

class DotenvEntry {
	[string]$name
	# [string]$value
	[string]$replaced

	DotenvEntry([Dotenv.EnvEntry]$entry) {
		$this.replaced = get-item "env:\$($entry.name)" -errorAction ignore
		# If it's double quoted or bare, expand env vars.
		if(-not ($entry.quote -eq [Dotenv.QuoteKind]::Single -or $entry.quote -eq [Dotenv.QuoteKind]::MultiSingle)) {
			$entry.value = expand-env ($entry.value -replace '\$([^"])', '$env:$1')
		}
		set-item "env:/$($entry.name)" $entry.value
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
				set-item "env:\$name" $x.replaced
			} else {
				remove-item "env:\$name" -errorAction ignore
			}
		}
	}
}

[System.Collections.Generic.List[Dotenv]]$envs = [System.Collections.Generic.List[Dotenv]]::new(32)
[string]$Lastdir = $PWD.path

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
		return
	}

	$vars = read-dotenv $path -ignoreErrors | % { [DotenvEntry]::new($_) }
	if($vars) {
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
