build:
	ellec src/main.le -z -Wl,-rpath,$(HOME)/.local/lib -r -z -O3 -z -lraylib --nogc -o blackhole

run: build
	./blackhole