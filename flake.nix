{
  description = "Tool for packaging FSX files as self-contained executables";

  inputs = {
    nixpkgs.url = "nixpkgs/nixos-22.05";
    flake-utils.url = "github:numtide/flake-utils";
  };

  outputs = { self, nixpkgs, flake-utils }:
    flake-utils.lib.eachDefaultSystem (system:
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
          fspack = flake-utils.lib.mkApp { drv = derivation; };
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