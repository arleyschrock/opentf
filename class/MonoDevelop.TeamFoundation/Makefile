thisdir = tools/MonoDevelop.TeamFoundation
include ../../build/rules.make

LIBRARY = MonoDevelop.TeamFoundation.dll

LIB_MCS_FLAGS = /unsafe /pkg:monodevelop /r:$(MONODEVELOP_LIBPATH)/monodevelop/AddIns/VersionControl/VersionControlAddIn.dll /pkg:gtk-sharp-2.0 /r:Mono.Posix /r:Microsoft.TeamFoundation.dll /r:Microsoft.TeamFoundation.Common.dll /r:Microsoft.TeamFoundation.Client.dll /r:Microsoft.TeamFoundation.VersionControl.Client.dll /r:Microsoft.TeamFoundation.VersionControl.Common.dll /r:Gtk.TeamFoundation.dll
TEST_MCS_FLAGS =  $(LIB_MCS_FLAGS)

NO_SIGN_ASSEMBLY = 1
LIBRARY_INSTALL_DIR=$(MONODEVELOP_LIBPATH)/monodevelop/AddIns/

include ../../build/library.make

install:
	$(INSTALL_DATA) MonoDevelop.TeamFoundation.addin.xml $(LIBRARY_INSTALL_DIR)/
