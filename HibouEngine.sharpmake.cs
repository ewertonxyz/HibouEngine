using System.Collections.Generic;
using System.IO;
using Sharpmake;

[Sharpmake.Generate]
public class EngineProject : Project
{
    public EngineProject()
    {
        Name = "Engine";
        SourceRootPath = @"[project.SharpmakeCsPath]\Engine";

        SourceFilesExcludeRegex.Add(@".*vendor[\\/]EASTL[\\/]test[\\/].*");
        SourceFilesExcludeRegex.Add(@".*vendor[\\/]EASTL[\\/]benchmark[\\/].*");
        SourceFilesExcludeRegex.Add(@".*vendor[\\/]EABase[\\/]test[\\/].*");
        SourceFilesExcludeRegex.Add(@".*vendor[\\/]stb[\\/]tests[\\/].*");
        SourceFilesExcludeRegex.Add(@".*vendor[\\/]TinyEXR[\\/]deps[\\/].*");
        SourceFilesExcludeRegex.Add(@".*vendor[\\/]TinyEXR[\\/]examples[\\/].*");
        SourceFilesExcludeRegex.Add(@".*vendor[\\/]TinyEXR[\\/]test[\\/].*");
        SourceFilesExcludeRegex.Add(@".*vendor[\\/]TinyEXR[\\/]experimental[\\/].*");
        SourceFilesExcludeRegex.Add(@".*vcpkg_installed[\\/].*");

        SourceFilesExcludeRegex.Add(@".*vendor[\\/]TinyEXR[\\/].*\.c.*");
        SourceFilesExcludeRegex.Add(@".*vendor[\\/]stb[\\/].*\.c.*");

        AddTargets(new Target(Platform.win64, DevEnv.vs2026, Optimization.Debug | Optimization.Release));
    }

    [Configure]
    public void ConfigureAll(Project.Configuration configurationInstance, Target targetInstance)
    {
        configurationInstance.ProjectFileName = "[project.Name]";
        configurationInstance.ProjectPath = @"[project.SharpmakeCsPath]\Engine";

        configurationInstance.Output = Configuration.OutputType.Dll;
        configurationInstance.Options.Add(Options.Vc.Compiler.CppLanguageStandard.CPP20);
        configurationInstance.Options.Add(Options.Vc.Compiler.Exceptions.Enable);

        if (targetInstance.Optimization == Optimization.Debug)
        {
            configurationInstance.Options.Add(Options.Vc.Compiler.RuntimeLibrary.MultiThreadedDebug);
        }
        else
        {
            configurationInstance.Options.Add(Options.Vc.Compiler.RuntimeLibrary.MultiThreaded);
        }

        configurationInstance.AdditionalCompilerOptions.Add("/utf-8");

        configurationInstance.PrecompHeader = "Core/EnginePCH.h";
        configurationInstance.PrecompSource = "src/Core/EnginePCH.cpp";

        configurationInstance.PrecompSourceExcludeFolders.Add(@"vendor");

        configurationInstance.Defines.Add("ENGINE_EXPORTS");
        configurationInstance.Defines.Add("WIN32");

        configurationInstance.IncludePaths.Add(@"[project.SourceRootPath]\include");
        configurationInstance.IncludePaths.Add(@"[project.SourceRootPath]\include\Core");
        configurationInstance.IncludePaths.Add(@"[project.SourceRootPath]\include\Graph");
        configurationInstance.IncludePaths.Add(@"[project.SourceRootPath]\include\Graphics");
        configurationInstance.IncludePaths.Add(@"[project.SourceRootPath]\include\Input");
        configurationInstance.IncludePaths.Add(@"[project.SourceRootPath]\include\Interop");
        
        configurationInstance.IncludePaths.Add(@"[project.SourceRootPath]\src");
        configurationInstance.IncludePaths.Add(@"[project.SourceRootPath]\src\Core");
        configurationInstance.IncludePaths.Add(@"[project.SourceRootPath]\src\Graph");
        configurationInstance.IncludePaths.Add(@"[project.SourceRootPath]\src\Graphics");
        configurationInstance.IncludePaths.Add(@"[project.SourceRootPath]\src\Graphics\Abstraction");
        configurationInstance.IncludePaths.Add(@"[project.SourceRootPath]\src\Graphics\OpenGL");
        configurationInstance.IncludePaths.Add(@"[project.SourceRootPath]\src\Input");
        configurationInstance.IncludePaths.Add(@"[project.SourceRootPath]\src\Interop");

        configurationInstance.IncludePaths.Add(@"[project.SourceRootPath]\vendor");
        configurationInstance.IncludePaths.Add(@"[project.SourceRootPath]\vendor\EASTL\include");
        configurationInstance.IncludePaths.Add(@"[project.SourceRootPath]\vendor\EASTL\source");
        configurationInstance.IncludePaths.Add(@"[project.SourceRootPath]\vendor\EABase\include\Common");

        configurationInstance.LibraryFiles.Add("opengl32.lib");

        configurationInstance.CustomProperties.Add("VcpkgEnableManifest", "true");
        configurationInstance.CustomProperties.Add("VcpkgApplocalDeps", "true");

        configurationInstance.CustomProperties.Add("VcpkgUseStatic", "true");
        configurationInstance.Options.Add(Options.Vc.General.PlatformToolset.v145);

        string editorBinaryDirectory = Path.Combine(configurationInstance.ProjectPath, "..", "Editor", "output", "win64", targetInstance.Optimization.ToString(), "net10.0-windows");
        configurationInstance.TargetPath = editorBinaryDirectory;

        configurationInstance.VcxprojUserFile = new Configuration.VcxprojUserFileSettings
        {
            LocalDebuggerWorkingDirectory = @"[project.SharpmakeCsPath]"
        };
    }
}

