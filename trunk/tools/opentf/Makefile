thisdir = tools/opentf
SUBDIRS = 
include ../../build/rules.make

ifeq ($(HAVE_GTK),yes)
EXTRA_SOURCES += ExploreCommand.cs Gui.cs
LOCAL_MCS_FLAGS += -pkg:gtk-sharp-2.0 -r:Gtk.TeamFoundation.dll -d:HAVE_GTK 
else
EXTRA_SOURCES += NoGui.cs
endif

PROGRAM = opentf.exe
PROGRAM_INSTALL_DIR=$(prefix)/lib/opentf/

install: 
	$(MKINSTALLDIRS) $(DESTDIR)$(prefix)/bin/
	$(INSTALL_BIN) tf.sh $(DESTDIR)$(prefix)/bin/tf

uninstall:
	-rm -f $(DESTDIR)$(prefix)/bin/tf

include ../../build/executable.make

cp-mslibs:
	cp /cygdrive/c/Program\ Files/Microsoft\ Visual\ Studio\ 8/Common7/IDE/PrivateAssemblies/Microsoft.TeamFoundation.* .

ondotnet:
	csc /out:opentf.exe $(EXTRA_SOURCES) -r:Microsoft.TeamFoundation.dll -r:Microsoft.TeamFoundation.Client.dll -r:Microsoft.TeamFoundation.VersionControl.Client.dll -r:Microsoft.TeamFoundation.VersionControl.Common.dll -r:Microsoft.TeamFoundation.Common.dll -r:Mono.GetOptions.dll -r:System.Configuration.dll @opentf.exe.sources
