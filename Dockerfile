FROM mcr.microsoft.com/dotnet/sdk:8.0
WORKDIR /App

# Copy everything
COPY . ./
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet pack FSharpPacker.FSharp -c Release
RUN dotnet tool install FSharpPacker --global --add-source FSharpPacker.FSharp/bin/Release/
ENV PATH "$PATH:/root/.dotnet/tools"

ENTRYPOINT ["/bin/bash"]
