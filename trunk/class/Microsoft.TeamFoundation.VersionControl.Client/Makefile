thisdir = class/Microsoft.TeamFoundation.VersionControl.Client
include ../../build/rules.make
SUBDIRS =

LIBRARY = Microsoft.TeamFoundation.VersionControl.Client.dll

LIB_MCS_FLAGS = /r:$(corlib) /r:System.dll /r:System.Xml.dll /r:System.Web.Services.dll /r:Microsoft.TeamFoundation.Common.dll /r:Microsoft.TeamFoundation.Client.dll /r:Microsoft.TeamFoundation.VersionControl.Common.dll
TEST_MCS_FLAGS =  $(LIB_MCS_FLAGS) 

include ../../build/library.make
