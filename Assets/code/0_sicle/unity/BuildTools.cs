using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Diagnostics;

public class BuildTools
{
    const int VERSION_MAJOR = 0;
    const int VERSION_MINOR = 0;
    const int VERSION_RELEASE = 2;

    static public string VERSION_STRING {
        get { return string.Format( "{0}.{1}.{2}", VERSION_MAJOR, VERSION_MINOR, VERSION_RELEASE ); }
    }

    const string FILENAME = "/sum-of-its-parts.exe";
    static readonly string[] LEVELS = {
        "Assets/scenes/title.unity",
        "Assets/scenes/main.unity"
    };

#if UNITY_EDITOR

    [MenuItem( "Build/Windows x64" )]
    public static void BuildWindows64() {
        BuildWindows64( BuildOptions.None, "latest" );
    }

    [MenuItem( "Build/Windows x64 and Run" )]
    public static void BuildWindowsAndRun() {
        BuildWindows64( BuildOptions.AutoRunPlayer, "latest" );
    }

    [MenuItem( "Build/All Latest" )]
    public static void BuildLatest() { BuildAll( "latest" ); }

    [MenuItem( "Build/All Releases" )]
    public static void BuildRelease() { BuildAll( VERSION_STRING ); }

    [MenuItem( "Upload/All Latest" )]
    public static void UploadLatest() { Upload( "latest" ); }

    [MenuItem( "Upload/Most Recent Releases" )]
    public static void UploadRelease() { Upload( VERSION_STRING ); }

    private static void Upload( string a_prefix ) {
        Butler( "push build/" + a_prefix + "_windows64 joshua-mclean/ld40:" + a_prefix + "-windows64" );
        Butler( "push build/" + a_prefix + "_windows32 joshua-mclean/ld40:" + a_prefix + "-windows32" );
        Butler( "push build/" + a_prefix + "_osx-universal joshua-mclean/ld40:" + a_prefix + "-osx-universal" );
        Butler( "push build/" + a_prefix + "_linux-universal joshua-mclean/ld40:" + a_prefix + "-linux-universal" );
    }

    private static void BuildAll( string a_prefix ) {
        BuildWindows64( BuildOptions.None, a_prefix );
        BuildWindows32( a_prefix );
        BuildLinuxUniversal( a_prefix );
        BuildOsxUniversal( a_prefix );
    }

    private static void BuildLinuxUniversal( string a_prefix ) {
        string path = "build/" + a_prefix + "_linux-universal";
        BuildPipeline.BuildPlayer( LEVELS, path + FILENAME, BuildTarget.StandaloneLinuxUniversal, BuildOptions.None );
        CopyFiles( path );
    }

    private static void BuildOsxUniversal( string a_prefix ) {
        string path = "build/" + a_prefix + "_osx-universal";
        BuildPipeline.BuildPlayer( LEVELS, path + FILENAME, BuildTarget.StandaloneOSXUniversal, BuildOptions.None );
        CopyFiles( path );
    }

    private static void BuildWindows32( string a_prefix ) {
        string path = "build/" + a_prefix + "_windows32";
        BuildPipeline.BuildPlayer( LEVELS, path + FILENAME, BuildTarget.StandaloneWindows, BuildOptions.None );
        CopyFiles( path );
    }

    private static void BuildWindows64( BuildOptions a_options, string a_prefix ) {
        string path = "build/" + a_prefix + "_windows64";
        BuildPipeline.BuildPlayer( LEVELS, path + FILENAME, BuildTarget.StandaloneWindows64, a_options );
        CopyFiles( path );
    }

    private static void CopyFiles( string a_path ) {
        FileUtil.ReplaceFile( "README.md", a_path + "/README.md" );
        FileUtil.ReplaceFile( "changelog.md", a_path + "/changelog.md" );
    }

    private static void Butler( string args ) {
        var info = new ProcessStartInfo( "butler", args );
        var process = Process.Start( info );

        process.OutputDataReceived += ( object s, DataReceivedEventArgs e ) => {
            UnityEngine.Debug.Log( "Butler: " + e.Data );
        };
        process.ErrorDataReceived += ( object s, DataReceivedEventArgs e ) => {
            UnityEngine.Debug.LogError( "Butler: " + e.Data );
        };

        process.WaitForExit();
    }

#endif // UNITY_EDITOR
}
