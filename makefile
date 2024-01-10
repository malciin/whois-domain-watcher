# e.g. running image after build: 
# docker run --rm -it -p 8053:80 domain-watcher:1.0
image:
	docker build -f dockerfile -t domain-watcher:1.0 .

ui:
	cd src/DomainWatcher.UI && npm i && npm run build

# make build.linux-x64
# make build.linux-arm64
# make build.win-x64
build.%:
	dotnet publish src/DomainWatcher.Cli --runtime $* --output build/$*
	rm -f build/$*/*.dbg
	rm -f build/$*/*.pdb
	cp src/DomainWatcher.UI/dist/index.html build/$*/ui.html
	tar czvf build/DomainWatcher.$*.tar.gz -C build/$*/ .
	cd build/$* && zip -r ../DomainWatcher.$*.zip *

windows: build.win-x64
linux: build.linux-x64
