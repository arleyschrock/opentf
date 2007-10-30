thisdir = class/Microsoft.TeamFoundation.WorkItemTracking.Common
include ../../build/rules.make
SUBDIRS =

LIBRARY = Microsoft.TeamFoundation.WorkItemTracking.Common.dll

LIB_MCS_FLAGS = /r:$(corlib) /r:System.dll /r:System.Data.dll /r:System.Xml.dll /r:System.Web.Services.dll /r:Microsoft.TeamFoundation.Common.dll /r:Microsoft.TeamFoundation.Client.dll
TEST_MCS_FLAGS =  $(LIB_MCS_FLAGS)

include ../../build/library.make

