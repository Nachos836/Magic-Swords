
#
# PROJ_CC
# DB
#
# FLAG
# BUILD
#
# COMPILE_APP_RULE
# COMPILE_OBJ_RULE
#
# ASAN
# P_FUNCTION 
#

#
# Konsole clear routine - clear && echo -en "\e[3J"
# Otherwise try "reset" or use whatever u want
#
ifeq ($(OS_DETECT),$(OS_LINUX))
	CLEAR = clear && echo -en "\e[3J"
	COMPILE_APP_RULE += -lwayland-client -lwayland-cursor
else ifeq ($(OS_DETECT),$(OS_OSX))
	CLEAR = clear && printf "\e[3J"
	COMPILE_APP_RULE += -framework metal -framework cocoa -framework metal-utils
else ifeq ($(OS_DETECT),$(OS_WINDOWS))
	CLEAR = clean"
	COMPILE_APP_RULE += -lgdi32
endif