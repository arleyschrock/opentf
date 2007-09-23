thisdir = class/Microsoft.TeamFoundation.WorkItemTracking.Client
include ../../build/rules.make
SUBDIRS =

LIBRARY = Microsoft.TeamFoundation.WorkItemTracking.Client.dll

LIB_MCS_FLAGS = /r:$(corlib) /r:System.dll /r:System.Xml.dll /r:System.Web.Services.dll /r:Microsoft.TeamFoundation.Common.dll /r:Microsoft.TeamFoundation.Client.dll
TEST_MCS_FLAGS =  $(LIB_MCS_FLAGS)

include ../../build/library.make

