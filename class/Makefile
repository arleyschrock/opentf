thisdir = class

include ../build/rules.make

SUBDIRS = \
	Microsoft.TeamFoundation.Common \
	Microsoft.TeamFoundation \
	Microsoft.TeamFoundation.Client \
	Microsoft.TeamFoundation.VersionControl.Common \
	Microsoft.TeamFoundation.VersionControl.Client \
	Microsoft.TeamFoundation.WorkItemTracking.Common \
	Microsoft.TeamFoundation.WorkItemTracking.Client \
	OpenTF.Common 

ifeq ($(HAVE_GTK),yes)
SUBDIRS += Gtk.TeamFoundation
endif

ifeq ($(HAVE_MONODEVELOP),yes)
SUBDIRS += MonoDevelop.TeamFoundation
endif

DISTFILES = \
	LICENSE

.PHONY: all-local $(STD_TARGETS:=-local)
all-local $(STD_TARGETS:=-local):
	@:

dist-local: dist-default
#	$(MKINSTALLDIRS) $(distdir)/deps
