@{
	RootModule = "Dotenv.psm1"
	ModuleVersion = "0.1.0"
	Author = "insomnia"

	FunctionsToExport = @(
		"Update-Dotenv"
		"Enable-Dotenv"
		"Disable-Dotenv"
		"Show-Dotenv"
	)
	CmdletsToExport = @("Read-Dotenv")
	VariablesToExport = @()
	AliasesToExport = @()

	NestedModules = @(
		"Dotenv.dll"
	)
}
