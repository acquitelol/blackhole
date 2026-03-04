blackhole: build
	./$@

build:
	ellec src/main.le -z -Wl,-rpath,$(HOME)/.local/lib -z -lraylib --nogc -o blackhole