[Sharpmake.Generate]
public class EditorProject : CSharpProject
{
    public EditorProject()
    {
        Name = "Editor";
        SourceRootPath = @"[project.SharpmakeCsPath]\Editor";
        RootPath = @"[project.SharpmakeCsPath]\Editor";

        ProjectSchema = CSharpProjectSchema.NetCore;

        AddTargets(new Target(Platform.win64, DevEnv.vs2026, Optimization.Debug | Optimization.Release));
    }

    [Configure]
    public void ConfigureAll(Project.Configuration configurationInstance, Target targetInstance)
    {
        configurationInstance.ProjectFileName = "[project.Name]";
        configurationInstance.ProjectPath = @"[project.SharpmakeCsPath]\Editor";

        configurationInstance.Output = Configuration.OutputType.DotNetWindowsApp;

        configurationInstance.CustomProperties.Add("TargetFramework", "net10.0-windows");
        configurationInstance.CustomProperties.Add("UseWPF", "true");
        configurationInstance.CustomProperties.Add("Nullable", "enable");
        configurationInstance.CustomProperties.Add("ImplicitUsings", "enable");
        configurationInstance.CustomProperties.Add("Platforms", "x64");

        configurationInstance.ReferencesByNuGetPackage.Add("Nodify", "7.1.0");
        configurationInstance.ReferencesByNuGetPackage.Add("CommunityToolkit.Mvvm", "8.4.0");

        configurationInstance.IncludePaths.Add(@"[project.SourceRootPath]\Interop");
        configurationInstance.IncludePaths.Add(@"[project.SourceRootPath]\Models");
        configurationInstance.IncludePaths.Add(@"[project.SourceRootPath]\Nodes");
        configurationInstance.IncludePaths.Add(@"[project.SourceRootPath]\ViewModels");
        configurationInstance.IncludePaths.Add(@"[project.SourceRootPath]\Views");

        configurationInstance.AddPublicDependency<EngineProject>(targetInstance);
    }
}

[Sharpmake.Generate]
public class HibouEngineSolution : Sharpmake.Solution
{
    public HibouEngineSolution()
    {
        Name = "HibouEngine";

        ExtraItems["Solution Items"] = new Sharpmake.Strings(
            @"[solution.SharpmakeCsPath]\.gitignore",
            @"[solution.SharpmakeCsPath]\.gitmodules",
            @"[solution.SharpmakeCsPath]\GenerateProjects.bat",
            @"[solution.SharpmakeCsPath]\HibouEngine.sharpmake.cs"
        );

        AddTargets(new Target(Platform.win64, DevEnv.vs2026, Optimization.Debug | Optimization.Release));
    }

    [Configure]
    public void ConfigureAll(Configuration configurationInstance, Target targetInstance)
    {
        configurationInstance.SolutionFileName = "[solution.Name]";
        configurationInstance.SolutionPath = @"[solution.SharpmakeCsPath]";

        configurationInstance.AddProject<EngineProject>(targetInstance);
        configurationInstance.AddProject<EditorProject>(targetInstance);
    }
}

public static class MainClass
{
    [Sharpmake.Main]
    public static void SharpmakeMain(Sharpmake.Arguments argumentsInstance)
    {
        argumentsInstance.Generate<HibouEngineSolution>();
    }
}