thisdir = tools/Gtk.TeamFoundation
include ../../build/rules.make

LIBRARY = Gtk.TeamFoundation.dll

LIB_MCS_FLAGS = /unsafe /pkg:gtk-sharp-2.0 /r:Microsoft.TeamFoundation.dll /r:Microsoft.TeamFoundation.Common.dll /r:Microsoft.TeamFoundation.Client.dll /r:Microsoft.TeamFoundation.VersionControl.Client.dll /r:Microsoft.TeamFoundation.VersionControl.Common.dll

ifeq ($(HAVE_SYNTAX_HIGHLIGHTING),no)
EXTRA_SOURCES += NoSourceView.cs
else
EXTRA_SOURCES += SourceView.cs
LOCAL_MCS_FLAGS += -pkg:gtksourceview-sharp-2.0 -pkg:gnome-vfs-sharp-2.0 -d:HAVE_SYNTAX_HIGHLIGHTING
endif

TEST_MCS_FLAGS =  $(LIB_MCS_FLAGS)

include ../../build/library.make

