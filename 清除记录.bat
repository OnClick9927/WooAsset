chcp 65001
@echo off
set /p gd=输入要清除历史提交信息的仓库目录的绝对路径:
echo 待处理的路径：%gd%
set /p gm=输入提交说明：
pushd
cd /d %gd%
git checkout --orphan latest_branch
git add -A
git commit -am "%gm%"
git branch -D main
git branch -m main
git push -f origin main
git pull
echo "已清除全部的历史记录!"
echo "查看新仓库信息："
git log --pretty=oneline
git branch -a
git tag
git ls-remote --tags
pause
popd
exit