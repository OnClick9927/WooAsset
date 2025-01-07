chcp 65001

git checkout --orphan latest_branch
git add -A
git commit -am 清除提交记录
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