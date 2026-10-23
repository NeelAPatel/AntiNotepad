# Builds and renders every variant into out/
dotnet build -c Debug -nologo -v q
foreach ($v in 'ink','paper','terminal','slate','midnight','carousel') { dotnet bin/Debug/net10.0/UiVariants.dll --render $v "out/$v.png" }
