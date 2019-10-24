
# 1. checking out master; safety-check s.t. that updates can only be applied when you're on the master branch
# 2. override everything in the current upm branch by the new contents (might take a while..)
# 3. push the updated upm branch 

git checkout master && git merge develop --no-edit && git subtree split --prefix Assets/PST/ --branch upm && git push origin upm
