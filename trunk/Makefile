thisdir := .

SUBDIRS := build

net_2_0_SUBDIRS := $(SUBDIRS)

include build/config.make
include build/rules.make

ifeq ($(HAVE_MSLIBS),no)
PROFILE_SUBDIRS += class
endif

ifeq ($(HAVE_DOC_TOOLS),yes)
PROFILE_SUBDIRS += docs
endif

PROFILE_SUBDIRS += tools data

all-recursive $(STD_TARGETS:=-recursive): platform-check profile-check

.PHONY: all-local $(STD_TARGETS:=-local)
all-local $(STD_TARGETS:=-local):
	@:

# fun specialty targets

PROFILES = default net_2_0

.PHONY: all-profiles $(STD_TARGETS:=-profiles)
all-profiles $(STD_TARGETS:=-profiles): %-profiles: profiles-do--%
	@:

profiles-do--%:
	$(MAKE) $(PROFILES:%=profile-do--%--$*)

# The % below looks like profile-name--target-name
profile-do--%:
	$(MAKE) PROFILE=$(subst --, ,$*)

# We don't want to run the tests in parallel.  We want behaviour like -k.
profiles-do--run-test:
	ret=:; $(foreach p,$(PROFILES), { $(MAKE) PROFILE=$(p) run-test || ret=false; }; ) $$ret

# Orchestrate the bootstrap here.
_boot_ = all clean install
$(_boot_:%=profile-do--net_2_0--%):           profile-do--net_2_0--%:           profile-do--net_2_0_bootstrap--%
$(_boot_:%=profile-do--net_2_0_bootstrap--%): profile-do--net_2_0_bootstrap--%: profile-do--default--%
$(_boot_:%=profile-do--default--%):           profile-do--default--%:           profile-do--net_1_1_bootstrap--%
$(_boot_:%=profile-do--net_1_1_bootstrap--%): profile-do--net_1_1_bootstrap--%: profile-do--basic--%

compiler-tests:
	$(MAKE) TEST_SUBDIRS="tests errors" run-test-profiles

test-installed-compiler:
	$(MAKE) TEST_SUBDIRS="tests errors" PROFILE=default TEST_RUNTIME=mono MCS=mcs run-test
	$(MAKE) TEST_SUBDIRS="tests errors" PROFILE=net_2_0 TEST_RUNTIME=mono MCS=gmcs run-test

dist:
	git-archive --format=tar --prefix=opentf-$(PACKAGE_VERSION)/ master . |gzip > ../opentf-$(PACKAGE_VERSION).tgz
	cp docs/tf.html ../tf-$(PACKAGE_VERSION).html

dist.deb:
	dpkg-buildpackage -rfakeroot -uc -b

dist.reset:
	./configure --prefix=/usr --enable-highlight
	make clean all

dist.rpm:
	rpmbuild -ba tfs.spec

# ok, now this is ugly
dist.win:
	./configure --disable-magic --disable-keyring --disable-gtk
	make clean all
	makensis tfs.nsi
	cp opentf.exe ../opentf-base-$(PACKAGE_VERSION).exe
	./configure --disable-magic --disable-keyring --enable-highlight
	(cd tools/tf &&	make clean all)
	makensis tfs.nsi
	cp opentf.exe ../opentf-full-$(PACKAGE_VERSION).exe

distclean: clean

clean:
	rm -rf debian/tmp debian/opentf-client debian/libopentf-cil \
	opentf.exe tfs.spec

sample:
	gmcs sample.cs -r:Microsoft.TeamFoundation.dll -r:Microsoft.TeamFoundation.Client.dll -r:Microsoft.TeamFoundation.VersionControl.Client.dll -r:Microsoft.TeamFoundation.VersionControl.Common.dll -r:Microsoft.TeamFoundation.Common.dll

# if you want to run the unit tests against the MS versions of these
# assemblies you might find these targets useful

cp-mslibs:
	cp /cygdrive/c/Program\ Files/Microsoft\ Visual\ Studio\ 8/Common7/IDE/PrivateAssemblies/Microsoft.TeamFoundation.* class/lib/net_2_0

sample-ondotnet:
	csc sample.cs -r:Microsoft.TeamFoundation.dll -r:Microsoft.TeamFoundation.Client.dll -r:Microsoft.TeamFoundation.VersionControl.Client.dll -r:Microsoft.TeamFoundation.VersionControl.Common.dll -r:Microsoft.TeamFoundation.Common.dll
