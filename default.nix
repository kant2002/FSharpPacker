{ pkgs, src }:
pkgs.buildDotnetModule rec {
  inherit src;
  
  pname = "fspack";
  name = "fspack";
  

  projectFile = "FSharpPacker.FSharp/FSharpPacker.FSharp.fsproj";
  testProjectFile = "FSharpPacker.Tests/FSharpPacker.Tests.csproj";
  nugetDeps = ./deps.nix;

  doCheck = true;
  dotnet-sdk = pkgs.dotnetCorePackages.sdk_6_0;
  dotnet-runtime = pkgs.dotnetCorePackages.aspnetcore_6_0;

  runtimeDeps = [ 
      pkgs.dotnetCorePackages.sdk_6_0
  ];
  postPatch = ''
      # this fixes "dotnet" path
      substituteInPlace \
        FSharpPacker.FSharp/Program.fs \
        --replace \
        'Process.Start("dotnet"' \
        'Process.Start("${pkgs.dotnetCorePackages.sdk_6_0}/bin/dotnet"'
    '';
#  meta = with lib; {
#    description = "Tool for packaging FSX files as self-contained executables";
#    homepage = "https://github.com/kant2002/FSharpPacker";
#    maintainers = [ maintainers.abbradar ];
#  };
    
  executables = [ "FSharpPacker.FSharp" ];
}
