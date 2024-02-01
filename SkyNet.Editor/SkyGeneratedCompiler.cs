using SkyNet.Unity.Utils;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using UnityEditor;

[InitializeOnLoad]
internal class SkyGeneratedCompiler
{   
    public static ManualResetEvent Run()
    {
        ManualResetEvent evnt = new ManualResetEvent(false);
        try
        {
            Directory.CreateDirectory(Util.sourceDir);
            RunCSharpCompiler(evnt);
        }
        catch (Exception ex)
        {
            evnt.Set();
            UnityEngine.Debug.LogException(ex);
        }
        return evnt;
    }

    private static void RunCSharpCompiler(ManualResetEvent evnt)
    {
        Process p = new Process();
        p.StartInfo.FileName = Util.monoPath;
        p.StartInfo.Arguments = Util.GenCompilerArgs;
        p.EnableRaisingEvents = true;
        p.StartInfo.CreateNoWindow = true;
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardError = true;
        p.StartInfo.RedirectStandardOutput = true;
        p.ErrorDataReceived += new DataReceivedEventHandler(ErrorDataReceived);
        p.OutputDataReceived += new DataReceivedEventHandler(OutputDataReceived);
        p.Exited += ((s, ea) =>
        {
            evnt.Set();
            SkyNet.Unity.Editor.SkyEditorHousekeeping.Invoke((() =>
            {
                if (p.ExitCode != 0)
                    return;
                CompilationDone();
            }));
        });
        p.Start();
        p.BeginErrorReadLine();
        p.BeginOutputReadLine();
    }

    private static void CompilationDone()
    {
        AssetDatabase.ImportAsset(Util.SkyNetGenAssemblyPath, ImportAssetOptions.ForceUpdate);
        UnityEngine.Debug.Log("Compiler: Success!");
        EditorPrefs.SetInt("SKYNET_UNCOMPILED_COUNT", 0);
        EditorPrefs.SetBool("SKYNET_COMPILE", false);

        string[] allFiles = Directory.GetFiles(Util.SkyNetGenFilesPath, "*.cs", SearchOption.AllDirectories);
        for (int i = 0; i < allFiles.Length; i++)
        {
            File.Delete(allFiles[i]);
        }
    }

    private static void OutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (e.Data == null)
            return;
        UnityEngine.Debug.Log(e.Data);
    }

    private static void ErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (e.Data == null)
            return;
        if (e.Data.Contains(": warning") && !e.Data.Contains(": error"))
            UnityEngine.Debug.LogWarning(e.Data);
        else
            UnityEngine.Debug.LogError(e.Data);
    }
}