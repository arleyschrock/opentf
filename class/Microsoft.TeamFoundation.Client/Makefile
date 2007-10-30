thisdir = class/Microsoft.TeamFoundation.Client
include ../../build/rules.make
SUBDIRS =

LIBRARY = Microsoft.TeamFoundation.Client.dll

LIB_MCS_FLAGS = /r:$(corlib) /r:System.dll /r:System.Xml.dll /r:System.Web.Services.dll /r:../lib/net_2_0/Microsoft.TeamFoundation.dll /r:../lib/net_2_0/Microsoft.TeamFoundation.Common.dll
TEST_MCS_FLAGS =  $(LIB_MCS_FLAGS)

include ../../build/library.make

