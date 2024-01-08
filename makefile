build_img:
	docker build -f dockerfile -t domain-watcher:1.0 .

# make build.linux-x64
# make build.linux-arm64
# make build.win-x64
build.%:
	dotnet publish src/DomainWatcher.Cli --runtime $* --output build/$*
	rm -f build/$*/*.dbg
	rm -f build/$*/*.pdb
	tar czvf build/DomainWatcher.$*.tar.gz -C build/$*/ .
	cd build/$* && zip -r ../DomainWatcher.$*.zip *

build-windows: build.win-x64
build-linux: build.linux-x64
