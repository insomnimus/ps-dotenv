enum LogLevel {
	None
	Info
	Warning
	Error
	Debug
}

[LogLevel]$LogLevel = [LogLevel]::Debug

function :log {
	param(
		[parameter(mandatory, position = 0)]
		[string]$msg,
		[parameter(position = 1)]
		[LogLevel]$lvl = [LogLevel]::Debug
	)
	if($lvl -gt $script:LogLevel) {
		return
	}
	$msg = "[dotenv] $msg"
	switch($lvl) {
		[LogLevel]::Info { write-infrmation $msg; break }
		[LogLevel]::Error { write-error $msg; break }
		[LogLevel]::Warn { write-warning $msg; break }
		[LogLevel]::Debug { write-debug $msg; break }
	}
}

function :info {
	param(
		[parameter(mandatory, position = 0)]
		[string]$msg
	)
	script::log -lvl Info $msg
}

function :error {
	param(
		[parameter(mandatory, position = 0)]
		[string]$msg
	)
	script::log -lvl Error $msg
}

function :warn {
	param(
		[parameter(mandatory, position = 0)]
		[string]$msg
	)
	script::log -lvl Warn $msg
}

function :debug {
	param(
		[parameter(mandatory, position = 0)]
		[string]$msg
	)
	script::log -lvl Debug $msg
}

function Set-DotenvLogLevel {
	param(
		[Parameter(Mandatory, Position = 0)]
		[LogLevel]$Level
	)
	$script:LogLevel = $level
}

function Get-DotenvLogLevel {
	[CmdletBinding()]
	[OutputType([LogLevel])]
	param()
	$script:LogLevel
}
