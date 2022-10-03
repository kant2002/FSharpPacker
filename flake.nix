{
  description = "Tool for packaging FSX files as self-contained executables";

  inputs = {
    nixpkgs.url = "nixpkgs/nixos-22.05";
    flake-utils.url = "github:numtide/flake-utils";
  };

  outputs = { self, nixpkgs, flake-utils }:
    flake-utils.lib.eachSystem ([
      flake-utils.lib.system.x86_64-linux
      flake-utils.lib.system.aarch64-darwin
      flake-utils.lib.system.x86_64-darwin 
    ]) (system:
      let
        pkgs = import nixpkgs { inherit system; };
        fspack = import ./default.nix {
          inherit pkgs;
          src = builtins.path { path = ./.; };
        };
        derivation = { inherit fspack; };
      in rec
      {
        packages = rec {
          inherit fspack;
          default = fspack;
        };
        apps = rec {
          fspack = flake-utils.lib.mkApp { drv = derivation.fspack; exePath = "/bin/FSharpPacker.FSharp"; };
          default = fspack;
        };
        shell = pkgs.mkShell {
          name = "dotnet-env";
          packages = [
            pkgs.dotnet-sdk
          ];
        };
      }
    );
}