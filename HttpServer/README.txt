PROJECT: HTTP SERVER

SYNOPSIS:
As always, when I try and learn something new, I really like to get into the nitty gritty of how it works.
As of 27/5/26, my current obsession is learning how the network functions. I am already working on a doucment:

https://docs.google.com/document/d/1k4ArsKjEHohKud0qqYkEl5JVv-3VjsgkGUji3xjkBHs/edit?usp=sharing

But here I want to actually try and implement HTTP. Later down the line, might be go as far as implementing
the TCP/IP server.

STARTING UP PROJECT
To start the project, I did the following:

git init
git add . ( if it complais about a .vs you need to create a gitignore file with the following content:
	.vs/
	bin/
	obj/
	*.user
	*.suo

	then run git rm -r --cached
	then rerun git add .
)

git commit -m "Initial commit"
Create the git repository on GitHub
git remode add origin http://...
git push -u origin master

Done!