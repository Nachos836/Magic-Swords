
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

CLEAR = clear && echo -en "\e[3J"

COMPILE_OBJ_RULE += -DUSE_WAYLAND_API=ON
COMPILE_APP_RULE += -DUSE_WAYLAND_API=ON
