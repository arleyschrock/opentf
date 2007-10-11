thisdir = data
SUBDIRS = 
include ../build/rules.make

ifeq ($(PLATFORM), win32)
	tfsdir := $(shell cygpath -m '$(prefix)/lib/mono/2.0')
else
	tfsdir = $(prefix)/lib/mono/2.0
endif

all-local: tfs.pc

REWRITE=
tfs.pc: tfs.pc.in Makefile
	sed -e "s,@tfsdir@,$(tfsdir),g" -e "s,@VERSION@,$(MONO_VERSION),g" < tfs.pc.in > $@.tmp
	mv $@.tmp $@

install-local: tfs.pc
	$(MKINSTALLDIRS) $(DESTDIR)$(prefix)/lib/pkgconfig/
	$(INSTALL_DATA) tfs.pc $(DESTDIR)$(prefix)/lib/pkgconfig/

uninstall-local:
	rm -f $(DESTDIR)$(prefix)/lib/pkgconfig/tfs.pc

clean-local:
	rm -rf tfs.pc

DISTFILES= \
	tfs.pc.in \
	Makefile \
	ChangeLog

dist-local: dist-default

test-local:
run-test-local:
