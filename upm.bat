@echo off
set b="version"
set version ="1"
REM 获取版本号
for /f "tokens=1,2* delims=:," %%a in (Assets/WooAsset/package.json) do (
    echo %%a| findstr %b% >nul && (
       set version=  %%b
    ) || (
        @REM echo %%a nnn %b%
    )
)


set version=%version: =%
echo on
git subtree split --prefix=Assets/WooAsset --branch src
git push origin src:src
git tag %version% src
git push origin src --tags
set cur=%~dp0
set srcPath=%cur%Assets\WooAsset
set dstPath=%cur%WooAssetForUPM
rd /s /q %dstPath%\Editor
rd /s /q %dstPath%\Runtime

echo off
xcopy /S /Y %srcPath% %dstPath%
cd WooAssetForUPM
echo on
git add .
git commit -m %version%
git push origin main:main
git tag %version% main
git push origin main --tags

pause


