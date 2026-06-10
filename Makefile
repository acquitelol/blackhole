ELLE_LIBS ?= -lraylib -Wl,-rpath,$(HOME)/.local/lib
ELLE_FLAGS ?= -o blackhole -t -r -z -O3

default: blackhole

.PHONY: run
run: blackhole
	./$<

blackhole: src/main.le
	ellec src/main.le $(ELLE_FLAGS) $(foreach L,$(ELLE_LIBS),-z $(L))

