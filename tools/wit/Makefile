thisdir = tools/wit
SUBDIRS = 
include ../../build/rules.make

LOCAL_MCS_FLAGS = -r:Microsoft.TeamFoundation.dll -r:Microsoft.TeamFoundation.Common.dll -r:Microsoft.TeamFoundation.Client.dll -r:Microsoft.TeamFoundation.VersionControl.Client.dll -r:Microsoft.TeamFoundation.VersionControl.Common.dll -r:Mono.GetOptions.dll -r:Mono.Posix.dll -r:System.Web.Services.dll

ifeq ($(HAVE_GTK),yes)
EXTRA_SOURCES += $(EXPLORE_SOURCES) Gui.cs
LOCAL_MCS_FLAGS += -pkg:gtk-sharp-2.0 
else
EXTRA_SOURCES += NoGui.cs
endif

ifeq ($(HAVE_GNOME_KEYRING),no)
EXTRA_SOURCES += NoKeyring.cs
else
EXTRA_SOURCES += Keyring.cs
LOCAL_MCS_FLAGS += -pkg:gnome-keyring-sharp
endif

PROGRAM = wit.exe
PROGRAM_INSTALL_DIR=$(prefix)/lib/teamfoundation/

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
	csc /out:wit.exe $(EXTRA_SOURCES) -r:Microsoft.TeamFoundation.dll -r:Microsoft.TeamFoundation.Client.dll -r:Microsoft.TeamFoundation.VersionControl.Client.dll -r:Microsoft.TeamFoundation.VersionControl.Common.dll -r:Microsoft.TeamFoundation.Common.dll -r:Mono.GetOptions.dll -r:System.Configuration.dll @wit.exe.sources
