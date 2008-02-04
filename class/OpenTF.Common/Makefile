thisdir = tools/OpenTF.Common
include ../../build/rules.make

LIBRARY = OpenTF.Common.dll

ifeq ($(HAVE_MAGIC),no)
EXTRA_SOURCES += NoFileTypeDatabase.cs
else
EXTRA_SOURCES += FileTypeDatabase.cs
LOCAL_MCS_FLAGS +=  /unsafe -d:HAVE_MAGIC -r:Mono.Posix.dll
endif

ifeq ($(HAVE_GNOME_KEYRING),no)
EXTRA_SOURCES += NoKeyring.cs
else
EXTRA_SOURCES += Keyring.cs
LOCAL_MCS_FLAGS += -pkg:gnome-keyring-sharp -d:HAVE_GNOME_KEYRING
endif

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)

include ../../build/library.make

