@{
	RootModule = "Dotenv.psm1"
	ModuleVersion = "0.1.0"
	Author = "insomnia"

	FunctionsToExport = @("Update-Dotenv")
	CmdletsToExport = @("Read-Dotenv")
	VariablesToExport = @()
	AliasesToExport = @()

	NestedModules = @(
		"Dotenv.dll"
	)
}
