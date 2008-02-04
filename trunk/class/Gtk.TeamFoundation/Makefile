thisdir = tools/Gtk.TeamFoundation
include ../../build/rules.make

LIBRARY = Gtk.TeamFoundation.dll
RESOURCES = $(wildcard icons/*.png)

LIB_MCS_FLAGS = /unsafe /pkg:gtk-sharp-2.0 

ifeq ($(HAVE_SYNTAX_HIGHLIGHTING),no)
EXTRA_SOURCES += NoSourceView.cs
else
EXTRA_SOURCES += SourceView.cs
LOCAL_MCS_FLAGS += -pkg:gtksourceview-sharp-2.0 -pkg:gnome-vfs-sharp-2.0 -d:HAVE_SYNTAX_HIGHLIGHTING
endif

res:
	echo $(RESOURCES)

LOCAL_MCS_FLAGS += $(RESOURCES:%=/resource:%)

TEST_MCS_FLAGS =  $(LIB_MCS_FLAGS)

include ../../build/library.make

