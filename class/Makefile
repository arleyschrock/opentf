thisdir = class

include ../build/rules.make

SUBDIRS = \
	Microsoft.TeamFoundation.Common \
	Microsoft.TeamFoundation \
	Microsoft.TeamFoundation.Client \
	Microsoft.TeamFoundation.VersionControl.Common \
	Microsoft.TeamFoundation.VersionControl.Client \
	Microsoft.TeamFoundation.WorkItemTracking.Common \
	Microsoft.TeamFoundation.WorkItemTracking.Client

DISTFILES = \
	LICENSE

.PHONY: all-local $(STD_TARGETS:=-local)
all-local $(STD_TARGETS:=-local):
	@:

dist-local: dist-default
#	$(MKINSTALLDIRS) $(distdir)/deps
