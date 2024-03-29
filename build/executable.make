# -*- makefile -*-
#
# The rules for building a program.

base_prog = $(shell basename $(PROGRAM))
sourcefile = $(base_prog).sources
refsfile = $(base_prog).references

base_prog_config := $(wildcard $(base_prog).config)
ifdef base_prog_config
PROGRAM_config := $(PROGRAM).config
endif

executable_CLEAN_FILES = *.exe $(PROGRAM) $(PROGRAM).mdb $(BUILT_SOURCES)

ifeq (cat,$(PLATFORM_CHANGE_SEPARATOR_CMD))
response = $(sourcefile)
else
response = $(depsdir)/$(base_prog).response
executable_CLEAN_FILES += $(response)
endif

makefrag = $(depsdir)/$(PROFILE)_$(base_prog).makefrag
pdb = $(patsubst %.exe,%.pdb,$(PROGRAM))
mdb = $(patsubst %.exe,%.mdb,$(PROGRAM))
csproj = $(patsubst %.exe,%.csproj,$(PROGRAM))

executable_CLEAN_FILES += $(makefrag) $(pdb) $(mdb) $(sourcefile) $(refsfile)

$(sourcefile): $(csproj)
	xsltproc -o $@ $(topdir)/build/sources.xsl $< 

$(refsfile): $(csproj)
	xsltproc -o $@ $(topdir)/build/references.xsl $< 

all-local: $(PROGRAM) $(PROGRAM_config)

install-local: all-local
test-local: all-local
uninstall-local:

ifdef NO_INSTALL
install-local uninstall-local:
	@:
else

ifndef PROGRAM_INSTALL_DIR
PROGRAM_INSTALL_DIR = $(prefix)/bin
endif

install-local: $(PROGRAM) $(PROGRAM_config)
	$(MKINSTALLDIRS) $(DESTDIR)$(PROGRAM_INSTALL_DIR)
	$(INSTALL_BIN) $(PROGRAM) $(DESTDIR)$(PROGRAM_INSTALL_DIR)
	test ! -f $(PROGRAM).mdb || $(INSTALL_BIN) $(PROGRAM).mdb $(DESTDIR)$(PROGRAM_INSTALL_DIR)
ifdef PROGRAM_config
	$(INSTALL_DATA) $(PROGRAM_config) $(DESTDIR)$(PROGRAM_INSTALL_DIR)
endif

uninstall-local:
	-rm -f $(DESTDIR)$(PROGRAM_INSTALL_DIR)/$(base_prog) $(DESTDIR)$(PROGRAM_INSTALL_DIR)/$(base_prog).mdb $(DESTDIR)$(PROGRAM_INSTALL_DIR)/$(base_prog).config
endif

clean-local:
	-rm -f $(executable_CLEAN_FILES) $(CLEAN_FILES)

test-local:
	@:
run-test-local:
	@:
run-test-ondotnet-local:
	@:

DISTFILES = $(base_prog_config) $(EXTRA_DISTFILES)

dist-local: dist-default $(sourcefile)
	for f in `cat $(sourcefile)` ; do \
	  case $$f in \
	  ../*) : ;; \
	  *) dest=`dirname $$f` ; \
	     case $$subs in *" $$dest "*) : ;; *) subs=" $$dest$$subs" ; $(MKINSTALLDIRS) $(distdir)/$$dest ;; esac ; \
	     cp -p $$f $(distdir)/$$dest || exit 1 ;; \
	  esac ; done ; \
	for d in . $$subs ; do \
	  case $$d in .) : ;; *) test ! -f $$d/ChangeLog || cp -p $$d/ChangeLog $(distdir)/$$d ;; esac ; done

ifndef PROGRAM_COMPILE
PROGRAM_COMPILE = $(CSCOMPILE)
endif

$(PROGRAM): $(BUILT_SOURCES) $(EXTRA_SOURCES) $(response) $(refsfile)
	$(PROGRAM_COMPILE) -target:exe -out:$(base_prog) $(BUILT_SOURCES) $(EXTRA_SOURCES) @$(refsfile) @$(response)
ifneq ($(base_prog),$(PROGRAM))
	mv $(base_prog) $(PROGRAM)
	test ! -f $(base_prog).mdb || mv $(base_prog).mdb $(PROGRAM).mdb
endif

ifdef PROGRAM_config
ifneq ($(base_prog_config),$(PROGRAM_config))
executable_CLEAN_FILES += $(PROGRAM_config)
$(PROGRAM_config): $(base_prog_config)
	cp $(base_prog_config) $(PROGRAM_config)
endif
endif

$(makefrag): $(sourcefile) 
	@echo Creating $@ ...
	@sed 's,^,$(PROGRAM): ,' $< > $@

ifneq ($(response),$(sourcefile))
$(response): $(sourcefile)
	@echo Creating $@ ...
	@( $(PLATFORM_CHANGE_SEPARATOR_CMD) ) <$< >$@
endif

-include $(makefrag)

all-local: $(makefrag)
$(makefrag): $(topdir)/build/executable.make
