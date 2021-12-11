@{
	RootModule = "Dotenv.psm1"
	ModuleVersion = "0.1.0"
	Author = "insomnia"

	FunctionsToExport = @(
		"Update-Dotenv"
		"Enable-Dotenv"
		"Disable-Dotenv"
		"Show-Dotenv"
		"Add-DotenvName"
		"Remove-DotenvName"
		"Set-DotenvLogLevel"
		"Get-DotenvLogLevel"
	)
	CmdletsToExport = @("Read-Dotenv")
	VariablesToExport = @()
	AliasesToExport = @()

	NestedModules = @(
		"Dotenv.dll"
		"Dotenv.Logging.ps1"
	)
}
