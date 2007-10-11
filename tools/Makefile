thisdir = tools
SUBDIRS = \
	opentf \
	wit

ifeq ($(HAVE_MONODEVELOP),yes)
SUBDIRS += MonoDevelop.VersionControl.TeamFoundation 
endif

include ../build/rules.make

test-local run-test-local run-test-ondotnet-local all-local install-local uninstall-local:
	@:

dist-local: dist-default

clean-local:
	rm -f *.exe *.dll *.pdb 
