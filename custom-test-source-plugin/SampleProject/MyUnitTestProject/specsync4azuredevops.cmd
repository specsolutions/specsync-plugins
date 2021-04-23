@pushd %~dp0
dotnet tool restore
dotnet specsync %*
@popd