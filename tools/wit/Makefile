thisdir = tools/wit
SUBDIRS = 
include ../../build/rules.make

LOCAL_MCS_FLAGS = -r:Mono.Posix.dll

PROGRAM = wit.exe
PROGRAM_INSTALL_DIR=$(prefix)/lib/opentf/

install: 
	$(MKINSTALLDIRS) $(DESTDIR)$(prefix)/bin/
	$(INSTALL_BIN) wit.sh $(DESTDIR)$(prefix)/bin/wit

uninstall:
	-rm -f $(DESTDIR)$(prefix)/bin/wit

include ../../build/executable.make

# if you want to run the tf.exe against the MS versions of these
# assemblies you might find these targets useful

cp-mslibs:
	cp /cygdrive/c/Program\ Files/Microsoft\ Visual\ Studio\ 8/Common7/IDE/PrivateAssemblies/Microsoft.TeamFoundation.* .

ondotnet:
	csc /out:wit.exe -r:Microsoft.TeamFoundation.dll -r:Microsoft.TeamFoundation.Client.dll -r:Microsoft.TeamFoundation.VersionControl.Client.dll -r:Microsoft.TeamFoundation.VersionControl.Common.dll -r:Microsoft.TeamFoundation.Common.dll -r:Mono.GetOptions.dll -r:System.Configuration.dll -r:Microsoft.TeamFoundation.WorkItemTracking.Client.dll @wit.exe.sources
