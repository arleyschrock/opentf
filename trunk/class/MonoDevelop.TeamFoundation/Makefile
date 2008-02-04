thisdir = tools/MonoDevelop.TeamFoundation
include ../../build/rules.make

LIBRARY = MonoDevelop.TeamFoundation.dll

LIB_MCS_FLAGS = /unsafe /pkg:monodevelop /r:$(MONODEVELOP_LIBPATH)/AddIns/VersionControl/MonoDevelop.VersionControl.dll /pkg:gtk-sharp-2.0 /r:Mono.Posix /r:Gtk.TeamFoundation.dll
TEST_MCS_FLAGS =  $(LIB_MCS_FLAGS)

NO_SIGN_ASSEMBLY = 1
LIBRARY_INSTALL_DIR=$(MONODEVELOP_LIBPATH)/AddIns/

include ../../build/library.make

install:
	$(INSTALL_DATA) MonoDevelop.TeamFoundation.addin.xml $(DESTDIR)$(LIBRARY_INSTALL_DIR)/
