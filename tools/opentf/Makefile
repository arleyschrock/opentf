thisdir = tools/opentf
SUBDIRS = 
include ../../build/rules.make

LOCAL_MCS_FLAGS = -r:Microsoft.TeamFoundation.dll -r:Microsoft.TeamFoundation.Common.dll -r:Microsoft.TeamFoundation.Client.dll -r:Microsoft.TeamFoundation.VersionControl.Client.dll -r:Microsoft.TeamFoundation.VersionControl.Common.dll -r:Mono.GetOptions.dll -r:Mono.Posix.dll -r:System.Web.Services.dll
EXPLORE_SOURCES = ExploreCommand.cs ExploreView.cs RepositoryView.cs ChangesetView.cs FileView.cs DirectoryView.cs ChangesetDetailView.cs ChangesetDiffView.cs

ifeq ($(HAVE_GTK),yes)
EXTRA_SOURCES += $(EXPLORE_SOURCES) Gui.cs
LOCAL_MCS_FLAGS += -pkg:gtk-sharp-2.0 -d:HAVE_GTK 
ifeq ($(HAVE_SYNTAX_HIGHLIGHTING),no)
EXTRA_SOURCES += NoSourceView.cs
else
EXTRA_SOURCES += SourceView.cs
LOCAL_MCS_FLAGS += -pkg:gtksourceview-sharp-2.0 -pkg:gnome-vfs-sharp-2.0 -d:HAVE_SYNTAX_HIGHLIGHTING
endif
else
EXTRA_SOURCES += NoGui.cs
endif

ifeq ($(HAVE_MAGIC),no)
EXTRA_SOURCES += NoFileType.cs
else
EXTRA_SOURCES += FileType.cs
LOCAL_MCS_FLAGS += -d:HAVE_MAGIC
endif

ifeq ($(HAVE_GNOME_KEYRING),no)
EXTRA_SOURCES += NoKeyring.cs
else
EXTRA_SOURCES += Keyring.cs
LOCAL_MCS_FLAGS += -pkg:gnome-keyring-sharp -d:HAVE_GNOME_KEYRING
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
