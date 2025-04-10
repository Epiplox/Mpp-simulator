----- DOCUMENTATION -----

Doxygen generated HTML (Open with index.html)

----- USER GUIDE (Windows)-----

Download and install .NET 8. (Runtime for running, SDK for building and running)
Create a WPF application with C# and choose the target framework to be .NET 8.
Move the .dll files (3 pcs) to the project directory.
Add reference to the .dll files. (VS 2022 Project -> Add project reference)

Evaluation version shuts down after 1 hour and requires to be restarted.

----- USER GUIDE (Linux, harder option)-----

Download and install .NET 8. (Runtime for running, SDK for building and running)
Add extension ".NET Install Tool" to VS Code, which should also add extensions "C#" and "C# Dev kit".
After that creation of .NET project should be available in the explorer tab.
Create a ASP.NET Core Web App (MVC or Razor Pages), since Wpf applications are Windows exclusive.
Check that the target framework is .NET 8.

The .dll files needs System.Configuration.ConfigurationManager-package so if that is missing,
add it by typing in the terminal:

dotnet add package System.Configuration.ConfigurationManager --version 9.0.3

Move the .dll files (3 pcs) to the project directory.
Manually add references to the .csproj-file:

  <ItemGroup>
    <Reference Include="Tuni.MppOpcUaClientLib">
      <HintPath>..\Tuni.MppOpcUaClientLib.dll</HintPath>
    </Reference>
    <Reference Include="UnifiedAutomation.UaBase">
      <HintPath>..\UnifiedAutomation.UaBase.dll</HintPath>
    </Reference>
    <Reference Include="UnifiedAutomation.UaClient">
      <HintPath>..\UnifiedAutomation.UaClient.dll</HintPath>
    </Reference>
  </ItemGroup>

Evaluation version shuts down after 1 hour and requires to be restarted.
