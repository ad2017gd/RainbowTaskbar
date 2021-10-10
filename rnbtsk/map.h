#pragma once

// cant believe i made a fucking map library when better others probably and surely exist
// atleast its only a header ig


#define MAP_MAXKEY 48
#define MAP_REALLOC 32
#define MAP_UNUSED "M__UNUSED__"
#define MAP_STR(s) #s

int MAP_TEMP;

// Define a map.
#define MapDef(name,type) \
unsigned int __ ## name ## _len = 0; \
char* __ ## name ## _type = MAP_STR(type); \
type * __ ## name ## _values; \
char** __ ## name ## _keys; \
unsigned int __ ## name ## _size = 0;\
unsigned int __ ## name ## _sizeof = sizeof(type);

// Get map array.
#define MapArray(name) __ ## name ## _values

// Get map length.
#define MapLength(name) __ ## name ## _len


// Initialize a map.
#define MapInit(name) \
__ ## name ## _size = MAP_REALLOC; \
__ ## name ## _values = malloc(MAP_REALLOC * __ ## name ## _sizeof); \
memset(__ ## name ## _values, 0, MAP_REALLOC * __ ## name ## _sizeof); \
__ ## name ## _keys = malloc(MAP_REALLOC * MAP_MAXKEY); \
memset(__ ## name ## _keys, 0, MAP_REALLOC * MAP_MAXKEY); \
\

// Deletes and frees a map.
#define MapDelete(name) \
__ ## name ## _size = 0; \
__ ## name ## _len = 0; \
free(__ ## name ## _values); \
__ ## name ## _values = NULL; \
free(__ ## name ## _keys); \
__ ## name ## _keys = NULL; \
\

// Gets value by key. Max key length is defined as MAP_MAXKEY.
#define MapGet(name,key,var) \
var = 0; \
for(int i = 0; i < __ ## name ## _size; i++){ \
 if( __ ## name ## _keys[i] == NULL) continue; \
 if( !strcmp(__ ## name ## _keys[i], key) ){\
  var = __ ## name ## _values[i]; \
 }\
}\
\

// Get index of value in array by key. Max key length is defined as MAP_MAXKEY.
#define MapGetIdx(name,key,var) \
var = -1; \
for(int i = 0; i < __ ## name ## _size; i++){ \
 if( __ ## name ## _keys[i] == NULL) continue; \
 if( !strcmp(__ ## name ## _keys[i], key) ){\
  var = i; \
 }\
}\
\

// Add key-value pair to a map. Max key length is defined as MAP_MAXKEY.
// todo: actually get reallocation to work?
#define MapAdd(name,key,value) \
if(__ ## name ## _len >= __ ## name ## _size){ \
realloc(__ ## name ## _values, (__ ## name ## _size + MAP_REALLOC) * __ ## name ## _sizeof ); \
realloc(__ ## name ## _keys, (__ ## name ## _size + MAP_REALLOC)*MAP_MAXKEY);\
__ ## name ## _size += MAP_REALLOC; \
}; \
MapGetIdx(name , "M__UNUSED__" , MAP_TEMP); \
if (MAP_TEMP != -1) { \
 __ ## name ## _values[ MAP_TEMP ] = value; \
 __ ## name ## _keys[ MAP_TEMP ] = key; \
MAP_TEMP = -1; \
} else { \
__ ## name ## _values[ __ ## name ## _len ] = value; \
__ ## name ## _keys[ __ ## name ## _len ] = key; \
} \
__ ## name ## _len++;

// Remove key-value pair from a map. Max key length is defined as MAP_MAXKEY.
#define MapRemove(name,key) \
for(int i = 0; i < __ ## name ## _size; i++){ \
 if( __ ## name ## _keys[i] == NULL) continue; \
 if( !strcmp(__ ## name ## _keys[i], key) ){\
  __ ## name ## _keys[i] = "M__UNUSED__"; \
  __ ## name ## _values[i] = NULL; \
  __ ## name ## _len--; \
 }\
}\
\

// Modify existing key-value pair from a map. Max key length is defined as MAP_MAXKEY.
#define MapSet(name,key,value) \
for(int i = 0; i < __ ## name ## _size; i++){ \
 if( __ ## name ## _keys[i] == NULL) continue; \
 if( !strcmp(__ ## name ## _keys[i], key) ){\
  __ ## name ## _values[i] = value; \
 }\
}\
\

// Search and retrieve key name from value. Max key length is defined as MAP_MAXKEY.
#define MapFind(name,value,var) \
 var = MAP_UNUSED; \
for(int i = 0; i < __ ## name ## _size; i++){ \
 if( __ ## name ## _keys[i] == NULL) continue; \
 if( __ ## name ## _values[i] == 0){ \
  var = __ ## name ## _keys[i]; \
 } else \
 if( !strcmp( __ ## name ## _type , "char*") || !strcmp( __ ## name ## _type , "char *") || !strcmp( __ ## name ## _type , "LPTSTR") || !strcmp( __ ## name ## _type , "LPCSTR") || !strcmp( __ ## name ## _type , "LPWSTR")){ \
  if( !strcmp(__ ## name ## _values[i], value) ){\
   var = __ ## name ## _keys[i]; \
  }\
 }\
 else {\
  if( __ ## name ## _values[i] == value ){\
   var = __ ## name ## _keys[i]; \
  }\
 }\
}\

// Search and retrieve index from value. Max key length is defined as MAP_MAXKEY.
#define MapFindIdx(name,value,var) \
var = -1; \
for(int i = 0; i < __ ## name ## _size; i++){ \
 if( __ ## name ## _keys[i] == NULL) continue; \
 if( __ ## name ## _values[i] == 0){ \
  if( __ ## name ## _values[i] == value ){\
   var = i; \
  }\
 } else \
 if( !strcmp( __ ## name ## _type , "char*") || !strcmp( __ ## name ## _type , "char *") || !strcmp( __ ## name ## _type , "LPTSTR") || !strcmp( __ ## name ## _type , "LPCSTR") || !strcmp( __ ## name ## _type , "LPWSTR")){ \
     if( !strcmp(__ ## name ## _values[i], value) ){\
   var = i; \
  }\
 }\
 else {\
  if( __ ## name ## _values[i] == value ){\
   var = i; \
  }\
 }\
}\

// Iterate every key-value pair from map. Use instead of MapArray if deleting items.
#define MapIterate(name,key,value,idx,code) \
idx = 0; \
key = MAP_UNUSED; \
for(int i = 0; i < __ ## name ## _size; i++){ \
 if( __ ## name ## _keys[i] == NULL) {continue;} \
 if( strcmp(__ ## name ## _keys[i], MAP_UNUSED) ) { \
  value = __ ## name ## _values[i]; \
  key = __ ## name ## _keys[i]; \
  idx = i; \
  code \
 } \
} \
