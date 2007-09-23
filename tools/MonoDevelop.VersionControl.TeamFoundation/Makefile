thisdir = tools/MonoDevelop.VersionControl.TeamFoundation
include ../../build/rules.make

LIBRARY = MonoDevelop.VersionControl.TeamFoundation.dll

LIB_MCS_FLAGS = /unsafe /pkg:monodevelop /r:$(MONODEVELOP_LIBPATH)/monodevelop/AddIns/VersionControl/VersionControlAddIn.dll /pkg:gtk-sharp-2.0 /r:Mono.Posix /r:Microsoft.TeamFoundation.dll /r:Microsoft.TeamFoundation.Common.dll /r:Microsoft.TeamFoundation.Client.dll /r:Microsoft.TeamFoundation.VersionControl.Client.dll /r:Microsoft.TeamFoundation.VersionControl.Common.dll
TEST_MCS_FLAGS =  $(LIB_MCS_FLAGS)

NO_SIGN_ASSEMBLY = 1
LIBRARY_INSTALL_DIR=$(MONODEVELOP_LIBPATH)/monodevelop/AddIns/VersionControl

include ../../build/library.make

install:
	$(INSTALL_DATA) MonoDevelop.VersionControl.TeamFoundation.addin.xml $(LIBRARY_INSTALL_DIR)/